using lulzbot.Extensions;
using lulzbot.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace lulzbot
{
    /// <summary>
    /// Various tools for use throughout the bot.
    /// </summary>
    public class Tools
    {
        // This will keep track of how many arguments each tablump uses.
        private static Dictionary<String, int> lump_arg_count = new Dictionary<string, int>();
        private static DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0);

        /// <summary>
        /// Initialize the tablumps lists
        /// </summary>
        public static void InitLumps ()
        {
            lump_arg_count.Clear();

            // Zero args
            foreach (String lump in new List<String>() {
                // Opening tags
                "b","s","i","u","p","li","ul","ol","br","sup","sub","code","bcode",
                // Closing tags
                "/b","/i","/u","/s","/p","/li","/ul","/ol","/br","/sup","/sub","/code","/bcode","/iframe","/embed","/a","/acro","/abbr"})
            {
                lump_arg_count.Add(lump, 0);
            }

            // One arg
            foreach (String lump in new List<String>() { "abbr", "acro" })
            {
                lump_arg_count.Add(lump, 1);
            }

            // Two args
            foreach (String lump in new List<String>() { "a", "dev", "avatar", "link" })
            {
                // Technically, link is two OR three. But we'll deal with that.
                lump_arg_count.Add(lump, 2);
            }

            // Three args
            foreach (String lump in new List<String>() { "img", "iframe", "embed" })
            {
                lump_arg_count.Add(lump, 3);
            }

            // Four args
            // None

            // Five args
            foreach (String lump in new List<String>() { "emote" })
            {
                lump_arg_count.Add(lump, 5);
            }

            // Six args
            foreach (String lump in new List<String>() { "thumb" })
            {
                lump_arg_count.Add(lump, 6);
            }
        }

        /// <summary>
        /// Format a chat namespace.
        /// </summary>
        /// <param name="channel">Namespace to format</param>
        /// <returns>Formatted namespace
        /// Deprecated. Use FormatNamespace instead.</returns>
        public static String FormatChat (String channel)
        {
            if (channel.StartsWith("chat:"))
                return FormatNamespace(channel, NamespaceFormat.Channel);
            else if (channel.StartsWith("#"))
                return FormatNamespace(channel, NamespaceFormat.Packet);
            else if (channel.StartsWith("login:"))
                return FormatNamespace(channel, NamespaceFormat.Username);
            else if (channel.StartsWith("pchat:"))
                return FormatNamespace(channel, NamespaceFormat.PrivateChat);
            else
                return channel;
        }

        public static String FormatPCNS (String who1, String who2)
        {
            var names = new System.Collections.Generic.List<String>() { who1, who2 };
            names.Sort();
            return "pchat:" + names[0] + ":" + names[1];
        }

        /// <summary>
        /// Formats a namespace to the requested format.
        /// </summary>
        /// <param name="ns">Namespace to be formatted</param>
        /// <param name="format">Format type</param>
        /// <returns>Formatted namespace</returns>
        public static String FormatNamespace (String ns, NamespaceFormat format)
        {
            if (ns.Length < 2)
                throw new FormatException("Namespace provided was invalid.");

            if (format == NamespaceFormat.Channel)
            {
                if (ns.Length > 1 && ns[0] == '#')
                    return ns;

                else if (ns.Length > 1 && ns[0] == '@')
                    return ns;

                else if (ns.StartsWith("chat:") && ns.Length > 5)
                    return '#' + ns.Substring(5);

                else if (ns.StartsWith("pchat:") && ns.Length > 6)
                {
                    String[] names = ns.Split(new char[] { ':' });

                    if (names[1].ToLower() == Program.Bot.Config.Username.ToLower())
                        return '@' + names[2];
                    else
                        return '@' + names[1];
                }

                else throw new FormatException("Namespace provided cannot be converted to channel format.");
            }

            else if (format == NamespaceFormat.Packet)
            {
                if (ns.Length > 1 && ns[0] == '#')
                    return "chat:" + ns.Substring(1);

                else if (ns.Length > 1 && ns[0] == '@')
                {
                    List<String> names = new List<String>() { Program.Bot.Config.Username, ns.Substring(1) };
                    names.Sort();
                    return "pchat:" + names[0] + ":" + names[1];
                }

                else if (ns.StartsWith("chat:") && ns.Length > 5)
                    return ns;

                else if (ns.StartsWith("pchat:") && ns.Length > 6)
                    return ns;

                else throw new FormatException("Namespace provided cannot be converted to packet format.");
            }

            else if (format == NamespaceFormat.Username)
            {
                if (ns.StartsWith("login:") && ns.Length > 6)
                    return ns.Substring(6);

                else throw new FormatException("Namespace provided cannot be converted to username format.");
            }

            else if (format == NamespaceFormat.PrivateChat)
            {
                String[] bits;

                if (ns.Length > 1 && ns[0] == '@')
                    return ns;

                else if (ns.Contains(":") && (bits = ns.Split(new char[] { ':' })).Length == 3)
                {
                    if (bits[1].ToLower() == Program.Bot.Config.Username.ToLower())
                        return '@' + bits[2];
                    else if (bits[2].ToLower() == Program.Bot.Config.Username.ToLower())
                        return '@' + bits[1];

                    else throw new FormatException("Namespace provided cannot be converted to private chat format.");
                }

                else throw new FormatException("Namespace provided cannot be converted to private chat format.");
            }

            throw new NotSupportedException("Unsupported namespace format.");
        }

        public static String ParseEntities (String message, bool amp = false)
        {
            // Doesn't need to be parsed?
            if (!message.Contains("&"))
                return message;

            String parsed = message;

            // The basics
            parsed = parsed.Replace("&lt;", "<");
            parsed = parsed.Replace("&gt;", ">");
            parsed = parsed.Replace("&raquo;", "»");
            parsed = parsed.Replace("&laquo;", "«");
            parsed = parsed.Replace("&middot;", "·");
            parsed = parsed.Replace("&nbsp;", " ");
            parsed = parsed.Replace("\a", "");

            if (amp)
            {
                // I don't even...
                parsed = parsed.Replace("&amp;", "&");
                parsed = parsed.Replace("&amp;", "&");
            }

            return parsed;
        }

        public static String HtmlEncode (String msg)
        {
            var x = "";
            foreach (char c in msg.ToCharArray())
            {
                if ((int)c > 127)
                    x += "&#" + (int)c + ";";
                else
                    x += c;
            }
            return x;
        }

        public static String UnicodeString (byte[] data)
        {
            var x = "";
            foreach (byte b in data)
            {
                if (b > 127)
                    x += "&#" + (int)b + ";";
                else
                    x += (char)b;
            }
            return x;
        }

        /// <summary>
        /// Checks if the bot is up to date.
        /// </summary>
        /// <param name="ver">Bot version</param>
        /// <returns>OK if up to date [or newer]; ERR if error. Otherwise, update message.</returns>
        public static String UpToDate (String ver)
        {
            var p = Tools.GrabPage(@"http://botdom.com/w/api.php?action=query&prop=revisions&rvlimit=1&rvprop=content&format=json&titles=LulzBot");

            if (p == null) return "ERR";
            
            var searchA = "last_release_s = ";
            var searchB = "last_releasedate_s = ";
            
            int indexA, indexB;

            if (!p.Contains("\\n")) return "ERR";

            if ((indexA = p.IndexOf(searchA)) != -1)
            {
                if (indexA + searchA.Length + 16 < p.Length)
                {
                    var nver = p.Substring(indexA + searchA.Length, 16);
                    nver = nver.Substring(0, nver.IndexOf("\\n")).Trim().ToLower();

                    if (nver != ver.ToLower())
                    {
                        double realver = 0.0, realnver = 0.0;

                        if (!double.TryParse(RegexReplace(ver, @"[^0-9\.]", ""), out realver)) return "ERR";
                        if (!double.TryParse(RegexReplace(nver, @"[^0-9\.]", ""), out realnver)) return "ERR";

                        if (realver >= realnver) return "OK";

                        if ((indexB = p.IndexOf(searchB)) != -1)
                        {
                            if (indexB + searchB.Length + 16 < p.Length)
                            {
                                var nrel = p.Substring(indexB + searchB.Length, 16);
                                nrel = nrel.Substring(0, nrel.IndexOf("\\")).Trim();

                                return String.Format("Version {0} is now available! Released {1}.", nver, nrel);
                            }
                            else return String.Format("Version {0} is now available!", nver);
                        }
                        else return String.Format("Version {0} is now available!", nver);
                    }
                    else return "OK";
                }
                else return "ERR";
            }
            else return "ERR";
        }

        /// <summary>
        /// Parse tablumps in a message.
        /// </summary>
        /// <param name="message">Unparsed message</param>
        /// <returns>Parsed message</returns>
        public static String ParseTablumps (String message)
        {
            // Do the basics to get certains & out of our way.
            message = ParseEntities(message);

            // If there's no ampersand or tab, there's no tablumps.
            if (!message.Contains("&") || !message.Contains("\t"))
                return ParseEntities(message, true);

            // Split the message by \t, into an array of bits. (chunks)
            String[] bits = message.Split('\t');

            // Allocate strings for reuse
            String parsed = String.Empty;
            String bit    = String.Empty;
            String lump   = String.Empty;

            // Assign some basic values for reuse
            int index = 0, last_index = 0, amp_pos = 0;

            // Loop through each bit of the string
            for (int p = 0; p < bits.Length; p++)
            {
                // Grab the string at the current position
                bit = bits[p];

                // We don't do anything if there's no & in the string.
                if (bit.Contains("&"))
                {
                    // Everything before the lump gets thrown into the parsed string as-is
                    amp_pos = bit.IndexOf('&');
                    parsed += bit.Substring(0, amp_pos);

                    // If there is one or more & in the string, let's loop through their indices
                    while ((index = bit.IndexOf('&', last_index)) != -1)
                    {
                        lump = bit.Substring(index + 1);
                        last_index = index + 1;

                        // If there's another lump inside out substring, only go up to that position.
                        if (lump.Contains("&"))
                        {
                            lump = lump.Substring(0, lump.IndexOf('&'));
                        }

                        // We don't know this lump. No reason to parse it, so just add it as-is.
                        if (!lump_arg_count.ContainsKey(lump))
                        {
                            // Make sure to give the & back to non-tablumps!
                            parsed += "&" + lump;
                            continue;
                        }

                        // Get the argument count for this lump
                        int arg_count = lump_arg_count[lump];

                        // If the arg count is zero, we only need to surround it with <>
                        if (arg_count == 0)
                        {
                            parsed += "<" + lump + ">";
                        }
                        else
                        {
                            // Otherwise, we need to act according to the tag.
                            if (lump == "abbr" || lump == "acro")
                            {
                                // One argument.
                                // We need to parse the tablumps of the title, since it'd go untouched.
                                parsed += "<" + lump + " title=\"" + bits[p + 1] + "\">";
                            }
                            else if (lump == "a")
                            {
                                // One argument.
                                parsed += "<a href=\"" + bits[p + 1] + "\">";
                            }
                            else if (lump == "avatar")
                            {
                                // Two arguments. We use the first
                                parsed += ":icon" + bits[p + 1] + ":";
                            }
                            else if (lump == "dev")
                            {
                                // Two argument. We use the second
                                parsed += ":dev" + bits[p + 2] + ":";
                            }
                            else if (lump == "img")
                            {
                                // Three arguments. We're only going to use the first (src)
                                //  and ignore the other two (height and width).
                                parsed += "<img src=\"" + bits[p + 1] + "\" />";
                            }
                            else if (lump == "iframe" || lump == "embed")
                            {
                                // Three arguments. We're only going to use the first (src)
                                //  and ignore the other two (height and width).
                                parsed += "<" + lump + " src=\"" + bits[p + 1] + "\">";
                            }
                            else if (lump == "emote")
                            {
                                // Five arguments. We're only going to use the first (emote code)
                                parsed += bits[p + 1];
                            }
                            else if (lump == "thumb")
                            {
                                // Seven arguments. We're only going to use the first (thumb code)
                                parsed += ":thumb" + bits[p + 1] + ":";
                            }
                            else if (lump == "link")
                            {
                                // Two OR three arguments. Odd one.
                                // We just use the first, in a way, instead of getting the title.
                                parsed += "<a href=\"" + bits[p + 1] + "\">";
                                if (bits[p + 2] != "&")
                                {
                                    parsed += bits[p + 2] + "</a>";
                                    arg_count++;
                                }
                                else parsed += "[link]</a>";
                            }
                        }

                        p += arg_count;

                        // If we pass the end of the string, we're done with this bit.
                        if (last_index >= bit.Length)
                            break;
                    }

                    // Clear index and last_index
                    index = 0;
                    last_index = 0;
                }
                else
                {
                    // It's not a tablump, so throw it into the parsed string as-is
                    parsed += bit;
                }
            }

            parsed = ParseEntities(parsed, true);

            // Unfortunately, this has to be done.
            int start;
            if ((start = parsed.IndexOf("<abbr title=\"colors:")) != -1)
            {
                return parsed.Substring(0, start);
            }

            // Return the parsed message
            return parsed;
        }

        /// <summary>
        /// Generates an MD5 hash for the specified input
        /// </summary>
        /// <param name="input">Plaintext string</param>
        /// <returns>MD5 hash in string format</returns>
        public static String md5 (String input)
        {
            StringBuilder output = new StringBuilder();
            using (MD5 md5 = MD5.Create())
            {
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                for (int i = 0; i < data.Length; i++)
                {
                    output.Append(data[i].ToString("x2"));
                }
            }
            return output.ToString();
        }

        public static int Timestamp (bool milliseconds = false)
        {
            if (milliseconds)
                return (int)((DateTime.UtcNow - epoch).TotalMilliseconds);
            else
                return (int)((DateTime.UtcNow - epoch).TotalSeconds);
        }

        /// <summary>
        /// Formats a specified amount of seconds into a human readable string.
        /// i.e. 3669 becomes:
        /// 1 hour, 1 minute, 9 seconds.
        /// </summary>
        /// <param name="seconds">Amount of seconds</param>
        /// <returns>Human readable string</returns>
        public static String FormatTime (ulong seconds)
        {
            String output = String.Empty;

            if (seconds <= 0)
                return "0 seconds";

            int millennia = 0, centuries = 0, decades = 0, years = 0, weeks = 0, days = 0, hours = 0, minutes = 0;

            while (seconds >= 31556926000)
            {
                ++millennia;
                seconds -= 31556926000;
            }

            while (seconds >= 3155692600)
            {
                ++centuries;
                seconds -= 3155692600;
            }

            while (seconds >= 315569260)
            {
                ++decades;
                seconds -= 315569260;
            }

            while (seconds >= 31556926)
            {
                ++years;
                seconds -= 31556926;
            }

            while (seconds >= 604800)
            {
                ++weeks;
                seconds -= 604800;
            }

            while (seconds >= 86400)
            {
                ++days;
                seconds -= 86400;
            }

            while (seconds >= 3600)
            {
                ++hours;
                seconds -= 3600;
            }

            while (seconds >= 60)
            {
                ++minutes;
                seconds -= 60;
            }

            if (millennia > 0)
                output += millennia + " millenni" + (millennia == 1 ? "um" : "a") + ", ";

            if (centuries > 0)
                output += centuries + " centur" + (centuries == 1 ? "y" : "ies") + ", ";

            if (decades > 0)
                output += decades + " decade" + (decades == 1 ? "" : "s") + ", ";

            if (years > 0)
                output += years + " year" + (years == 1 ? "" : "s") + ", ";

            if (weeks > 0)
                output += weeks + " week" + (weeks == 1 ? "" : "s") + ", ";

            if (days > 0)
                output += days + " day" + (days == 1 ? "" : "s") + ", ";

            if (hours > 0)
                output += hours + " hour" + (hours == 1 ? "" : "s") + ", ";

            if (minutes > 0)
                output += minutes + " minute" + (minutes == 1 ? "" : "s") + ", ";

            if (seconds > 0)
                output += seconds + " second" + (seconds == 1 ? "" : "s");
            else if (output.Length > 0)
                output = output.Substring(0, output.Length - 2);

            return output;
        }

        public static String FormatTime (int seconds) { return FormatTime((ulong)seconds); }

        /// <summary>
        /// Formats a specified amount of bytes into a human readable string.
        /// i.e. 1026 becomes:
        /// 1kB, 2B.
        /// </summary>
        /// <returns>Human readable string</returns>
        public static String FormatBytes (ulong bytes, bool verbose = false)
        {
            String output = String.Empty;

            if (bytes == 0)
                return (verbose ? "0 Bytes" : "0B");

            int tb = 0, gb = 0, mb = 0, kb = 0;

            while (bytes >= (ulong)ByteCounts.GigaByte)
            {
                ++gb;
                bytes -= (ulong)ByteCounts.GigaByte;
            }

            while (gb >= 1024)
            {
                ++tb;
                gb -= 1024;
            }

            while (bytes >= (ulong)ByteCounts.MegaByte)
            {
                ++mb;
                bytes -= (ulong)ByteCounts.MegaByte;
            }

            while (bytes >= (ulong)ByteCounts.KiloByte)
            {
                ++kb;
                bytes -= (ulong)ByteCounts.KiloByte;
            }

            if (tb > 0)
                output += tb + (verbose ? " TeraByte" + (tb == 1 ? "" : "s") : "TB") + ", ";

            if (gb > 0)
                output += gb + (verbose ? " GigaByte" + (gb == 1 ? "" : "s") : "GB") + ", ";

            if (mb > 0)
                output += mb + (verbose ? " MegaByte" + (mb == 1 ? "" : "s") : "MB") + ", ";

            if (kb > 0)
                output += kb + (verbose ? " KiloByte" + (kb == 1 ? "" : "s") : "kB") + ", ";

            if (bytes > 0)
                output += bytes + (verbose ? " Byte" + (bytes == 1 ? "" : "s") : "B");
            else if (output.Length > 0)
                output = output.Substring(0, output.Length - 2);

            return output;
        }

        /// <summary>
        /// Writes or appends text to a file
        /// </summary>
        /// <param name="filename">filename with path</param>
        /// <param name="content">content to write</param>
        /// <param name="append">append to the end or overwrite the file</param>
        public static void WriteFile (String filename, String content, bool append = false)
        {
            if (!filename.Contains("/") || filename.Contains("..") || filename.Contains("~") || filename.StartsWith(@"\") || filename.StartsWith("/"))
            {
                ConIO.Warning("Tools.WriteFile", "Cannot write to (or below) bot's root directory.");
                return;
            }

            try
            {
                String dir = Path.GetDirectoryName(filename);

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (!File.Exists(filename) || !append)
                    File.Create(filename).Close();

                using (StreamWriter writer = File.AppendText(filename))
                {
                    writer.Write(content);
                }
            }
            catch (Exception E)
            {
                ConIO.Warning("Tools.WriteFile", "Caught Exception: " + E.ToString());
            }
        }

        /// <summary>
        /// strftime implementation
        /// </summary>
        /// <param name="format">format string</param>
        /// <returns>formatted time string</returns>
        public static String strftime (String format, DateTime date)
        {
            // I haven't extensively tested this, but it should work OK. - Justin

            String final = format;
            DateTime now = date;
            GregorianCalendar cal = new GregorianCalendar(GregorianCalendarTypes.Localized);

            final = final.Replace("%a", now.ToString("ddd"));
            final = final.Replace("%A", now.ToString("dddd"));
            final = final.Replace("%b", now.ToString("MMM"));
            final = final.Replace("%B", now.ToString("MMMM"));
            final = final.Replace("%B", now.ToString("MMMM"));
            final = final.Replace("%c", now.ToString("ddd MMM dd hh:mm:ss yyyy"));
            final = final.Replace("%d", now.ToString("dd"));
            final = final.Replace("%H", now.ToString("HH"));
            final = final.Replace("%I", now.ToString("hh"));
            final = final.Replace("%j", now.DayOfYear.ToString().PadLeft(3, '0'));
            final = final.Replace("%m", now.ToString("MM"));
            final = final.Replace("%M", now.ToString("mm"));
            final = final.Replace("%p", now.ToString("tt"));
            final = final.Replace("%S", now.ToString("ss"));
            final = final.Replace("%U", cal.GetWeekOfYear(now, CalendarWeekRule.FirstDay, System.DayOfWeek.Sunday).ToString().PadLeft(2, '0'));
            final = final.Replace("%w", ((Types.DayOfWeek)now.DayOfWeek).ToString().PadLeft(2, '0'));
            final = final.Replace("%W", cal.GetWeekOfYear(now, CalendarWeekRule.FirstDay, System.DayOfWeek.Monday).ToString().PadLeft(2, '0'));
            final = final.Replace("%x", now.ToString("MM/dd/yy"));
            final = final.Replace("%X", now.ToString("HH:mm:ss"));
            final = final.Replace("%y", now.ToString("yy"));
            final = final.Replace("%Y", now.ToString("yyyy"));
            final = final.Replace("%z", now.ToString("zz"));
            final = final.Replace("%Z", now.ToString("zzz"));

            return final;
        }

        public static String strftime (String format, int timestamp)
        {
            DateTime then = epoch.Date.AddSeconds(timestamp);
            return strftime(format, then);
        }

        public static String strftime (String format)
        {
            return strftime(format, DateTime.Now);
        }

        public static List<String> MutualChannels (String user)
        {
            List<String> chans = new List<String>();
            String who = user.ToLower();

            foreach (lulzbot.Types.ChatData data in Core.ChannelData.Values)
            {
                if (data.Members.ContainsKey(who) && data.Name.ToLower() != "chat:datashare")
                {
                    chans.Add(data.Name);
                }
            }

            return chans;
        }

        public static T Json2Data<T> (String json)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            }
            catch { return default(T); }
        }

        public static String Obj2Json (object obj)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            }
            catch { return ""; }
        }

        public static String StripTags (String data)
        {
            char[] a = new char[data.Length];
            int i = 0;
            bool t = false;
            for (int x = 0; x < data.Length; x++)
            {
                if (data[x] == '<') { t = true; continue; }
                if (data[x] == '>') { t = false; continue; }
                if (!t) { a[i] = data[x]; i++; }
            }
            return new String(a, 0, i);
        }

        public static String GrabPage (String url, bool strip_tags = false, bool gzip = true, String accept = null, String encoding = "ASCII")
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
                String content = String.Empty;
                Encoding enc = Encoding.GetEncoding(encoding, new EncoderReplacementFallback(""), new DecoderReplacementFallback(""));

                HttpWebRequest page_request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url).AbsoluteUri);

                page_request.AllowAutoRedirect = true;
                page_request.MaximumAutomaticRedirections = 5;
                page_request.Method = "GET";
                //page_request.KeepAlive = false;
                page_request.Proxy = null;
                page_request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                // :)
                page_request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1312.57 Safari/537.17";
                page_request.Accept = (accept != null ? accept : "text/plain") + (gzip ? "; gzip, deflate;" : "");
                page_request.ContentType = (accept != null ? accept : "text/plain");

                using (WebResponse resp = page_request.GetResponse())
                {
                    using (Stream s = resp.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(s, enc))
                        {
                            content = reader.ReadToEnd();
                        }
                    }
                }

                return strip_tags ? StripTags(content) : content;
            }
            catch (Exception E)
            {
                if (Program.Debug)
                    ConIO.Warning("GrabPage", E.Message);
                return null;
            }
        }

        private static Regex _devinfo_regex = new Regex(@"(<title>(?<username>[^\s]+)[^<]+</title>)|(<strong>(?<number>[^<]+)</strong>(?<type>[^\t]+))|(<div id=""super-secret-\w+""[^>]*>(?<tagline>[^<]+))|(<d. class=""f h"">(?<item>[^<]+)</d.>)|(<div>Deviant for (?<years>[^<]+)</div><div>(?<member>[^<]+)</div>)", RegexOptions.Compiled);
        private static Regex _multispace_regex = new Regex(@"\s+", RegexOptions.Compiled);
        public static Dictionary<string, string> DeviantInfo (string who)
        {
            var page = GrabPage("http://" + who + ".deviantart.com/");

            if (page == null) return null;

            var data    = new Dictionary<string, string>();
            var lines   = page.Split('\n');
            var type    = 0;

            try
            {
                foreach (var line in lines)
                {
                    if (line.Length > 0)
                    {
                        var match = _devinfo_regex.Match(line);

                        if (!match.Success) continue;

                        if (match.Groups["username"].Success)
                        {
                            data.Add("Username", match.Groups["username"].Value.Replace("#", ""));
                        }
                        else if (match.Groups["tagline"].Success)
                        {
                            data.Add("Tagline", match.Groups["tagline"].Value);
                        }
                        else if (match.Groups["item"].Length > 0)
                        {
                            if (match.Groups["item"].Captures.Count > 1 && match.Groups["item"].Captures[0].Value.Length > 5 && match.Groups["item"].Captures[0].Value.Length <= 32)
                                data.Add(match.Groups["item"].Captures[0].Value, match.Groups["item"].Captures[1].Value);
                            else
                            {
                                var t = (type == 0 ? "Type" : type == 1 ? "Name" : "ASL");

                                if (!data.ContainsKey(t))
                                    data.Add(t, match.Groups["item"].Value);
                            }
                        }
                        else if (match.Groups["number"].Success)
                        {
                            var nt = StripTags(match.Groups["type"].Captures[0].Value.Trim());
                            if (nt.Length < 1) continue;
                            if (nt.Contains("   ")) nt = nt.Substring(0, nt.IndexOf("   "));
                            if (nt.Length > 5 && nt.Length <= 32 && !data.ContainsKey(nt))
                                data.Add(nt, match.Groups["number"].Value);
                        }
                        else if (match.Groups["years"].Success)
                        {
                            data.Add("Joined", "Deviant for " + _multispace_regex.Replace(match.Groups["years"].Value, " "));
                            data.Add("Member", match.Groups["member"].Value);
                        }
                    }
                }
            }
            catch { return null; }

            return data;
        }

        private static bool ValidateRemoteCertificate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            return true;
        }

        public static String RegexReplace (String haystack, String what, String with)
        {
            return Regex.Replace(haystack, what, with);
        }
    }
}
