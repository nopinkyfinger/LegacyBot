using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

/// <summary>
/// This is still in the main namespace, but the sub-namespace is "Networking".
/// So, if we want to use this, we have to add a "using" to the the file we want to 
///  use it in: using lulzbot.Networking
///  Later on, packets will be kept in this namespace as well. (Yes, packets will be done 
///   the OOP way.)
/// </summary>
namespace lulzbot.Networking
{
    /// <summary>
    /// This will be our socket wrapper class.
    /// While, arguably, there's no great reason to really have one, I think it's a good idea.
    /// It makes things a tad cleaner, and also allows for modification of the methods later on,
    ///  without the need for modifying every single usagee. Such is the beauty of OOP design.
    /// </summary>

    public class SocketWrapper
    {
        // This is private so it can only be accessed within the class.
        // This is a wrapper, not a placeholder. They shouldn't be able to
        //  access this via anything but class methods.
        private Socket _socket;

        // This is our buffer length
        private int _buffer_length = 8192;
        // This is our receive buffer. There's no reason for this to be public.
        private byte[] _buffer, temp = new byte[1];
        private String _packet = String.Empty;

        // Close override
        private bool Closed = true;
        private bool IsConnected
        {
            get
            {
                try
                {
                    return !(_socket.Poll(1, SelectMode.SelectRead) && _socket.Available == 0);
                }
                catch { return false; }
            }
        }

        public static int Reconnects = 0;

        // We need to check for timeouts.
        private Timer timeout_timer;
        private ulong LastPacket = Bot.EpochTimestampMS;

        // Our server variables
        private String _host;
        private IPAddress _ip;
        private int _port;
        private EndPoint _endpoint;

        // In certain cases, we get segmented packets. That's just how TCP works. Now, there are
        //  a few ways to handle this. With dAmn, you can just read until \0 and call it a packet.
        //  But that's not really considered the "correct" way to go about it. It's also inefficient,
        //  when you compare it to just grabbing chunks of data and processing it. So. we basically 
        //  want to use a packet queue instead. We just grab complete packets from the buffer, and add
        //  them to the queue. After which, they're processed from there on. Simple enough?
        private Queue<dAmnPacket> _packet_queue;
        private Queue<byte[]> _out_queue;

        public void Connect (String host, int port)
        {
            try
            {
                // Store the host and port
                _host = host;
                _port = port;

                // Empty the buffer and allocate it
                _buffer = new byte[_buffer_length];

                // Initialize the packet queue
                _packet_queue = new Queue<dAmnPacket>();
                _out_queue = new Queue<byte[]>();

                ConIO.Write("Attempting to connect to the server...");

                // Get the IP of the host
                _ip = Dns.GetHostEntry(_host).AddressList[0];

                // Create the endpoint we will connect to.
                _endpoint = new IPEndPoint(_ip, _port);

                // Init the timeout timer
                timeout_timer = new Timer(1000);
                timeout_timer.Elapsed += TimerTick;
                timeout_timer.Start();

                // Initialize the socket
                // We could use normal sockets, but I like asynchronous sockets.
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                if (_socket == null)
                {
                    ConIO.Warning("Socket", "Unable to obtain a socket! Does the bot have privileges, or is a firewall blocking it?");
                    Program.Kill();
                }

                _socket.Blocking = false;
                _socket.BeginConnect(_endpoint, new AsyncCallback(on_connect), null);
            }
            catch (Exception E)
            {
                if (Program.Debug)
                    ConIO.Warning("Socket", "Error while creating socket: " + E.Message);
                ConIO.Warning("Socket", "Unable to connect to the internet. Check your connection.");
                Program.Kill();
            }
        }

        private void TimerTick (object sender, ElapsedEventArgs e)
        {
            if (_socket == null) return;

            if ((!IsConnected && !Closed) || Bot.EpochTimestampMS - LastPacket >= 140000)
            {
                Closed = true;
                on_disconnect();
            }
            else
            {
                try
                {
                    _socket.Send(temp, 0, SocketFlags.None);
                }
                catch
                {
                    Closed = true;
                    on_disconnect();
                }
            }
        }

        private void on_disconnect ()
        {
            if (Program.Bot != null && Program.Bot.Quitting) return;

            ConIO.Warning("Socket", "Disconnected! Reconnecting...");

            _socket.Dispose();
            timeout_timer.Dispose();
            _packet_queue.Clear();

            Program.Disconnects++;
            Program.ForceReconnect = true;
            Program.wait_event.Set();
        }

        private void on_connect (IAsyncResult result)
        {
            try
            {
                if (_socket == null) return;

                // We got a connection!
                _socket.EndConnect(result);
                Closed = false;

                // Announce ourselves
                Events.CallEvent("on_connect", null);

                Reconnects = 0;

                // Wait for data
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(on_receive), null);
            }
            catch
            {
                ConIO.Warning("Socket", "Unable to connect to the internet. Check your connection.");
                ConIO.Notice("Attempting to reconnect in 10 seconds...");
                System.Threading.Thread.Sleep(10000);
                Program.ForceReconnect = true;
                Program.wait_event.Set();
                return;
            }
        }

