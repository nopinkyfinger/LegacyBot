using System;
using System.Text;

namespace lulzbot.Networking
{
    public partial class dAmnPackets
    {
        public static byte[] Kick (String chan, String username, String reason)
        {
            return Encoding.UTF8.GetBytes(String.Format("kick {0}\nu={1}\n\n{2}\n\0", chan, username, reason));
        }
    }
}
