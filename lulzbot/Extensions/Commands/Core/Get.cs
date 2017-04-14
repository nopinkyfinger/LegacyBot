using System;
using System.Collections.Generic;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_get (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String helpmsg = String.Format("<b>&raquo; Usage:</b> {0}get <i>[#channel]</i> [title|topic|members|privclasses]", bot.Config.Trigger);

            if (args.Length < 2)
            {
                bot.Say(ns, helpmsg);
            }
            else
            {
                // We need it in chat:xxxx format
                String chan, prop;

                if (!args[1].StartsWith("#"))
                {
                    chan = ns.ToLower();
                    prop = args[1];
                }
                else if (args.Length >= 3)
                {
                    chan = Tools.FormatChat(args[1]).ToLower();
                    prop = args[2];
                }
                else
                {
                    bot.Say(ns, helpmsg);
                    return;
                }

                if (prop != "title" && prop != "topic" && prop != "members" && prop != "privclasses")
                {
                    bot.Say(ns, "<b>&raquo; Invalid property!</b> Valid properties are title, topic, members, and privclasses.");
                    return;
                }

                if (!ChannelData.ContainsKey(chan))
                {
                    bot.Say(ns, "<b>&raquo; No data for that channel!</b>");
                    return;
                }

                lock (ChannelData[chan])
                {
                    Types.ChatData data = ChannelData[chan];

                    // Correct capitalization and #
                    String friendly_name = Tools.FormatChat(data.Name);

                    if (prop == "title")
                    {
                        if (data.Title.Length < 1)
                            bot.Say(ns, String.Format("<b>&raquo; Title for {0} is empty.</b>", friendly_name));
                        else
                            bot.Say(ns, String.Format("<b>&raquo; Title for {0}:</b><br/>{1}", friendly_name, data.Title));
                    }
                    else if (prop == "topic")
                    {
                        if (data.Topic.Length < 1)
                            bot.Say(ns, String.Format("<b>&raquo; Topic for {0} is empty.</b>", friendly_name));
                        else
                            bot.Say(ns, String.Format("<b>&raquo; Topic for {0}:</b><br/>{1}", friendly_name, data.Topic));
                    }
                    else if (prop == "members")
                    {
                        if (data.Members.Count < 1)
                            bot.Say(ns, String.Format("<b>&raquo; No members for {0}.</b>", friendly_name));
                        else
                        {
                            String members = String.Empty;

                            Dictionary<String, List<String>> ordered_list = new Dictionary<String, List<String>>();

                            foreach (Types.ChatMember member in data.Members.Values)
                            {
                                if (!ordered_list.ContainsKey(member.Privclass))
                                    ordered_list.Add(member.Privclass, new List<String>());

                                // We split the names to stop it from tabbing people
                                ordered_list[member.Privclass].Add(member.Name.Substring(0, 1) + "<i></i>" + member.Name.Substring(1) + (member.ConnectionCount > 1 ? String.Format("[{0}]", member.ConnectionCount) : ""));
                            }

                            foreach (Types.Privclass privclass in data.Privclasses.Values)
                            {
                                if (!ordered_list.ContainsKey(privclass.Name))
                                {
                                    members += String.Format("<br/><b>{0}</b>: None.", privclass.Name);
                                    continue;
                                }

                                ordered_list[privclass.Name].Sort();

                                members += String.Format("<br/><b>{0}</b>: <b>[</b>{1}<b>]</b>", privclass.Name, String.Join("<b>], [</b>", ordered_list[privclass.Name]));
                            }

                            bot.Say(ns, String.Format("<b>&raquo; {0} member(s) in {1}:</b>{2}", data.Members.Count, friendly_name, members));
                        }
                    }
                    else if (prop == "privclasses")
                    {
                        if (data.Privclasses.Count < 1)
                            bot.Say(ns, String.Format("<b>&raquo; No privclasses for {0}.</b>", friendly_name));
                        else
                        {
                            String privclasses = String.Empty;

                            foreach (Types.Privclass pc in data.Privclasses.Values)
                            {
                                privclasses += String.Format("<br/>&raquo; {0}: {1}", pc.Order, pc.Name);
                            }

                            bot.Say(ns, String.Format("<b>&raquo; Privclasses in {0}:</b>{1}", friendly_name, privclasses));
                        }
                    }
                }
            }
        }
    }
}

