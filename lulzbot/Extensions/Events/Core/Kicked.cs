using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void evt_kicked (Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (!Program.NoDisplay.Contains(Tools.FormatNamespace(packet.Parameter.ToLower(), Types.NamespaceFormat.Channel)))
            {
                if (packet.Body.Length > 0)
                    ConIO.Write(String.Format("*** Kicked by {0}: {1}", packet.Arguments["by"], packet.Body), Tools.FormatChat(packet.Parameter));
                else
                    ConIO.Write(String.Format("*** Kicked by {0}", packet.Arguments["by"]), Tools.FormatChat(packet.Parameter));
            }

            // In the event that we cannot (or will not) rejoin, remove channel data.
            lock (ChannelData)
            {
                if (ChannelData.ContainsKey(packet.Parameter.ToLower()))
                    ChannelData.Remove(packet.Parameter.ToLower());
            }

            // Rejoin!
            if (bot.AutoReJoin)
                bot.Join(packet.Parameter);
        }
    }
}

