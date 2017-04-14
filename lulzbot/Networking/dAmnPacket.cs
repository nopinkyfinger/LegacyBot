using System;
using System.Collections.Generic;

namespace lulzbot
{
    /// <summary>
    /// This will be our dAmnPacket class. It will basically be an object that we
    ///  pass data to, and it will process the data and create a viable packet object
    ///  from it. From there on, we can use the object to handle events and respond.
    /// </summary>
    public class dAmnPacket
    {
        // Here are our public properties, which will hold our important data!

        // This shouldn't ever change in dAmn, but who knows. It's the separator between
        //  argument names and their data.
        // This is flagged "const" (a constant) so that it cannot be changed. It's also
        //  static by default, and is easily optimized by the compiler.
        private const String Separator = "=";

        // I prefer to initialize strings with the String.Empty value (""). This seems
        //  neater to me, and avoids errors if you somehow add to/use them before assignment.
        public String Command       = String.Empty;
        public String Parameter     = String.Empty;
        public String SubCommand    = String.Empty;
        public String SubParameter  = String.Empty;
        public String Body          = String.Empty;
        public String Raw           = String.Empty;
        public String Message       = String.Empty;

        // For the arguments, we'll use a dictionary. While, in the past, I used different types
        //  to handle duplicate keys, I'm not going to. Why? dAmn should _not_ send duplicate argument
        //  names.
        public Dictionary<String, String> Arguments = new Dictionary<String, String>();

        /// <summary>
        /// Initializes a new instance of the <see cref="lulzbot.Networking.dAmnPacket"/> class.
        /// </summary>
        public dAmnPacket ()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="lulzbot.Networking.dAmnPacket"/> class.
        /// </summary>
        /// <param name='data'>
        /// Data.
        /// </param>
        public dAmnPacket (String data)
        {
            this.Parse(data);
        }

        /// <summary>
        /// Here, we'll parse the string data of a packet and create our object.
        /// </summary>
        /// <param name="data">The string data of a packet.</param>
        public void Parse (String data)
        {
            if (data.Contains("\n"))
            {
                int pos = data.IndexOf("\n");
                String header = data.Substring(0, pos);

                if (header.Contains(" "))
                {
                    String[] bits   = header.Split(' ');
                    Command = bits[0];
                    Parameter = bits[1];
                }
                else
                {
                    Command = header;
                }
                data = data.Substring(pos + 1);
                if (data.Contains("\n\n"))
                {
                    pos = data.IndexOf("\n\n");
                    // We'll parse tablumps here
                    Body = Tools.ParseTablumps(data.Substring(pos + 2));
                    Message = Body;
                    data = data.Substring(0, pos);
                }
                foreach (String chunk in data.Split('\n'))
                {
                    if (String.IsNullOrWhiteSpace(chunk))
                    {
                        // Don't bother with empty chunks!
                        continue;
                    }
                    if (chunk.Contains(Separator))
                    {
                        String argument     = chunk.Substring(0, chunk.IndexOf(Separator));
                        String value        = chunk.Substring(chunk.IndexOf(Separator) + 1);

                        Arguments.Add(argument, value);
                    }
                    else
                    {
                        if (String.IsNullOrWhiteSpace(SubCommand))
                        {
                            if (chunk.Contains(" "))
                            {
                                String[] bits   = chunk.Split(' ');
                                SubCommand = bits[0];
                                SubParameter = bits[1];
                            }
                            else
                            {
                                SubCommand = chunk;
                            }
                        }
                        else
                        {
                            // Shouldn't happen
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Some packets contain arguments in the body. This function pulls them and adds them to Arguments.
        /// </summary>
        public void PullBodyArguments ()
        {
            if (!Body.Contains("\n") || !Body.Contains(Separator))
                return;

            foreach (String chunk in Body.Split('\n'))
            {
                if (String.IsNullOrWhiteSpace(chunk))
                {
                    // Don't bother with empty chunks!
                    continue;
                }
                if (chunk.Contains(Separator))
                {
                    String argument = chunk.Substring(0, chunk.IndexOf(Separator));
                    String value = chunk.Substring(chunk.IndexOf(Separator) + 1);

                    Arguments.Add(argument, value);
                }
            }
        }

        public override string ToString ()
        {
            String output = "dAmnPacket()\n{";
            output += "\n\tCmd     : " + Command;
            output += "\n\tParam   : " + Parameter;
            output += "\n\tSubCmd  : " + SubCommand;
            output += "\n\tSubParam: " + SubParameter;
            output += "\n\tArgs    :\n\t{";
            foreach (var pair in Arguments)
            {
                output += "\n\t\t[" + pair.Key + "]: " + pair.Value;
            }
            output += "\n\t}\n\tBody    : " + Body.Replace("\n", "\n\t\t  ") + "\n}";
            return output.Replace("\0", "");
        }

        // is the subcmd "action"? (Is it a /me post?)
        public bool isAction()
        {
            bool isAction;

            if (SubCommand == "action")
                isAction = true;
            else
                isAction = false;

            return isAction;
        }

        // get the name of the chatroom
        public String getChatroom()
        {
            String chatroom = Parameter.Substring(5);
            chatroom = String.Format("#{0}", chatroom);
            return chatroom;
        }
    }
}
