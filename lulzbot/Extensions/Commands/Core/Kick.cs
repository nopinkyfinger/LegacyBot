using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_kick (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String helpmsg = String.Format("<b>&raquo; Usage:</b> {0}kick <i>[#channel]</i> username <i>[reason]</i>", bot.Config.Trigger);

            if (args.Length < 2)
            {
                bot.Say(ns, helpmsg);
            }
            else
            {
                String chan, who, reason;

                if (!args[1].StartsWith("#"))
                {
                    chan = ns;
                    who = args[1];
                    reason = (args.Length >= 3 ? ": " + msg.Substring(6 + who.Length) : "");
                }
                else if (args.Length >= 3)
                {
                    chan = args[1];
                    who = args[2];
                    reason = (args.Length >= 4 ? ": " + msg.Substring(7 + who.Length + chan.Length) : "");
                }
                else
                {
                    bot.Say(ns, helpmsg);
                    return;
                }

                lock (CommandChannels["kick"])
                {
                    CommandChannels["kick"].Add(ns);
                }

                bot.Kick(chan, who, "<b>" + from + "</b>" + reason);
            }
        }
    }
}

