
namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void evt_ping (Bot bot, dAmnPacket packet)
        {
            // Don't see a reason to write a packet object for this.
            bot.Send("pong\n\0");
        }
    }
}

