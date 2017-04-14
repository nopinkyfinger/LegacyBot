using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_chat (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String pcns = Tools.FormatPCNS(from, bot.Config.Username);

            lock (CommandChannels["join"])
            {
                CommandChannels["join"].Add(ns);
            }

            bot.Join(pcns);
        }
    }
}

