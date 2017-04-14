using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_override (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String helpmsg = String.Format("<b>&raquo; Usage:</b>{0}override list{0}override command_name priv_level/reset", "<br/> &middot; " + bot.Config.Trigger);

            if (args.Length < 2)
            {
                bot.Say(ns, helpmsg);
            }
            else
            {
                String arg = args[1], cmd;

                if (arg == "list")
                {
                    if (_command_overrides.Count == 0)
                        bot.Say(ns, "<b>&raquo; There are currently no overridden commands.</b>");
                    else
                    {
                        string output = String.Format("<b>&raquo; There's {0} overridden command{1}:</b><br/>", _command_overrides.Count, _command_overrides.Count == 1 ? "" : "s");
                        foreach (var c in _command_overrides.Keys)
                        {
                            output += String.Format("<br/> &middot; Command <b>{0}</b> overridden from <b>{1}</b> to <b>{2}</b>.", c, ((Privs)Events.GetCommandAccess(c, true)).ToString(), _command_overrides[c].ToString());
                        }
                        bot.Say(ns, output);
                    }
                }
                else if (args.Length == 3)
                {
                    cmd = args[1].ToLower();
                    string priv_level = args[2].ToLower();
                    Privs privs;

                    if (!Events.ValidateCommandName(cmd))
                    {
                        bot.Say(ns, "<b>&raquo; The specified command does not exist:</b> " + cmd);
                        return;
                    }

                    if (priv_level == "reset")
                    {
                        if (_command_overrides.ContainsKey(cmd))
                        {
                            _command_overrides.Remove(cmd);
                            bot.Say(ns, String.Format("<b>&raquo; The specified command has been removed from the overrides list:</b> {0}", cmd));
                            SaveOverrides();
                        }
                        else
                        {
                            bot.Say(ns, "<b>&raquo; That command is not overridden!</b>");
                        }
                        return;
                    }

                    if (priv_level != "guests" && priv_level != "members" && priv_level != "operators" && priv_level != "admins" && priv_level != "owner")
                    {
                        bot.Say(ns, "<b>&raquo; Invalid priv level!</b> Appropriate values are: guests, members, operators, admins, owner.");
                        return;
                    }

                    if (priv_level == "owner")
                        privs = Privs.Owner;
                    else if (priv_level == "admins")
                        privs = Privs.Admins;
                    else if (priv_level == "operators")
                        privs = Privs.Operators;
                    else if (priv_level == "members")
                        privs = Privs.Members;
                    else privs = Privs.Guest;

                    if (_command_overrides.ContainsKey(cmd))
                    {
                        _command_overrides[cmd] = privs;
                    }
                    else
                    {
                        _command_overrides.Add(cmd, privs);
                    }

                    bot.Say(ns, String.Format("<b>&raquo;</b> Command <b>{0}</b> overridden from <b>{1}</b> to <b>{2}</b>.", cmd, ((Privs)Events.GetCommandAccess(cmd, true)).ToString(), privs.ToString()));
                    SaveOverrides();
                }
            }
        }
    }
}