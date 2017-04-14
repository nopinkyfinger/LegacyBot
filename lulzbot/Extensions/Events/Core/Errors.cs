using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void evt_send_error (Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (Program.NoDisplay.Contains(Tools.FormatNamespace(packet.Parameter.ToLower(), Types.NamespaceFormat.Channel))) return;

            if (CommandChannels["send"].Count > 0)
            {
                String chan = CommandChannels["send"][0];
                CommandChannels["send"].RemoveAt(0);

                bot.Say(chan, String.Format("<b>&raquo; Failed to send to {0}:</b> {1}", Tools.FormatChat(packet.Parameter), packet.Arguments["e"]));
            }

            ConIO.Write(String.Format("*** Failed to send to {0} [{1}]", Tools.FormatChat(packet.Parameter), packet.Arguments["e"]));
        }

        public static void evt_kick_error (Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (Program.NoDisplay.Contains(Tools.FormatNamespace(packet.Parameter.ToLower(), Types.NamespaceFormat.Channel))) return;

            if (CommandChannels["kick"].Count > 0)
            {
                String chan = CommandChannels["kick"][0];
                CommandChannels["kick"].RemoveAt(0);

                bot.Say(chan, String.Format("<b>&raquo; Failed to kick {0} in {1}:</b> {2}", packet.Arguments["u"], Tools.FormatChat(packet.Parameter), packet.Arguments["e"]));
            }

            ConIO.Write(String.Format("*** Failed to kick {0} from {1} [{2}]", packet.Arguments["u"], Tools.FormatChat(packet.Parameter), packet.Arguments["e"]));
        }

        public static void evt_get_error (Bot bot, dAmnPacket packet)
        {
            if (packet.Parameter.StartsWith("login:"))
            {
                lock (CommandChannels["whois"])
                {
                    if (CommandChannels["whois"].Count > 0)
                    {
                        String chan = CommandChannels["whois"][0];
                        CommandChannels["whois"].RemoveAt(0);

                        bot.Say(chan, String.Format("<b>&raquo; Failed to whois {0}:</b> {1}", packet.Parameter.Substring(6), packet.Arguments["e"]));
                    }
                }
            }
            else
            {
                if (Program.NoDisplay.Contains(Tools.FormatNamespace(packet.Parameter.ToLower(), Types.NamespaceFormat.Channel))) return;

                ConIO.Write(String.Format("*** Failed to get {0} in {1} [{2}]", packet.Arguments["p"], Tools.FormatChat(packet.Parameter), packet.Arguments["e"]));
            }
        }

        public static void evt_set_error (Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (Program.NoDisplay.Contains(Tools.FormatNamespace(packet.Parameter.ToLower(), Types.NamespaceFormat.Channel))) return;

            if (CommandChannels["set"].Count > 0)
            {
                String chan = CommandChannels["set"][0];
                CommandChannels["set"].RemoveAt(0);

                bot.Say(chan, String.Format("<b>&raquo; Failed to set {0} of {1}:</b> {2}", packet.Arguments["p"], Tools.FormatChat(packet.Parameter), packet.Arguments["e"]));
            }

            ConIO.Write(String.Format("*** Failed to set {0} in {1} [{2}]", packet.Arguments["p"], Tools.FormatChat(packet.Parameter), packet.Arguments["e"]));
        }

        public static void evt_kill_error (Bot bot, dAmnPacket packet)
        {
            ConIO.Write(String.Format("*** Failed to kill {0} [{1}]", Tools.FormatChat(packet.Parameter), packet.Arguments["e"]));

            if (CommandChannels["kill"].Count > 0)
            {
                String chan = CommandChannels["kill"][0];
                CommandChannels["kill"].RemoveAt(0);

                bot.Say(chan, String.Format("<b>&raquo; Failed to kill {0}:</b> {1}", Tools.FormatChat(packet.Parameter), packet.Arguments["e"]));
            }
        }
    }
}

