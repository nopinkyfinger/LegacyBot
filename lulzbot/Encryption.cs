using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace lulzbot
{
    public class Encryption
    {
        private static readonly byte[] key = new byte[32] {
            171, 035, 003, 105, 148, 253, 091, 223, 
            045, 017, 224, 175, 234, 096, 053, 053, 
            008, 065, 028, 217, 180, 151, 175, 036, 
            237, 065, 096, 081, 253, 225, 098, 132};

        private static readonly byte[] iv = new byte[16] {
            149, 211, 228, 015, 132, 103, 056, 085, 
            211, 173, 023, 166, 012, 006, 059, 142};

        public static String Encrypt (String data)
        {
            RijndaelManaged crypt = new RijndaelManaged()
            {
                Padding = PaddingMode.PKCS7
            };

            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, crypt.CreateEncryptor(key, iv), CryptoStreamMode.Write);

            byte[] encrypted_data = Encoding.UTF8.GetBytes(data);
            cs.Write(encrypted_data, 0, encrypted_data.Length);
            cs.FlushFinalBlock();
            cs.Close();

            return Convert.ToBase64String(ms.ToArray());
        }

        public static String Decrypt (String data)
        {
            byte[] data_bytes = Convert.FromBase64String(data);

            RijndaelManaged crypt = new RijndaelManaged()
            {
                Padding = PaddingMode.PKCS7
            };

            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, crypt.CreateDecryptor(key, iv), CryptoStreamMode.Write);

            cs.Write(data_bytes, 0, data_bytes.Length);
            cs.FlushFinalBlock();
            cs.Close();

            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }
}
