using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_sudo (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (args.Length < 3)
            {
                bot.Say(ns, String.Format("<b>&raquo; Usage:</b> {0}sudo [username] [command]", bot.Config.Trigger));
            }
            else
            {
                String who = args[1], cmd = msg.Substring(6 + who.Length);
                dAmnPacket pkt = packet;
                pkt.Arguments["from"] = who;
                pkt.Body = bot.Config.Trigger + cmd;
                Events.CallCommand(args[2], pkt);
            }
        }
    }
}

