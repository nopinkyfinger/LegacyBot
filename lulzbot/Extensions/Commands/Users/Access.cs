using System;
using System.Collections.Generic;

namespace lulzbot.Extensions
{
    public partial class Users
    {
        public static void cmd_access (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String helpmsg = String.Format("<b>&raquo; Usage:</b><br/>{0}access username add/del command<br/>{0}access username ban/unban command<br/>{0}access username list", " &middot; " + bot.Config.Trigger);

            if (args.Length >= 3)
            {
                String arg = args[2];
                String who = args[1].ToLower();

                if (arg == "list")
                {
                    if (!userdata.ContainsKey(who))
                    {
                        bot.Say(ns, "<b>&raquo; No such user exists.</b> Users must be banned or given access to at least one command, or their privlevel changed from guest to be displayed.");
                        return;
                    }

                    userdata[who].Access.Sort();
                    userdata[who].Banned.Sort();

                    String Output = String.Empty;

                    Output += String.Format("<b>Command access for :dev{0}::</b><br/><br/>", userdata[who].Name);
                    Output += String.Format("<b>&raquo; <abbr title=\"Can be used regardless of priv level.\">Allowed</abbr>:</b> {0}<br/>", (userdata[who].Access.Count > 0 ? String.Join(", ", userdata[who].Access) : "None"));
                    Output += String.Format("<b>&raquo; Banned:</b> {0}<br/>", (userdata[who].Banned.Count > 0 ? String.Join(", ", userdata[who].Banned) : "None"));

                    bot.Say(ns, Output);
                }
                else if (args.Length >= 4)
                {
                    String cmd = args[3].ToLower();

                    if (arg == "del")
                    {
                        if (userdata.ContainsKey(who))
                        {
                            String realname = userdata[who].Name;

                            if (userdata[who].Access.Contains(cmd))
                            {
                                userdata[who].Access.Remove(cmd);
                                Storage.Save("users", userdata);
                                bot.Say(ns, String.Format("<b>&raquo; Removed access for :dev{0}: to command:</b> {1}", realname, cmd));
                            }
                            else
                            {
                                bot.Say(ns, String.Format("<b>&raquo; User :dev{0}: does not have access to command:</b> {1}", realname, cmd));
                            }
                        }
                        else
                        {
                            bot.Say(ns, String.Format("<b>&raquo; No such user:</b> {0}", who));
                        }
                    }
                    else if (arg == "add")
                    {
                        if (!Events.ValidateCommandName(cmd))
                        {
                            bot.Say(ns, "<b>&raquo; The command must be a valid and existing command!</b>");
                            return;
                        }

                        if (Events.GetCommandAccess(cmd) >= (int)Privs.Owner)
                        {
                            bot.Say(ns, "<b>&raquo; You cannot grant access to commands that require owner access.</b>");
                            return;
                        }

                        if (!userdata.ContainsKey(who))
                            userdata.Add(who, new UserData()
                            {
                                Name = args[1],
                                PrivLevel = (int)Privs.Guest,
                                Access = new List<String>(),
                                Banned = new List<String>()
                            });

                        String realname = userdata[who].Name;

                        if (userdata[who].Access.Contains(cmd))
                        {
                            bot.Say(ns, String.Format("<b>&raquo; User :dev{0}: already has access to command:</b> {1}", realname, cmd));
                        }
                        else
                        {
                            if (userdata[who].Banned.Contains(cmd))
                                userdata[who].Banned.Remove(cmd);
                            userdata[who].Access.Add(cmd);
                            Storage.Save("users", userdata);
                            bot.Say(ns, String.Format("<b>&raquo; Added access for :dev{0}: to command:</b> {1}", realname, cmd));
                        }
                    }
                    else if (arg == "ban")
                    {
                        if (who == bot.Config.Owner.ToLower())
                        {
                            bot.Say(ns, "<b>&raquo; You cannot ban the bot's owner!</b>");
                            return;
                        }

                        if (!userdata.ContainsKey(who))
                            userdata.Add(who, new UserData()
                            {
                                Name = args[1],
                                PrivLevel = (int)Privs.Guest,
                                Access = new List<String>(),
                                Banned = new List<String>()
                            });

                        String realname = userdata[who].Name;

                        if (userdata[who].Banned.Contains(cmd))
                        {
                            bot.Say(ns, String.Format("<b>&raquo; User :dev{0}: already is already banned from using command:</b> {1}", realname, cmd));
                        }
                        else
                        {
                            if (userdata[who].Access.Contains(cmd))
                                userdata[who].Access.Remove(cmd);
                            userdata[who].Banned.Add(cmd);
                            Storage.Save("users", userdata);
                            bot.Say(ns, String.Format("<b>&raquo; User :dev{0}: has been banned from using command:</b> {1}", realname, cmd));
                        }
                    }
                    else if (arg == "unban")
                    {
                        if (userdata.ContainsKey(who))
                        {
                            String realname = userdata[who].Name;

                            if (userdata[who].Banned.Contains(cmd))
                            {
                                userdata[who].Banned.Remove(cmd);
                                Storage.Save("users", userdata);
                                bot.Say(ns, String.Format("<b>&raquo; User :dev{0}: is no longer banned from using command:</b> {1}", realname, cmd));
                            }
                            else
                            {
                                bot.Say(ns, String.Format("<b>&raquo; User :dev{0}: is not banned from using command:</b> {1}", realname, cmd));
                            }
                        }
                        else
                        {
                            bot.Say(ns, String.Format("<b>&raquo; No such user:</b> {0}", who));
                        }
                    }
                    else
                    {
                        bot.Say(ns, helpmsg);
                    }
                }
                else
                {
                    bot.Say(ns, helpmsg);
                }
            }
            else
            {
                bot.Say(ns, helpmsg);
            }
        }
    }
}

