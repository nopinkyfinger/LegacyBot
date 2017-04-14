using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_debug (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (args.Length < 2)
            {
                bot.Say(ns, String.Format("<b>&raquo; Usage:</b> {0}debug on/off", bot.Config.Trigger));
            }
            else
            {
                String cmd = args[1];
                Program.Debug = cmd == "on";
                bot.Say(ns, String.Format("<b>&raquo; Debug mode has been {0}.</b>", Program.Debug ? "enabled" : "disabled"));
            }
        }
    }
}

