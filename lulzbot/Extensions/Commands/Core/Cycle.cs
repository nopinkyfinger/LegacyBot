using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_cycle (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            var chan = "";

            if (args.Length > 1 && args[1].StartsWith("#"))
            {
                chan = args[1];
            }
            else
            {
                chan = ns;
            }

            String cpns = Tools.FormatNamespace(chan, Types.NamespaceFormat.Packet).ToLower();

            if (!Core.ChannelData.ContainsKey(cpns))
            {
                bot.Say(ns, "<b>&raquo; It doesn't look like I'm in that channel.</b>");
                return;
            }

            lock (CommandChannels["part"])
            {
                CommandChannels["part"].Add(ns);
            }

            lock (CommandChannels["join"])
            {
                CommandChannels["join"].Add(ns);
            }

            bot.Part(cpns);
            bot.Join(cpns);
        }
    }
}

