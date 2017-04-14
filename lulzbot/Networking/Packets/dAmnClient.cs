using System;
using System.Text;

namespace lulzbot.Networking
{
    public partial class dAmnPackets
    {
        public static byte[] dAmnClient (double version, String agent, String owner)
        {
            return Encoding.UTF8.GetBytes(String.Format("dAmnClient {0}\nagent={1}\nowner={2}\n\0", version, agent, owner));
        }
    }
}
