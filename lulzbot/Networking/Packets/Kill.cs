using System;
using System.Text;

namespace lulzbot.Networking
{
    public partial class dAmnPackets
    {
        public static byte[] Kill (String username, String reason)
        {
            return Encoding.UTF8.GetBytes(String.Format("kill login:{0}\n\n{1}\n\0", username, reason));
        }
    }
}
