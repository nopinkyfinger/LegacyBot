using System;
using System.Text;

namespace lulzbot.Networking
{
    public partial class dAmnPackets
    {
        public static byte[] Promote (String chan, String username, String privclass = null)
        {
            if (privclass != null)
                return Encoding.UTF8.GetBytes(String.Format("send {0}\n\npromote {1}\n\n{2}\0", chan, username, privclass));
            else
                return Encoding.UTF8.GetBytes(String.Format("send {0}\n\npromote {1}\n\0", chan, username));
        }

        public static byte[] Demote (String chan, String username, String privclass = null)
        {
            if (privclass != null)
                return Encoding.UTF8.GetBytes(String.Format("send {0}\n\ndemote {1}\n\n{2}\0", chan, username, privclass));
            else
                return Encoding.UTF8.GetBytes(String.Format("send {0}\n\ndemote {1}\n\0", chan, username));
        }

        public static byte[] Ban (String chan, String username)
        {
            return Encoding.UTF8.GetBytes(String.Format("send {0}\n\nban {1}\n\0", chan, username));
        }

        public static byte[] UnBan (String chan, String username)
        {
            return Encoding.UTF8.GetBytes(String.Format("send {0}\n\nunban {1}\n\0", chan, username));
        }
    }
}
