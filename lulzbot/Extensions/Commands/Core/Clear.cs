using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_clear (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            Console.Clear();

            bot.Say(ns, "<b>&raquo; Cleared the console!</b>");
        }
    }
}

