using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void evt_disconnect (Bot bot, dAmnPacket packet)
        {
            ConIO.Write(String.Format("*** Disconnected [{0}]", packet.Arguments["e"]));

            // Add an override for a restart command later?
            if (bot.Quitting)
            {
                bot.Close();
                Program.Running = false;
                Program.wait_event.Set();
            }
            else
                bot.Reconnect();
        }
    }
}

