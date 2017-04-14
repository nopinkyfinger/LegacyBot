using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_help (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (args.Length < 2)
            {
                bot.Say(ns, String.Format("<b>&raquo; Usage:</b> {0}help command_name", bot.Config.Trigger));
            }
            else
            {
                String cmd = args[1];

                if (!Events.ValidateCommandName(cmd))
                {
                    bot.Say(ns, "<b>&raquo; I don't have any command named:</b> " + cmd);
                    return;
                }

                String desc = Events.CommandDescription(cmd);

                if (desc.Length <= 1) desc = "No description. Bug the author!";

                bot.Say(ns, String.Format("<b>&raquo; Help for command {0}:</b><br/> &middot; {1}", cmd, desc));
            }
        }
    }
}

