using lulzbot.Types;
using System;

namespace lulzbot.Extensions
{
    public class Colors
    {
        public struct ColorConfig
        {
            public bool Enabled;
            public String UsernameColor;
            public String MessageColor;
        };

        public static ColorConfig Config;

        public static String ColorTag
        {
            get
            {
                if (!Config.Enabled) return String.Empty;

                return String.Format("<abbr title=\"colors:{0}:{1}\"></abbr>", Config.UsernameColor, Config.MessageColor);
            }
        }

        public Colors ()
        {
            var info = new ExtensionInfo("Colors", "DivinityArcane", "1.0");

            Events.AddCommand("colors", new Command(this, "cmd_colors", "DivinityArcane", 100, "Changes the bot's colors.", "[trig]colors on/off<br/>[trig]colors username/message #html_color_code", ext: info));

            // Load saved data, if we can.
            Config = Storage.Load<ColorConfig>("colors");

            if (Config.MessageColor == null)
            {
                Config.MessageColor = "000000";
                Save();
            }

            if (Config.UsernameColor == null)
            {
                Config.UsernameColor = "000000";
                Save();
            }
        }

        public static void Save ()
        {
            Storage.Save("colors", Config);
        }

        public static void cmd_colors (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String helpmsg = String.Format("<b>&raquo; Usage:</b><br/>{0}colors on/off<br/>{0}colors username/message #colorcode", " &middot; " + bot.Config.Trigger);

            if (args.Length == 1)
                bot.Say(ns, helpmsg);
            else
            {
                String arg = args[1];

                if (arg == "on" || arg == "off")
                {
                    Config.Enabled = arg == "on";
                    Save();
                    bot.Say(ns, String.Format("<b>&raquo; Colors have been {0}.</b>", Config.Enabled ? "enabled" : "disabled"));
                }
                else if (arg == "username" && args.Length == 3)
                {
                    if (!args[2].StartsWith("#") || args[2].Length != 7)
                    {
                        bot.Say(ns, "<b>&raquo; Invalid color code! Use HTML color codes.</b>");
                        return;
                    }

                    Config.UsernameColor = args[2].Substring(1);
                    Save();
                    bot.Say(ns, String.Format("<b>&raquo; Username color has been set to {0}.</b>", args[2]));
                }
                else if (arg == "message" && args.Length == 3)
                {
                    if (!args[2].StartsWith("#") || args[2].Length != 7)
                    {
                        bot.Say(ns, "<b>&raquo; Invalid color code! Use HTML color codes.</b>");
                        return;
                    }

                    Config.MessageColor = args[2].Substring(1);
                    Save();
                    bot.Say(ns, String.Format("<b>&raquo; Message color has been set to {0}.</b>", args[2]));
                }
                else bot.Say(ns, helpmsg);
            }
        }
    }
}