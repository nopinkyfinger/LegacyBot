using System;
using System.Text;

namespace lulzbot.Networking
{
    public partial class dAmnPackets
    {
        public static byte[] Set (String chan, String prop, String value)
        {
            return Encoding.UTF8.GetBytes(String.Format("set {0}\np={1}\n\n{2}\n\0", chan, prop, value));
        }
    }
}
