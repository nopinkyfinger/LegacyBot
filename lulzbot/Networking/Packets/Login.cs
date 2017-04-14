using System;
using System.Text;

namespace lulzbot.Networking
{
    public partial class dAmnPackets
    {
        public static byte[] Login (String username, String authtoken)
        {
            return Encoding.UTF8.GetBytes(String.Format("login {0}\npk={1}\n\0", username, authtoken));
        }
    }
}
