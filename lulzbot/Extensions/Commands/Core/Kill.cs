using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_kill (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (args.Length < 2)
            {
                bot.Say(ns, String.Format("<b>&raquo; Usage:</b> {0}kill username <i>reason</i>", bot.Config.Trigger));
            }
            else
            {
                lock (CommandChannels["kill"])
                {
                    CommandChannels["kill"].Add(ns);
                }

                bot.Kill(args[1], "<b>" + from + "</b>" + (args.Length >= 3 ? ": " + msg.Substring(6 + args[1].Length) : ""));
            }
        }
    }
}

