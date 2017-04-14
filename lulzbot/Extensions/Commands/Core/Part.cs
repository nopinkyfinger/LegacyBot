using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_part (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            var c = ns;

            if (args.Length != 2)
            {
                // Ignore this for now.
                //bot.Say(ns, String.Format("<b>&raquo; Usage:</b> {0}part #channel", bot.Config.Trigger));
            }
            else
            {
                if (!args[1].StartsWith("#"))
                {
                    bot.Say(ns, "<b>&raquo; Invalid channel!</b> Channels should start with a #");
                    return;
                }

                c = args[1];
            }

            lock (CommandChannels["part"])
            {
                CommandChannels["part"].Add(ns);
            }

            bot.Part(c);
        }
    }
}

