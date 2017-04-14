using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_disconnects (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (Program.Disconnects == 0)
                bot.Say(ns, "<b>&raquo; I have not disconnected since startup.</b>");
            else
                bot.Say(ns, String.Format("<b>&raquo; I have disconnected {0} time{1} since startup.</b>", Program.Disconnects, Program.Disconnects == 1 ? "" : "s"));
        }
    }
}

