using lulzbot.Types;
using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void evt_property (Bot bot, dAmnPacket packet)
        {
            // Only output this in debug mode.
            if (Program.Debug && !Program.NoDisplay.Contains(Tools.FormatNamespace(packet.Parameter.ToLower(), Types.NamespaceFormat.Channel)))
                ConIO.Write(String.Format("*** Got {0}", packet.Arguments["p"]), Tools.FormatChat(packet.Parameter));

            // Store data
            String ns = packet.Parameter;
            String type = packet.Arguments["p"];

            if (ns.StartsWith("pchat:")) return;

            lock (ChannelData)
            {
                if (!ChannelData.ContainsKey(ns.ToLower()))
                {
                    ChannelData.Add(ns.ToLower(), new Types.ChatData());
                    ChannelData[ns.ToLower()].Name = ns;
                }
            }

            lock (ChannelData[ns.ToLower()])
            {
                if (type == "topic")
                {
                    ChannelData[ns.ToLower()].Topic = packet.Body;
                    Bot.Logger.LogProperty(Tools.FormatChat(packet.Parameter), "topic", packet.Body);
                }
                else if (type == "title")
                {
                    ChannelData[ns.ToLower()].Title = packet.Body;
                    Bot.Logger.LogProperty(Tools.FormatChat(packet.Parameter), "title", packet.Body);
                }
                else if (type == "privclasses")
                {
                    // Ensure we don't run into duplicates.
                    ChannelData[ns.ToLower()].Privclasses.Clear();

                    foreach (String pc in packet.Body.Split('\n'))
                    {
                        if (pc.Length < 3 || !pc.Contains(":"))
                            continue;

                        Types.Privclass privclass = new Types.Privclass();

                        privclass.Order = Convert.ToByte(pc.Split(':')[0]);
                        privclass.Name = pc.Split(':')[1];

                        ChannelData[ns.ToLower()].Privclasses.Add(privclass.Name.ToLower(), privclass);
                    }
                }
                else if (type == "members")
                {
                    // Ensure we don't run into duplicates.
                    ChannelData[ns.ToLower()].Members.Clear();

                    String[] data = packet.Body.Split('\n');
                    var who = "";

                    for (int x = 0; x < data.Length; x++)
                    {
                        if (data[x].Length < 3 || !data[x].StartsWith("member") || x + 6 >= data.Length)
                            continue;

                        Types.ChatMember member = new Types.ChatMember();

                        member.Name = data[x].Substring(7);
                        who = member.Name.ToLower();

                        // We get duplicates on multiple connections.
                        if (ChannelData[ns.ToLower()].Members.ContainsKey(who))
                        {
                            ChannelData[ns.ToLower()].Members[who].ConnectionCount++;
                            continue;
                        }

                        member.Privclass = data[++x].Substring(3);

                        // We don't store the user icon. It's useless to us. Increment x anyway.
                        ++x;

                        member.Symbol = data[++x].Substring(7);
                        member.RealName = data[++x].Substring(9);
                        //member.TypeName = data[++x].Substring(9);
                        member.GPC = data[++x].Substring(4);
                        member.ConnectionCount = 1;

                        ChannelData[ns.ToLower()].Members.Add(who, member);

                        lock (BDS._seen_database)
                        {
                            if (BDS._seen_database.ContainsKey(who))
                            {
                                BDS._seen_database[who].Channel = ns;
                                BDS._seen_database[who].Type = (byte)Types.SeenType.None;
                                BDS._seen_database[who].Timestamp = Bot.EpochTimestamp;
                            }
                            else
                            {
                                BDS._seen_database.Add(who, new SeenInfo()
                                    {
                                        Name = member.Name,
                                        Channel = ns,
                                        Type = (byte)Types.SeenType.None,
                                        Timestamp = Bot.EpochTimestamp
                                    });
                            }
                        }

                        // Increment x for the blank line.
                        x++;
                    }

                    if (ns == "chat:DataShare")
                    {
                        foreach (var m in Core.ChannelData["chat:datashare"].Members.Keys)
                        {
                            BDS.ToggleOnline(m);
                        }
                    }
                }
                else if (type == "info")
                {
                    WhoisData wd = new WhoisData();

                    String[] data = packet.Body.Split(new char[] { '\n' });

                    // Don't parse what we don't need!
                    // Icon is 0
                    wd.Name = packet.Parameter.Substring(6);
                    //wd.Symbol   = data[1].Substring(7);
                    wd.RealName = data[2].Substring(9);
                    //wd.TypeName = data[3].Substring(9);
                    wd.GPC = data[3].Substring(4);

                    int conID = 0;
                    wd.Connections.Add(new WhoisConnection());

                    for (int i = 6; i < data.Length; i++)
                    {
                        if (data[i] == "conn")
                        {
                            conID++;
                            wd.Connections.Add(new WhoisConnection() { ConnectionID = conID });
                        }
                        else if (data[i].StartsWith("online="))
                            ulong.TryParse(data[i].Substring(7), out wd.Connections[conID].Online);
                        else if (data[i].StartsWith("idle="))
                            ulong.TryParse(data[i].Substring(5), out wd.Connections[conID].Idle);
                        else if (data[i].StartsWith("ns ") && data[i] != "ns chat:DataShare")
                            wd.Connections[conID].Channels.Add("#" + data[i].Substring(8));
                    }

                    Events.CallSpecialEvent("whois", new object[] { wd });

                    lock (CommandChannels["whois"])
                    {
                        if (CommandChannels["whois"].Count > 0)
                        {
                            String chan = CommandChannels["whois"][0];
                            CommandChannels["whois"].RemoveAt(0);

                            String output = String.Format("<b>&raquo;</b> :icon{0}: :dev{0}:<br/><br/>", wd.Name);

                            output += String.Format("<i>{0}</i><br/>{1}", wd.RealName, wd.GPC == "guest" ? "" : "<b>dAmn " + wd.GPC + "</b><br/>");

                            foreach (WhoisConnection wc in wd.Connections)
                            {
                                wc.Channels.Sort();
                                output += String.Format("<br/><b>&raquo; Connection #{0}</b><br/> <b>&middot; Online:</b> {1}<br/> <b>&middot; Idle:</b> {2}<br/> <b>&middot; Channels:</b> <b>[</b>{3}<b>]</b><br/>",
                                    wc.ConnectionID + 1, Tools.FormatTime(wc.Online), Tools.FormatTime(wc.Idle), String.Join("<b>]</b>, <b>[</b>", wc.Channels));
                            }

                            bot.Say(chan, output);
                        }
                    }
                }
            }
        }
    }
}

