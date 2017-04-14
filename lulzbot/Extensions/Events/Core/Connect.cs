using lulzbot.Networking;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void evt_connect (Bot bot, dAmnPacket packet)
        {
            try
            {
                if (bot == null)
                {
                    Program.ForceReconnect = true;
                    Program.wait_event.Set();
                    return;
                }

                if (Program.Debug)
                    ConIO.Write("Connected to the server: " + bot.Endpoint());

                bot.Send(dAmnPackets.dAmnClient(0.3, Program.BotName, bot.Config.Owner));
            }
            catch
            {
                bot.Close();
            }
        }
    }
}

