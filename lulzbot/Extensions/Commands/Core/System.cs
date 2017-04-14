using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_system (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String output = String.Empty;

            output += String.Format("<b>&raquo; System:</b> {0}. <b>Architecture:</b> {1}bit. <b>CLR Version:</b> {2}<br/>", Program.OS, Environment.Is64BitOperatingSystem ? 64 : 32, Environment.Version.ToString());

            bot.Say(ns, output);
        }
    }
}

