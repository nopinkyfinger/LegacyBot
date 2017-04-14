using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace lulzbot
{
    /// <summary>
    /// Configuration class. Create an object and save/load to/from a file.
    /// </summary>
    public class Config
    {
        public String Username       = String.Empty;
        public String Password       = String.Empty;
        public String Authtoken      = String.Empty;
        public String Owner          = String.Empty;
        public String Trigger        = String.Empty;
        public List<String> Channels = new List<string>();

        /// <summary>
        /// Method to save the configuration to a file.
        /// </summary>
        /// <param name="filename">The file that we will save the data to.</param>
        public void Save (String filename)
        {
            // We need a buffer to put all the data in.
            String buffer = String.Empty;

            // Now, let's throw all the data in there. Delimeter is \0
            buffer = String.Format("{0}\0{1}\0{2}\0{3}\0{4}\0{5}",
                Username, Password, Authtoken, Owner, Trigger, String.Join(",", Channels));

            // Now we just need to write it to a file!
            // I'm going to use "using" here, which automatically handles closing the streams
            //  after everything inside the block has executed.
            using (Stream stream = new FileStream(filename, FileMode.Create))
            {
                using (StreamWriter file = new StreamWriter(stream))
                {
                    file.Write(Encryption.Encrypt(buffer));
                }
            }

            // That's all, folks.
        }

        /// <summary>
        /// Method to load the configuration from a file.
        /// </summary>
        /// <param name="filename">The file that we will load the data from.</param>
        /// <returns>boolean; whether or not the file existed.</returns>
        public bool Load (String filename)
        {
            // First off, does the file even exist?
            if (!File.Exists(filename))
            {
                ConIO.Write("Error: File does not exist:" + filename, "Config");
                return false;
            }
            else
            {
                // We need a buffer for the data.
                String buffer = String.Empty;

                // Now to read the file. Let's just use "using" blocks again.
                using (Stream stream = new FileStream(filename, FileMode.Open))
                {
                    using (StreamReader file = new StreamReader(stream))
                    {
                        buffer = Encryption.Decrypt(file.ReadToEnd());
                    }
                }

                // Now, let's break it up into bits. Let's use a standard array of Strings
                String[] bits = buffer.Split('\0');

                // Make sure we have the right number of bits.
                // Also, unlike Lists, arrays use "Length" instead of "Count" for this.
                if (bits.Length != 6)
                {
                    ConIO.Write("Error: Invalid bit count.", "Config");
                    return false;
                }

                // And now, we assign the data.
                Username = bits[0];
                Password = bits[1];
                Authtoken = bits[2];
                Owner = bits[3];
                Trigger = bits[4];

                // Do channels need to be split up?
                if (bits[5].Contains(','))
                {
                    // Yep.
                    foreach (String chan in bits[5].Split(','))
                    {
                        Channels.Add(chan.ToLower());
                    }
                }
                else
                {
                    Channels.Add(bits[5]);
                }

                return true;
            }
        }
    }
}
