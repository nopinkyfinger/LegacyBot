using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_act (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (args.Length < 2)
            {
                bot.Say(ns, String.Format("<b>&raquo; Usage:</b> {0}act <i>[#channel]</i> msg", bot.Config.Trigger));
            }
            else
            {
                String chan, mesg;

                if (!args[1].StartsWith("#"))
                {
                    chan = ns;
                    mesg = msg.Substring(4);
                }
                else
                {
                    chan = args[1];
                    mesg = msg.Substring(5 + args[1].Length);
                }

                lock (CommandChannels["send"])
                {
                    CommandChannels["send"].Add(ns);
                }

                bot.Act(chan, mesg);
            }
        }
    }
}