        private void on_sent (IAsyncResult result)
        {
            try
            {
                if (_socket == null || Closed)
                    return;

                int bytes = _socket.EndSend(result);
                Program.bytes_sent += (ulong)bytes;
            }
            catch (Exception E)
            {
                if (Program.Debug)
                    AnnounceError("on_sent", E);
            }
        }

        private void on_receive (IAsyncResult result)
        {
            try
            {
                if (_socket == null || Closed)
                    return;

                // End the receive, and get the number of bytes that are data.
                int received_bytes = _socket.EndReceive(result);

                if (received_bytes <= 0)
                {
                    on_disconnect();
                    return;
                }

                Program.bytes_received += (ulong)received_bytes;

                LastPacket = Bot.EpochTimestampMS;

                // Create a temporary buffer, and get only the amount of bytes from the buffer that
                //  are actual data, and not null chars used for padding (which would screw up our
                //  real null char, which ends a packet!)
                byte[] temp_buffer = new byte[received_bytes];
                //Array.Copy(_buffer, temp_buffer, received_bytes);
                Buffer.BlockCopy(_buffer, 0, temp_buffer, 0, received_bytes);

                // Parse the packet!
                //_packet += Encoding.UTF8.GetString(temp_buffer);
                _packet += Tools.UnicodeString(temp_buffer);

                // Unset the temporary buffer
                temp_buffer = null;

                // If there's a null char in the packet, that means there's a whole packet in there.
                while (_packet.Contains("\0"))
                {
                    // Take out the chunk that is the whole packet.
                    String packet = _packet.Substring(0, _packet.IndexOf('\0'));
                    _packet = _packet.Substring(_packet.IndexOf('\0') + 1);

                    // If the data is empty, ignore it.
                    if (packet.Length == 0) continue;

                    // Throw it in the queue
                    _packet_queue.Enqueue(new dAmnPacket(packet));
                }

                // Clear the buffer and go back to listening
                _buffer = new byte[_buffer_length];
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(on_receive), null);
            }
            catch (SocketException)
            {
                // Socket was closed by the remote host, most likely. We handle this elsewhere.
                on_disconnect();
            }
            catch (Exception E)
            {
                if (Program.Debug)
                    AnnounceError("on_receive", E);
            }
        }

        /// <summary>
        /// Announces an error based on the exception passed.
        /// </summary>
        /// <param name="function">Name of the function the error originated</param>
        /// <param name="E">Exception object</param>
        private void AnnounceError (String function, Exception E)
        {
            ConIO.Write(String.Format("Error in SocketWrapper.{0}: {1}\n\tException: {2}", function, E.Message, E.ToString()));
        }

        /// <summary>
        /// Sends a packet to the server
        /// </summary>
        /// <param name="packet">Packet in byte array form</param>
        public void SendOut (byte[] packet)
        {
            try
            {
                if (_socket == null || Closed)
                    return;

                Program.packets_out++;
                _socket.BeginSend(packet, 0, packet.Length, SocketFlags.None, new AsyncCallback(on_sent), null);
            }
            catch (SocketException)
            {
                // Socket was closed by the remote host, most likely. We handle this elsewhere.
                on_disconnect();
            }
            catch (Exception E)
            {
                if (Program.Debug)
                    AnnounceError("Send", E);
            }
        }

        /// <summary>
        /// Queues a packet for sending
        /// </summary>
        /// <param name="packet">Packet in byte array form</param>
        public void Send (byte[] packet)
        {
            _out_queue.Enqueue(packet);
        }

        /// <summary>
        /// Sends a packet to the server
        /// </summary>
        /// <param name="packet">Packet in string form</param>
        public void Send (String packet)
        {
            try
            {
                if (_socket == null) return;

                Send(Encoding.UTF8.GetBytes(packet));
            }
            catch (Exception E)
            {
                if (Program.Debug)
                    AnnounceError("Send(str)", E);
            }
        }

        /// <summary>
        /// Grabs the first packet in the queue, and removes it.
        /// </summary>
        /// <returns>dAmnPacket object</returns>
        public dAmnPacket Dequeue ()
        {
            if (_packet_queue.Count > 0)
            {
                return _packet_queue.Dequeue();
            }
            else
            {
                return null;
            }
        }

        public void PopPacket ()
        {
            if (_out_queue.Count > 0)
            {
                SendOut(_out_queue.Dequeue());
                //Bot.wait_event.Set();
            }
        }

        public int QueuedIn
        {
            get
            {
                return _packet_queue.Count;
            }
        }

        public int QueuedOut
        {
            get
            {
                return _out_queue.Count;
            }
        }

        /// <summary>
        /// Closes and kills the socket.
        /// </summary>
        public void Close ()
        {
            try
            {
                Closed = true;
                _socket.Disconnect(false);
                _socket.Close();
            }
            catch { }
            _buffer = null;
            _packet = null;
            _socket = null;
        }

        /// <summary>
        /// Gets the string representation of the IP:PORT we're connected to.
        /// </summary>
        /// <returns>String representation of the server endpoint</returns>
        public String Endpoint ()
        {
            return _socket.RemoteEndPoint.ToString();
        }
    }
}
