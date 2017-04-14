using lulzbot.Types;
using System;
using System.Collections.Generic;
//using AIMLbot;
using System.Text;

namespace lulzbot.Extensions
{
    public class AI
    {
        public struct AIConfig
        {
            public bool Enabled;
            public List<String> WhiteList;
        };

        public static AIConfig Config;
        //public static AIMLbot.Bot Alice;
        //public Dictionary<String, AIMLbot.User> Users;
        private static List<String> blacklist = new List<String>() { "#botdom", "#datashare", "#dsgateway", "#dshost" };

        public AI ()
        {
            /*ConIO.Write("Loading AIML AI files...", "AI");

            try
            {
                Alice = new AIMLbot.Bot();
                Alice.loadSettings("Storage/ai/config/Settings.xml");
                Alice.loadAIMLFromFiles();
            }
            catch (Exception E)
            {
                ConIO.Warning("AI", "Error while loading AI: " + E.Message);
                ConIO.Warning("AI", "AI extension will not be loaded!");
                return;
            }

            Users = new Dictionary<String, User>();*/

            var info = new ExtensionInfo("AI", "DivinityArcane", "1.0");

            Events.AddCommand("ai", new Command(this, "cmd_ai", "DivinityArcane", 100, "AI settings.", "[trig]ai on/off<br/>[trig]ai enable/disable #chan", ext: info));
            Events.AddEvent("recv_msg", new Event(this, "e_onmsg", "Parses and handles AI requests.", "AI", ext: info));

            // Load saved data, if we can.
            Config = Storage.Load<AIConfig>("ai");

            if (Config.WhiteList == null)
                Config.WhiteList = new List<String>();

            //ConIO.Write("AI extension loaded.", "AI");
        }

        public static void Save ()
        {
            Storage.Save("ai", Config);
        }

        public static void cmd_ai (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String helpmsg = String.Format("<b>&raquo; Usage:</b><br/>{0}ai on/off<br/>{0}ai enable/disable #channel", " &middot; " + bot.Config.Trigger);

            if (args.Length == 1)
                bot.Say(ns, helpmsg);
            else
            {
                String arg = args[1];

                if (arg == "on" || arg == "off")
                {
                    Config.Enabled = arg == "on";
                    Save();
                    bot.Say(ns, String.Format("<b>&raquo; AI has been {0}.</b>", Config.Enabled ? "enabled" : "disabled"));
                }
                else if ((arg == "enable" || arg == "disable") && args.Length == 3)
                {
                    if (!args[2].StartsWith("#"))
                    {
                        bot.Say(ns, "<b>&raquo; Invalid channel name! Channel names start with #</b>");
                        return;
                    }
                    else if (blacklist.Contains(args[2].ToLower()))
                    {
                        bot.Say(ns, "<b>&raquo; AI is forbidden in channel:</b> " + args[2]);
                        return;
                    }

                    String chan = Tools.FormatChat(args[2]).ToLower();

                    if (arg == "enable")
                    {
                        if (!Config.WhiteList.Contains(chan))
                        {
                            Config.WhiteList.Add(chan);
                            Save();
                            bot.Say(ns, String.Format("<b>&raquo; Channel {0} has been added to the whitelist.</b>", args[2]));
                        }
                        else bot.Say(ns, "<b>&raquo; That channel is already in the whitelist!</b>");
                    }
                    else
                    {
                        if (Config.WhiteList.Contains(chan))
                        {
                            Config.WhiteList.Remove(chan);
                            Save();
                            bot.Say(ns, String.Format("<b>&raquo; Channel {0} has been removed from the whitelist.</b>", args[2]));
                        }
                        else bot.Say(ns, "<b>&raquo; That channel is not in the whitelist!</b>");
                    }
                }
                else bot.Say(ns, helpmsg);
            }
        }

        /*private void SaveUser(String who)
        {
            if (Users.ContainsKey(who.ToLower()))
            {
                using (FileStream stream = new FileStream(@"Storage/ai/users/" + who + ".xml", FileMode.Create))
                {
                    using (XmlWriter writer = new XmlTextWriter(stream, Encoding.UTF8))
                    {
                        Users[who.ToLower()].Predicates.DictionaryAsXML.WriteTo(writer);
                        writer.Flush();
                    }
                }
            }
        }*/

        public void e_onmsg (Bot bot, dAmnPacket packet)
        {
            if (!Config.Enabled) return;
            if (!Config.WhiteList.Contains(packet.Parameter.ToLower())) return;

            if (packet.Body.ToLower().StartsWith(bot.Config.Username.ToLower() + ": ") && packet.Body.Length > bot.Config.Username.Length + 2)
            {
                String msg = packet.Body.Substring(bot.Config.Username.Length + 2);
                String from = packet.Arguments["from"];
                /*User user;
                
                if (!Users.ContainsKey(from.ToLower()))
                {
                    user = new User(from, Alice);

                    if (File.Exists(@"Storage/ai/users/" + from + ".xml"))
                        user.Predicates.loadSettings(@"Storage/ai/users/" + from + ".xml");
                    else SaveUser(from);

                    Users.Add(from.ToLower(), user);
                }
                else user = Users[from.ToLower()];

                Request request = new Request(msg, user, Alice);
                Result reply = Alice.Chat(request);

                SaveUser(from);*/

                String request = Convert.ToBase64String(Encoding.ASCII.GetBytes(msg));
                String url = @"http://kato.botdom.com/respond/" + from + "/" + request;
                String reply = Tools.GrabPage(url);

                bot.Say(packet.Parameter, from + ": " + reply);
            }
        }
    }
}