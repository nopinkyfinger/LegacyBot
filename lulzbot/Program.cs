/*  lulzBot - a C# bot for dAmn.
 * 
 * Project creation date: Dec. 13th, 2012.
 * 
 * Authors: DivinityArcane, OrrinFox.
 * 
 * Desc.: The main purpose of this bot is to teach the basics of C#.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Mono.Unix.Native;
using lulzbot.Networking;

namespace lulzbot
{
    public class Program
    {
        // This boolean controls whether or not the bot is allowed to run.
        public static bool Running = true;

        // Are we in debug mode?
        public static bool Debug = false;
        public static bool OldPC = false;

        // This is our configuration.
        private static Config Config = new Config();

        private static string host = "chat.deviantart.com";
        private static int port = 3900;

        // Our OS string
        public static String OS
        {
            get
            {
                var osi = Environment.OSVersion;
                var plat = osi.Platform;
                var maj = osi.Version.Major;
                var min = osi.Version.Minor;
                var rev = osi.Version.Revision.ToString();
                var ret = osi.ToString();

                if (plat == PlatformID.Win32Windows)
                {
                    if (min == 0)
                        ret = "Windows 95";

                    else if (min == 10)
                    {
                        if (rev == "2222A")
                            ret = "Windows 98 Second Edition";
                        else
                            ret = "Windows 98";
                    }

                    else if (min == 90)
                        ret = "Windows ME";
                }

                else if (plat == PlatformID.Win32NT)
                {
                    if (maj == 3)
                        ret = "Windows NT 3.51";

                    else if (maj == 4)
                        ret = "Windows NT 4.0";

                    else if (maj == 5)
                    {
                        if (min == 0)
                            ret = "Windows 2000";
                        else
                            ret = "Windows XP";
                    }

                    else if (maj == 6)
                    {
                        if (min == 0)
                            ret = "Windows Vista";
                        else if (min == 1)
                            ret = "Windows 7";
                        else if (min == 2)
                            ret = "Windows 8";
                        else // 6.3
                            ret = "Windows 8.1";
                    }

                    else if (maj == 10)
                    {
                        ret = "Windows 10";
                    }
                }

                else if (plat == PlatformID.MacOSX)
                {
                    ret = String.Format("Mac OSX {0}.{1}.{2}", maj, min, rev);
                }

                else if (plat == PlatformID.Unix)
                {
                    ret = String.Format("Linux {0}.{1}.{2}", maj, min, rev);
                }

                return ret + (Mono != null ? Mono : "");
            }
        }

        public static String Mono = null;

        // This is our bot object.
        public static Bot Bot = null;

        // Force the bot to reconnect?
        public static bool ForceReconnect = false;

        // Our bot thread!
        private static Thread _thread;

        // Wait event. Helps keep certain events in order.
        public static ManualResetEvent wait_event;

        // Bot related globals.
        public static DateTime StartTime            = DateTime.UtcNow;
        public static int Disconnects               = 0;
        public static ulong bytes_sent              = 0, bytes_received = 0, packets_in = 0, packets_out = 0;
        public static List<String> OfficialChannels = new List<String>() { "#devart", "#help", "#mnadmin", "#seniors", "#communityrelations", "#damnidlers" };
        public static List<String> NoDisplay        = new List<String>() { "#datashare", "#dsgateway", "LegacyBotControl" };
        public const String BotName                 = "lulzBot";
        public const String Version                 = "1.28 Final-er";
        public const String ReleaseName             = "Synergy";

        static void Main (string[] args)
        {
            /* Well, first off, the bot is _not_ going to be in the main file.
            * Why? That's silly. I don't like doing that. OOP, man. OOP.
            * Anyway, the Bot will be a separate class, and hence, object.
            * 
            * Of course, it will be started in a separate thread, but this main
            *  class will be in control of when the program ultimately ends.
            * 
            * For example: If you were to set the variable "Running" to false, 
            *  the bot instance would be killed off and the application will exit.
            *  
            * This ensures that there is always a reasonable way to kill of the bot.
            * 
            * Maybe that's just me though. -DivinityArcane */

            //Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Just a bit of a simple title.
            ConIO.Write("RP Logger by ~nopinky. Written for Star Trek Legacy");
            ConIO.Write("Based on:");
            ConIO.Write(String.Format("{0} [{1}], version {2}", BotName, ReleaseName, Version));
            ConIO.Write("Written and developed by DivinityArcane.");

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                // Prevent the bot being run on a Windows NT OS earlier than 6.0 (Vista)
                // TL Note: it won't run on anything earlier than NT 5 anyway, so no need for a check there.
                if (Environment.OSVersion.Version.Major < 6)
                {
                    ConIO.Warning("Compatibility", "The bot cannot be run on Windows operating systems older than Windows Vista.");
                    ConIO.Notice("If you are running Windows XP or earlier, you will not be able to use this bot.");
                    ConIO.Notice("It is recommended that you either upgrade your operating system to something that's not over a decade old, or use a different bot.");
                    ConIO.Notice("Sorry for the inconvenience!");
                    Environment.Exit(-1);
                }
            }

            ConIO.Write("Looks like we're running on " + OS);

            try
            {
                if (Syscall.getuid() == 0)
                {
                    ConIO.Warning("System", "The bot cannot be run as root!");
                    Environment.Exit(-1);
                }
            }
            catch { } // Windows, without Mono.

            Type _monotype = Type.GetType("Mono.Runtime");
            if (null != _monotype)
            {
                System.Reflection.MethodInfo _mono_dsp_name = _monotype.GetMethod("GetDisplayName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (null != _mono_dsp_name)
                    Mono = String.Format(" (Running in Mono {0})", _mono_dsp_name.Invoke(null, null));
                else
                    Mono = " (Running in Mono)";
            }

            string pk = null;

            // Check for debug mode!
            if (args.Length >= 1)
            {
                //foreach (String arg in args)
                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i];

                    if (arg == "--server" && args.Length >= i + 1)
                    {
                        host = args[i + 1];
                    }
                    else if (arg == "--port" && args.Length >= i + 1)
                    {
                        if (!int.TryParse(args[i + 1], out port))
                            port = 3900;
                    }
                    else if (arg == "--pk" && args.Length >= i + 1)
                    {
                        pk = args[i + 1];
                    }
                    else if (arg == "--debug")
                    {
                        ConIO.Write("Debug mode is enabled!");
                        Debug = true;
                    }
                    else if (arg == "--oldpc")
                    {
                        ConIO.Write("Upping loop delay.");
                        OldPC = true;
                    }
                    else if (arg == "--help")
                    {
                        ConIO.Write("To enable debug mode use the command line switch --debug");
                        ConIO.Write("To specify an authtoken to use (for example, on Ouroboros) use --pk");
                        ConIO.Write("To set the server or port, use --server and --port");
                        Environment.Exit(0);
                    }
                }
            }

            // If in debug mode, output CWD
            if (Debug)
                ConIO.Write("Running in directory: " + Environment.CurrentDirectory, "Debug");

            // First things first: We need a config file! If we don't have one, make one.
            if (!File.Exists("./Config.dat"))
            {
                ConIO.Write("Looks like you don't have a config file. Let's make one!");
                ConIO.Write("I'm going to need some basic details about you and the bot.");
                Config.Username = ConIO.Read("What is the bot's dA username?");
                Config.Password = ConIO.Read("What is the bot's dA password?");
                Config.Owner = ConIO.Read("What is your dA username?");
                Config.Trigger = ConIO.Read("What is the bot's command trigger?");
                while (Config.Trigger.Length < 2)
                {
                    ConIO.Notice("The command trigger must be at least two characters long!");
                    Config.Trigger = ConIO.Read("What is the bot's command trigger?");
                }

                // Channels need to be split, so let's just get them one by one.
                String channel = "none";

                ConIO.Write("OK. What channels will the bot join? One at a time, please.");
                ConIO.Write("When you're finished, just hit enter on an empty line.");

                while (!String.IsNullOrWhiteSpace(channel))
                {
                    if (!channel.StartsWith("#"))
                    {
                        ConIO.Write("Valid channel names start with #");
                    }
                    else if (!Config.Channels.Contains(channel.ToLower()))
                    {
                        Config.Channels.Add(channel.ToLower());
                    }
                    else
                    {
                        ConIO.Write("You already added that channel!");
                    }
                    channel = ConIO.Read("Add a channel?");
                }

                if (Config.Channels.Count <= 0)
                {
                    ConIO.Write("No channels added. Defaulting to #Botdom");
                    Config.Channels.Add("#Botdom");
                }

                ConIO.Write("That'll do it! Saving the config and continuing.");
                Config.Save("./Config.dat");
            }
            else
            {
                ConIO.Write("Configuration exists, loading it...");
                if (Config.Load("./Config.dat"))
                {
                    if (String.IsNullOrWhiteSpace(Config.Username) || String.IsNullOrWhiteSpace(Config.Password) || String.IsNullOrWhiteSpace(Config.Owner) || String.IsNullOrWhiteSpace(Config.Trigger))
                    {
                        ConIO.Write("Config data was null. Clearing the config file. Please restart the bot and reconfigure it.");
                        File.Delete("./Config.dat");

                        // Exit the app.
                        ConIO.Read("Press return/enter to close this window...");
                        Environment.Exit(-1);
                    }

                    ConIO.Write("Config loaded for: " + Config.Username);
                }
                else
                {
                    ConIO.Write("Something went wrong with the config!");
                    ConIO.Write("Please delete Config.dat and restart the bot.");

                    // Exit the app.
                    ConIO.Read("Press return/enter to close this window...");
                    Environment.Exit(-1);
                }
            }

            ConIO.Write("Checking if we're running an up-to-date version...");
            String uptodate = Tools.UpToDate(Version);

            if (uptodate == "ERR")
                ConIO.Warning("Botdom", "Unable to retreive version information at this time.");
            else if (uptodate == "OK")
                ConIO.Write("We're up to date!");
            else
            {
                ConIO.Notice(uptodate);
                ConIO.Write("To update, check http://j.mp/15ikMg1 or use " + Config.Trigger + "update");
                ConIO.Warning("LulzBot", "This bot is out of date, and may not work correctly. Either update the bot or confirm below.");
                
                var ans = ConIO.Read("Continue starting the bot anyway [y/n]").ToLower();
                while (ans != "y" && ans != "n")
                {
                    ConIO.Notice("Invalid input. Please answer with Y or N.");
                    ans = ConIO.Read("Continue starting the bot anyway").ToLower();
                }

                if (ans == "n")
                {
                    ConIO.Write("Shutting down.");
                    Environment.Exit(0);
                }

                ConIO.Write("Starting anyway. You've been warned!");
            }

            if (pk != null) Config.Authtoken = pk;

            // Initialize events system
            Events.InitEvents();

            // Initialize the tablump parser
            Tools.InitLumps();

            // Initialize the wait event
            wait_event = new ManualResetEvent(true);

            // Ok, let's fire up the bot!
            // I considered passing the config as a reference, but there's no point.
            // Instead, let's just copy the config to the bot.
            _thread = new Thread(new ThreadStart(Start));
            _thread.IsBackground = false;
            _thread.Start();

            while (Running)
            {
                // Wait for a signal
                wait_event.WaitOne();

                // Check if we need to reconnect
                if (ForceReconnect)
                {
                    ForceReconnect = false;
                    Timers.Clear();
                    _thread.Abort();
                    SocketWrapper.Reconnects++;
                    if (SocketWrapper.Reconnects >= 3)
                    {
                        ConIO.Warning("Socket", "Failed to reconnect too many times. Killing the bot...");
                        Program.Kill();
                    }
                    if (!Running)
                        break;
                    _thread = new Thread(new ThreadStart(Start));
                    _thread.IsBackground = false;
                    _thread.Start();
                }

                // Wait for another signal
                wait_event.Reset();
            }

            // Make sure they see whatever happened first.
            ConIO.Read("Press return/enter to close this window...");

            // Make sure all threads are killed off, and exit. Exit code 0 = OK
            Environment.Exit(0);
        }

        /// <summary>
        /// Initializes a new bot instance
        /// </summary>
        public static void Start ()
        {
            if (!Program.Running || (Program.Bot != null && Program.Bot.Quitting)) return;
            Program.Bot = null;
            Program.Bot = new Bot(Config, host, port);
        }

        public static void Kill ()
        {
            try
            {
                _thread.Abort();
            }
            catch { }
            Running = false;
            Bot = null;
            ConIO.Read("Press ENTER/RETURN to close this window...");
            Environment.Exit(-1);
        }

        public static bool RenewToken ()
        {
            var at = Networking.AuthToken.Grab(Config.Username, Config.Password);

            if (at == null)
            {
                ConIO.Warning("dAmn", "Unable to grab a new authtoken!");
                return false;
            }

            Config.Authtoken = Networking.AuthToken.Grab(Config.Username, Config.Password);
            Config.Save("./Config.dat");
            ConIO.Write("Got a new authtoken!");
            return true;
        }

        public static void Change_Trigger (String trig)
        {
            Config.Trigger = trig;
            Config.Save("./Config.dat");
        }

        public static void AddChannel (String chan)
        {
            Config.Channels.Add(chan);
            Config.Save("./Config.dat");
        }

        public static void RemoveChannel (String chan)
        {
            Config.Channels.Remove(chan);
            Config.Save("./Config.dat");
        }
    }
}
