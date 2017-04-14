using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void evt_login (Bot bot, dAmnPacket packet)
        {
            if (packet.Arguments["e"] != "ok")
            {
                ConIO.Warning("dAmn", String.Format("Failed to login as {0} [{1}]", packet.Parameter, packet.Arguments["e"]));

                if (packet.Arguments["e"] == "authentication failed")
                {
                    ConIO.Write("Attempting to renew authtoken...");

                    if (Program.RenewToken())
                    {
                        Program.ForceReconnect = true;
                        Program.wait_event.Set();
                        return;
                    }
                }

                Program.Running = false;
                Program.wait_event.Set();
            }
            else
            {
                ConIO.Write(String.Format("Logged in as {0} [{1}]", packet.Parameter, packet.Arguments["e"]));

                bot.Join("chat:DSGateWay");

                foreach (String channel in bot.Config.Channels)
                {
                    bot.Join(channel);
                }
            }
        }
    }
}

