using lulzbot.Networking;
using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_set (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String helpmsg = String.Format("<b>&raquo; Usage:</b> {0}set <i>[#channel]</i> [title|topic] [content]", bot.Config.Trigger);

            if (args.Length < 2)
            {
                bot.Say(ns, helpmsg);
            }
            else
            {
                String chan, prop, body;

                if (!args[1].StartsWith("#"))
                {
                    chan = ns.ToLower(); ;
                    prop = args[1];
                    body = msg.Substring(prop.Length + 4);
                }
                else if (args.Length >= 3)
                {
                    chan = Tools.FormatChat(args[1]).ToLower();
                    prop = args[2];
                    body = msg.Substring(prop.Length + args[1].Length + 5);
                }
                else
                {
                    bot.Say(ns, helpmsg);
                    return;
                }

                if (prop != "title" && prop != "topic")
                {
                    bot.Say(ns, "<b>&raquo; Invalid property!</b> Valid properties are title and topic.");
                    return;
                }

                lock (CommandChannels["set"])
                {
                    CommandChannels["set"].Add(ns);

                    bot.Send(dAmnPackets.Set(chan, prop, body));
                }
            }
        }
    }
}

