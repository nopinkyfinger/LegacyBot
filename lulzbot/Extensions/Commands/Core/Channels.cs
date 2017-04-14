using System;
using System.Collections.Generic;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_channels (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String output = String.Empty;

            List<String> chans = new List<String>();

            try
            {
                foreach (var cd in Core.ChannelData.Values)
                    if (cd.Name != "chat:DataShare" && !cd.Name.StartsWith("pchat") && !cd.Name.StartsWith("login"))
                        chans.Add(Tools.FormatNamespace(cd.Name, Types.NamespaceFormat.Channel));

                chans.Sort();

                output += String.Format("<b>&raquo; I am currently residing in {0} channel{1}:</b><br/>", chans.Count, chans.Count == 1 ? "" : "s");

                output += String.Format("<b> &middot; [</b>{0}<b>]</b>", String.Join("<b>]</b>, <b>[</b>", chans));

                bot.Act(ns, output);
            }
            catch (Exception Ex)
            {
                if (Program.Debug)
                    bot.Say(ns, "Error: " + Ex.Message);
            }
        }
    }
}

