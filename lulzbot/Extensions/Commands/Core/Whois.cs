using lulzbot.Networking;
using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_whois (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (args.Length < 2)
            {
                bot.Say(ns, String.Format("<b>&raquo; Usage:</b> {0}whois username", bot.Config.Trigger));
            }
            else
            {
                lock (CommandChannels["whois"])
                {
                    CommandChannels["whois"].Add(ns);

                    bot.Send(dAmnPackets.Whois(args[1]));
                }
            }
        }
    }
}

