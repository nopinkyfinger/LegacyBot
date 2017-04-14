using System;
using System.Diagnostics;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void evt_join (Bot bot, dAmnPacket packet)
        {
            if (BDS.syncing && packet.Parameter.StartsWith("pchat:") && packet.Parameter.ToLower().Contains(BDS.syncwith))
            {
                BDS.syncwatch = Stopwatch.StartNew();
                bot.NPSay(packet.Parameter, "BDS:SYNC:BEGIN");
                return;
            }

            // Don't display DataShare messages.
            if (Program.NoDisplay.Contains(Tools.FormatNamespace(packet.Parameter.ToLower(), Types.NamespaceFormat.Channel))) return;

            if (packet.Arguments["e"] == "ok")
            {
                ConIO.Write(String.Format("** Joined [{0}]", packet.Arguments["e"]), Tools.FormatChat(packet.Parameter));

                // Initialize channel data
                lock (ChannelData)
                {
                    if (!ChannelData.ContainsKey(packet.Parameter.ToLower()))
                    {
                        ChannelData.Add(packet.Parameter.ToLower(), new Types.ChatData());
                        ChannelData[packet.Parameter.ToLower()].Name = packet.Parameter;
                    }
                }

                lock (CommandChannels["join"])
                {
                    if (CommandChannels["join"].Count != 0)
                    {
                        String chan = CommandChannels["join"][0];

                        bot.Say(chan, String.Format("<b>&raquo; Joined {0} [ok]</b>", Tools.FormatChat(packet.Parameter)));

                        CommandChannels["join"].RemoveAt(0);
                    }
                }
            }
            else
            {
                ConIO.Write(String.Format("** Failed to join [{0}]", packet.Arguments["e"]), Tools.FormatChat(packet.Parameter));

                lock (CommandChannels["join"])
                {
                    if (CommandChannels["join"].Count != 0)
                    {
                        String chan = CommandChannels["join"][0];

                        bot.Say(chan, String.Format("<b>&raquo; Failed to join {0} [{1}]</b>", Tools.FormatChat(packet.Parameter), packet.Arguments["e"]));

                        CommandChannels["join"].RemoveAt(0);
                    }
                }
            }
        }
    }
}

