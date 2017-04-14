using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_ping (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            bot.Say(ns, "Ping...");
            bot.PingTimer.Start();
        }
    }
}

