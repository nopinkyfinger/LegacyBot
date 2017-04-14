using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_join (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (args.Length != 2)
            {
                bot.Say(ns, String.Format("<b>&raquo; Usage:</b> {0}join #channel", bot.Config.Trigger));
            }
            else
            {
                if (!args[1].StartsWith("#"))
                {
                    bot.Say(ns, "<b>&raquo; Invalid channel!</b> Channels should start with a #");
                    return;
                }

                if (Program.OfficialChannels.Contains(args[1].ToLower()))
                {
                    bot.Say(ns, "<b>&raquo; Joining official channels is strictly prohibited.</b>");
                    return;
                }

                lock (CommandChannels["join"])
                {
                    CommandChannels["join"].Add(ns);
                }

                bot.Join(args[1]);
            }
        }
    }
}

