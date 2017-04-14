/* NOTES
 * The "Say" Command is able to call back the bot and make it say something. Maybe
 * something similar is possible for rp commands?
 */

using lulzbot.Extensions;
using lulzbot.Extensions.RP_Tools;
using lulzbot.Networking;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace lulzbot
{
    public class Bot
    {
        // These are our dAmn server variables.
        //private const String _hostname = "chat.deviantart.com";
        //private const int _port = 3900;

        // Our socket wrapper/object.
        private SocketWrapper Socket = null;
        private const int MAX_ALLOWED_RPS = 128;
        private const int MIN_RP_ID = 0;
        private const int MAX_RP_ID = 127;
        private int numberOfRPs = 0;

        //RP Variables
        Roleplay[] rpList = new Roleplay[MAX_ALLOWED_RPS];
        RpLogger rplogger;
        string pastebinUserKey;
        string pastebinDevKey;

        public int QueuedIn
        {
            get
            {
                return Socket.QueuedIn;
            }
        }

        public int QueuedOut
        {
            get
            {
                return Socket.QueuedOut;
            }
        }

        // This is our config.
        public Config Config;

        // Basic vars that will be saved later
        public bool AutoReJoin = true;

        // Are we shutting down?
        public bool Quitting = false;

        // Core extensions
        public static Core Core;
        public static BDS BDS;
        public static Logger Logger;
        // define a seperate RP logger class here
        public static ExtensionContainer Extensions;
        public static Users Users;
        public static Colors Colors;
        public static AI AI;

        // Whether or not we can loop
        private bool can_loop = false;
        private Thread _loop_thread;

        // Wait event for the thread
        //public static ManualResetEvent wait_event;

        // Bot vars!
        private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0);

        /// <summary>
        /// Bot uptime in seconds.
        /// </summary>
        public ulong uptime
        {
            get
            {
                return Convert.ToUInt64((DateTime.UtcNow - Program.StartTime).TotalSeconds);
            }

            set { }
        }

        /// <summary>
        /// Seconds since the unix epoch
        /// </summary>
        public static ulong EpochTimestamp
        {
            get
            {
                return Convert.ToUInt64((DateTime.UtcNow - _epoch).TotalSeconds);
            }

            set { }
        }

        /// <summary>
        /// Milliseconds since the unix epoch
        /// </summary>
        public static ulong EpochTimestampMS
        {
            get
            {
                return Convert.ToUInt64((DateTime.UtcNow - _epoch).TotalMilliseconds);
            }

            set { }
        }

        /// <summary>
        /// Ticks when we were pinged.
        /// </summary>
        public ulong _pinged = 0;
        public Stopwatch PingTimer = new Stopwatch();


        /// <summary>
        /// Constructor. Spawn a new bot instance
        /// </summary>
        /// <param name="config">Configuration object</param>
        public Bot(Config config, string host, int port)
        {
            // Initialize the wait handler
            //wait_event = new ManualResetEvent(false);

            // Quick and dirty load of pastebin stuff
            string buffer;


            //Somehow use a file for this or something, try to make it available on github without compromising security

            using (Stream stream = new FileStream("./pastebin.dat", FileMode.Open))
            {
                using (StreamReader file = new StreamReader(stream))
                {
                    buffer = Encryption.Decrypt(file.ReadToEnd());
                }
            }            

            int index = buffer.IndexOf('\n');
            pastebinUserKey = buffer.Substring(0, index);
            pastebinDevKey = buffer.Substring(index + 1);

            // Assign the config to our class variable
            this.Config = config;

            // Check if the authtoken stored is empty
            if (String.IsNullOrWhiteSpace(Config.Authtoken))
            {
                ConIO.Write("We don't have an authtoken. Grabbing one...");
                Config.Authtoken = AuthToken.Grab(Config.Username, Config.Password);

                if (String.IsNullOrWhiteSpace(Config.Authtoken))
                {
                    ConIO.Write("Invalid username or password! Deleting config...");
                    System.IO.File.Delete(@"./Config.dat");
                    Program.Running = false;
                    Program.wait_event.Set();
                    return;
                }
                else
                {
                    ConIO.Write("Got an authtoken!");
                    Config.Save("Config.dat");
                }
            }

            // Make sure events are clear.
            Events.ClearEvents();

            // Initialize the Core extensions
            Core = new Core();
            BDS = new BDS();
            Logger = new Logger();
            // Add a seperate RP logger.
            Extensions = new ExtensionContainer();
            Users = new Users(this.Config.Owner);
            Colors = new Colors();
            AI = new AI();

            // Now, let's initialize the socket.
            Socket = new SocketWrapper();
            Socket.Connect(host, port);

            can_loop = true;

            // Start a new thread for our MainLoop method
            _loop_thread = new Thread(new ThreadStart(MainLoop));
            _loop_thread.IsBackground = false;
            _loop_thread.Start();
        }

        /// <summary>
        /// Main bot loop
        /// </summary>
        private void MainLoop()
        {
            String day = Tools.strftime("%B %d %Y");
            String oldDay = day;
            // Set up 
            for (int i = 0; i < MAX_ALLOWED_RPS; i++)
            {
                rpList[i] = new Roleplay();
                rpList[i].setBot(this);
            }
            rplogger = new RpLogger(ref rpList, this);

            // Woo! Loop!
            while (can_loop)
            {
                //Reset logging if day changes
                day = Tools.strftime("%B %d %Y");
                if (day != oldDay)
                {
                    ConIO.Write("Midnight, resetting RP ids");
                    numberOfRPs = 0;
                }
                oldDay = day;

                // Wait for a signal
                //wait_event.WaitOne();

                if (Socket.QueuedOut > 0)
                {
                    Socket.PopPacket();
                }
                dAmnPacket packet = null;

                if ((packet = Socket.Dequeue()) != null)
                {
                    if (packet != null)
                    {
                        Program.packets_in++;
                        // Process the packet
                        try
                        {
                            if (packet.Command == "recv")
                            {
                                // This way of doing things seems hacky as fuck but I don't know a better way
                                // Core class has commands and stuff look into that

                                if (packet.isAction())
                                {
                                    rplogger.logAction(packet.Message, packet.Arguments["from"], convertChatroom(packet.Parameter));
                                }

                                if (packet.Body.IndexOf("LegacyBot: new rp", StringComparison.InvariantCultureIgnoreCase) != -1)
                                    startNewRP(packet.Body, packet.Parameter);
                                if (packet.Body.IndexOf("LegacyBot: end rp", StringComparison.InvariantCultureIgnoreCase) != -1)
                                    endRP(packet.Body, packet.Parameter);
                                if (packet.Body.IndexOf("LegacyBot: list all", StringComparison.InvariantCultureIgnoreCase) != -1)
                                    listRPs(packet.Parameter);
                                // LegacyBot: add character <character name> <player name> to rp <ID>, or something like that
                                if (packet.Body.IndexOf("LegacyBot: add character", StringComparison.InvariantCultureIgnoreCase) != -1)
                                    addCharacter(packet.Body, packet.Parameter);
                                // LegacyBot: remove character <character name> <player name> from rp <ID>, or something like that
                                if (packet.Body.IndexOf("LegacyBot: remove character", StringComparison.InvariantCultureIgnoreCase) != -1)
                                    removeCharacter(packet.Body, packet.Parameter);

                                //*********************
                                // End of my code block
                                new Thread(() => Events.CallEvent("recv_" + packet.SubCommand, packet)).Start();
                            }
                            else
                            {
                                new Thread(() => Events.CallEvent(packet.Command, packet)).Start();
                            }
                        }
                        catch { }
                    }
                }

                // Go back to waiting.
                //wait_event.Reset();

                Thread.Sleep(Program.OldPC ? 55 : 1);
            }
        }

        /// <summary>
        /// Reconnect the bot
        /// </summary>
        public void Reconnect()
        {
            if (Quitting)
                return;

            // No reason to call this now. It might cause issues.
            //Events.ClearEvents();

            ConIO.Write("Reconnecting in 5 seconds!");

            Thread.Sleep(5000);

            ConIO.Write("Reconnecting...");

            Program.ForceReconnect = true;
            Socket.Close();
            can_loop = false;
            Program.wait_event.Set();
        }

        /// <summary>
        /// Joins a dAmn channel
        /// </summary>
        /// <param name="channel">Channel to join</param>
        public void Join(String channel)
        {
            if (channel.StartsWith("#"))
            {
                channel = String.Format("chat:{0}", channel.Substring(1));
            }
            Send(dAmnPackets.Join(channel));
        }

        /// <summary>
        /// Parts a dAmn channel
        /// </summary>
        /// <param name="channel">Channel to part</param>
        public void Part(String channel)
        {
            if (channel.StartsWith("#"))
            {
                channel = String.Format("chat:{0}", channel.Substring(1));
            }
            if (channel.ToLower() != "chat:datashare")
                Send(dAmnPackets.Part(channel));
        }

        /// <summary>
        /// Sends a message to the specified channel
        /// </summary>
        /// <param name="channel">Channel to send to</param>
        /// <param name="message">Message to say</param>
        public void Say(String channel, String message)
        {
            if (channel.StartsWith("#"))
            {
                channel = String.Format("chat:{0}", channel.Substring(1));
            }
            if (Colors.Config.Enabled)
                message += Colors.ColorTag;
            Send(dAmnPackets.Message(channel, message));
        }

        /// <summary>
        /// Sends a non-parsed message to the specified channel
        /// </summary>
        /// <param name="channel">Channel to send to</param>
        /// <param name="message">Message to say</param>
        public void NPSay(String channel, String message)
        {
            if (channel.StartsWith("#"))
            {
                channel = String.Format("chat:{0}", channel.Substring(1));
            }
            Send(dAmnPackets.NPMessage(channel, message));
        }

        /// <summary>
        /// Sends an action to the specified channel
        /// </summary>
        /// <param name="channel">Channel to send to</param>
        /// <param name="message">Message to say</param>
        public void Act(String channel, String message)
        {
            if (channel.StartsWith("#"))
            {
                channel = String.Format("chat:{0}", channel.Substring(1));
            }
            Send(dAmnPackets.Action(channel, message));
        }

        public void Kick(String channel, String who, String reason)
        {
            if (channel.StartsWith("#"))
            {
                channel = String.Format("chat:{0}", channel.Substring(1));
            }
            Send(dAmnPackets.Kick(channel, who, reason));
        }

        public void Ban(String channel, String who)
        {
            if (channel.StartsWith("#"))
            {
                channel = String.Format("chat:{0}", channel.Substring(1));
            }
            Send(dAmnPackets.Ban(channel, who));
        }

        public void UnBan(String channel, String who)
        {
            if (channel.StartsWith("#"))
            {
                channel = String.Format("chat:{0}", channel.Substring(1));
            }
            Send(dAmnPackets.UnBan(channel, who));
        }

        public void Admin(String channel, String command)
        {
            if (channel.StartsWith("#"))
            {
                channel = String.Format("chat:{0}", channel.Substring(1));
            }
            Send(dAmnPackets.Admin(channel, command));
        }

        public void Kill(String who, String reason)
        {
            Send(dAmnPackets.Kill(who, reason));
        }

        public void Promote(String channel, String who, String privclass)
        {
            if (channel.StartsWith("#"))
            {
                channel = String.Format("chat:{0}", channel.Substring(1));
            }
            Send(dAmnPackets.Promote(channel, who, privclass));
        }

        public void Demote(String channel, String who, String privclass)
        {
            if (channel.StartsWith("#"))
            {
                channel = String.Format("chat:{0}", channel.Substring(1));
            }
            Send(dAmnPackets.Demote(channel, who, privclass));
        }

        /// <summary>
        /// Sends a packet to the server
        /// </summary>
        /// <param name="packet">dAmnPacket in byte array form</param>
        public void Send(byte[] packet)
        {
            Socket.Send(packet);
        }

        /// <summary>
        /// Sends a packet to the server
        /// </summary>
        /// <param name="packet">dAmnPacket in string form</param>
        public void Send(String packet)
        {
            Socket.Send(packet);
        }

        /// <summary>
        /// Sends the disconnect packet.
        /// </summary>
        public void Disconnect()
        {
            Send("disconnect\n\0");
        }

        /// <summary>
        /// Closes down the bot.
        /// </summary>
        public void Close()
        {
            Socket.Close();
            can_loop = false;
            Program.Running = false;
            Program.wait_event.Set();
        }

        /// <summary>
        /// Return the server endpoint in IP:PORT format
        /// </summary>
        /// <returns>Server endpoint</returns>
        public String Endpoint()
        {
            return Socket.Endpoint();
        }


        //*********************************************************************
        // Everything below this is my code
        //*********************************************************************

        /// <summary>
        /// Starts a new RP
        /// </summary>
        /// <param name="body">The body of the message the user sent</param>
        /// <param name="chatroom">The chatroom the message was sent in</param>
        void startNewRP(String body, String chatroom)
        {
            int availableRP;
            int numSpaces;
            int numChars;
            int index;
            int logID;
            String rpData;
            String rpDataParse;
            String command;
            String character;
            String player;
            String convertedRoom;
            String[] args;
            Character newChar;


            convertedRoom = convertChatroom(chatroom);
            availableRP = getNextAvailableRP();
            rpList[availableRP].setActive(true);
            rpList[availableRP].clear();
            command = getCommand(body);
            rpData = command.Substring(7);
            rpDataParse = rpData;
            numSpaces = rpData.Count(Char.IsWhiteSpace);
            numChars = (numSpaces + 1) / 2;
            args = new String[numSpaces + 1];

            String day = Tools.strftime("%B %d %Y");
            String month = Tools.strftime("%Y-%m %B");
            logID = numberOfRPs;
            String path = String.Format("Storage/Filtered RP Logs/{0}/{1}/{2}/{3}.txt",
                convertedRoom, month, day, logID);

            for (int i = 0; i < (numChars); i++)
            {
                if (i < numChars - 1)
                {
                    index = rpDataParse.IndexOf(' ');
                    player = rpDataParse.Substring(0, index);
                    rpDataParse = rpDataParse.Substring(index + 1);
                    args[i * 2] = player;

                    index = rpDataParse.IndexOf(' ');
                    character = rpDataParse.Substring(0, index);
                    rpDataParse = rpDataParse.Substring(index + 1);

                    newChar = new Character(player, character, convertedRoom);
                    rpList[availableRP].addCharacter(newChar);
                    ConIO.Write(convertedRoom);
                }
                else
                {
                    index = rpDataParse.IndexOf(' ');
                    player = rpDataParse.Substring(0, index);
                    rpDataParse = rpDataParse.Substring(index + 1);

                    character = rpDataParse;

                    newChar = new Character(player, character, convertedRoom);
                    rpList[availableRP].addCharacter(newChar);
                }
            }

            rpList[availableRP].setLogID(logID);
            rpList[availableRP].setPath(path);
            ConIO.Write(path);
            numberOfRPs++;

            Say(convertedRoom, String.Format("Started a new RP. ID number " +
                availableRP.ToString() + ". " + rpList[availableRP].ToString()));
        }

        /// <summary>
        /// Ends an rp, setting it as inactive.
        /// </summary>
        /// <param name="body">The body of the message the user sent</param>
        /// <param name="chatroom">The chatroom the message was sent in</param>
        void endRP(String body, String chatroom)
        {
            String command = getCommand(body);
            String idstr = command.Substring(7);
            int id;

            if (idstr == "") // no id
                Send(dAmnPackets.Message(chatroom, "Error, you must include the numeric ID!"));
            else
            {
                id = Int32.Parse(idstr);
                if (id > -1 && id < MAX_ALLOWED_RPS)
                {
                    if (rpList[id].isActive() == false) // The selected RP isn't active
                        Send(dAmnPackets.Message(chatroom,
                            String.Format("RP {0} isn't active to begin with...", id.ToString())));
                    else // success
                    {
                        string url = uploadToPastebin(rpList[id]);

                        rpList[id].setActive(false); // need to clear the characters!
                        rpList[id].clear();
                        Send(dAmnPackets.Message(chatroom,
                            String.Format("RP {0} has ended.", id.ToString())));
                        Send(dAmnPackets.Message(chatroom, url));
                    }
                }
                else // id out of range
                    Send(dAmnPackets.Message(chatroom, String.Format("Error, ID must be between 0 and {0}!",
                        (MAX_ALLOWED_RPS - 1).ToString())));
            }
            // take this out at some point, just a stopgap solution
        }

        /// <summary>
        /// Adds a new character to a specified RP
        /// </summary>
        /// <param name="body"></param>
        /// <param name="chatroom"></param>
        /// LegacyBot: Add character Nopinky Jalen 1
        /// Possibly start using commas to separate parameters 14
        void addCharacter(String body, String chatroom)
        {
            int selectedRP;
            int index;
            String character;
            String player;
            String command;
            Character newCharacter;

            // Put this code in its own class?
            chatroom = convertChatroom(chatroom);
            command = getCommand(body);
            command = command.Substring(14);

            index = command.IndexOf(' ');
            player = command.Substring(0, index);
            command = command.Substring(index + 1);

            index = command.IndexOf(' ');
            character = command.Substring(0, index);
            command = command.Substring(index + 1);

            selectedRP = Int32.Parse(command);

            newCharacter = new Character(player, character, chatroom);
            rpList[selectedRP].addCharacter(newCharacter);
            Say(chatroom, "Added " + newCharacter.ToString() + " to RP "
                + selectedRP + ".");
        }

        /// <summary>
        /// Removes a character from RP
        /// </summary>
        /// <param name="body"></param>
        /// <param name="chatroom"></param>
        /// LegacyBot: Remove character Nopinky Jalen 1
        void removeCharacter(String body, String chatroom)
        {
            int selectedRP;
            int index;
            String character;
            String player;
            String command;
            Character newCharacter;

            // Put this code in its own class?
            chatroom = convertChatroom(chatroom);
            command = getCommand(body);
            command = command.Substring(17);

            index = command.IndexOf(' ');
            player = command.Substring(0, index);
            command = command.Substring(index + 1);

            index = command.IndexOf(' ');
            character = command.Substring(0, index);
            command = command.Substring(index + 1);

            selectedRP = Int32.Parse(command);

            newCharacter = new Character(player, character, chatroom);

            if (rpList[selectedRP].hasCharacter(newCharacter))
            {
                rpList[selectedRP].removeCharacter(newCharacter);
                Say(chatroom, "Removed " + newCharacter.ToString() + " from RP "
                    + selectedRP + ".");
            }
            else
                Say(chatroom, newCharacter.ToString() + " is not in RP " + selectedRP + ".");
        }


        //*********************************************************************
        // Backend classes after this point
        //*********************************************************************

        /// <summary>
        /// Goes through the list of RPs, finding the next available one.
        /// </summary>
        /// <returns>The slot of the next available RP</returns>
        int getNextAvailableRP()
        {
            int slot = -1;
            bool hasSlot = false;
            int index = 0;
            while (hasSlot == false)
            {
                if (!(rpList[index].isActive()))
                {
                    slot = index;
                    hasSlot = true;
                }

                index++;
            }
            return slot;
        }

        /// <summary>
        /// Cuts off the username from a message
        /// </summary>
        /// <param name="msg">The message the user sent, raw</param>
        /// <returns>message the user sent, without the username</returns>
        String getCommand(String msg)
        {
            int cutoff = msg.IndexOf(' ');
            String command = msg.Substring(cutoff + 1);
            return command;
        }

        /// <summary>
        /// Converts the chatroom to the kind preceded by a #
        /// </summary>
        /// <param name="room">chatroom name, in raw packet form</param>
        /// <returns>chatroom name, type preceeded by #</returns>
        String convertChatroom(string room)
        {
            room = room.Substring(5);
            room = String.Format("#" + room);
            return room;
        }

        /// <summary>
        /// Lists all RPs
        /// </summary>
        /// <param name="room">The name of the chatroom the user sent the message from</param>
        void listRPs(String room)
        {
            room = convertChatroom(room);
            Say(room, "Listing all active RPs");
            for (int i = 0; i < MAX_ALLOWED_RPS; i++)
            {
                if (rpList[i].isActive())
                    Say(room, "RP " + i + ": " + rpList[i].ToString());
            }
        }

        /// <summary>
        /// Uploads an RP to pastebin
        /// API here: https://pastebin.com/api
        /// </summary>
        /// <param name="rp">The RP that is going to end</param>
        /// <param name="filepath">The path to the log text files</param>
        /// <returns></returns>
        string uploadToPastebin(Roleplay rp)
        {
            string uri = "https://pastebin.com/api/api_post.php";
            string option = "paste";
            string response = "placeholder";
            string filepath = rp.getPath();
            string pastename = rp.getLogID().ToString();
            string rpText = "";
            string line;

            WebClient client = new WebClient();
            NameValueCollection parameters = new NameValueCollection();
            StreamReader reader = new StreamReader(filepath);

            Console.Write(filepath);
            while ((line = reader.ReadLine()) != null)
                rpText += (line + "\n");


            parameters.Add("api_dev_key", pastebinDevKey);
            parameters.Add("api_option", option);
            parameters.Add("api_paste_code", rpText);
            parameters.Add("api_paste_name", pastename);
            parameters.Add("api_user_key", pastebinUserKey);
            response = System.Text.Encoding.UTF8.GetString(client.UploadValues(uri, parameters));

            return response;
        }

    }
}
