using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_ban (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String helpmsg = String.Format("<b>&raquo; Usage:</b> {0}ban <i>[#channel]</i> username", bot.Config.Trigger);

            if (args.Length < 2)
            {
                bot.Say(ns, helpmsg);
            }
            else
            {
                String chan, who;

                if (!args[1].StartsWith("#"))
                {
                    chan = ns;
                    who = args[1];
                }
                else if (args.Length == 3)
                {
                    chan = args[1];
                    who = args[2];
                }
                else
                {
                    bot.Say(ns, helpmsg);
                    return;
                }

                lock (CommandChannels["send"])
                {
                    CommandChannels["send"].Add(ns);
                }

                bot.Ban(chan, who);
            }
        }
    }
}

