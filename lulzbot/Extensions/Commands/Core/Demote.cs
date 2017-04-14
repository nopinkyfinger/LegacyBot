using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_demote (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String helpmsg = String.Format("<b>&raquo; Usage:</b> {0}demote <i>[#channel]</i> username <i>privclass</i>", bot.Config.Trigger);

            if (args.Length < 2)
            {
                bot.Say(ns, helpmsg);
            }
            else
            {
                String chan, who, pc;

                if (!args[1].StartsWith("#"))
                {
                    chan = ns;
                    who = args[1];
                    pc = (args.Length >= 3 ? args[2] : null);
                }
                else if (args.Length >= 3)
                {
                    chan = args[1];
                    who = args[2];
                    pc = (args.Length >= 4 ? args[3] : null);
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

                bot.Demote(chan, who, pc);
            }
        }
    }
}

