using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_admin (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (args.Length < 2)
            {
                bot.Say(ns, String.Format("<b>&raquo; Usage:</b> {0}admin <i>[#channel]</i> command", bot.Config.Trigger));
            }
            else
            {
                String chan, mesg;

                if (!args[1].StartsWith("#"))
                {
                    chan = ns;
                    mesg = msg.Substring(6);
                }
                else
                {
                    chan = args[1];
                    mesg = msg.Substring(7 + args[1].Length);
                }

                lock (CommandChannels["send"])
                {
                    CommandChannels["send"].Add(ns);
                }

                bot.Admin(chan, mesg);
            }
        }
    }
}

