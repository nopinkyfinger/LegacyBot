using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_command (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String helpmsg = String.Format("<b>&raquo; Usage:</b>{0}command list{0}command disable/enable command", "<br/> &middot; " + bot.Config.Trigger);

            if (args.Length < 2)
            {
                bot.Say(ns, helpmsg);
            }
            else
            {
                String arg = args[1], cmd;

                if (arg == "list")
                {
                    if (_disabled_commands.Count == 0)
                        bot.Say(ns, "<b>&raquo; There are currently no disabled commands.</b>");
                    else
                        bot.Say(ns, String.Format("<b>&raquo; There's {0} disabled command{1}:</b><br/> &middot; <b>[</b>{2}<b>]</b>", _disabled_commands.Count, _disabled_commands.Count == 1 ? "" : "s", String.Join("<b>]</b>, <b>[</b>", _disabled_commands)));
                }
                else if ((arg == "enable" || arg == "disable") && args.Length == 3)
                {
                    cmd = args[2].ToLower();

                    if (!Events.ValidateCommandName(cmd))
                    {
                        bot.Say(ns, "<b>&raquo; The specified command does not exist:</b> " + cmd);
                        return;
                    }

                    bool en = arg == "enable";

                    if (!en && _disabled_commands.Contains(cmd))
                    {
                        bot.Say(ns, "<b>&raquo; The specified command is already disabled:</b> " + cmd);
                        return;
                    }
                    else if (en && !_disabled_commands.Contains(cmd))
                    {
                        bot.Say(ns, "<b>&raquo; The specified command is not disabled:</b> " + cmd);
                        return;
                    }

                    if (en)
                        _disabled_commands.Remove(cmd);
                    else
                    {
                        _disabled_commands.Add(cmd);
                        _disabled_commands.Sort();
                    }

                    bot.Say(ns, String.Format("<b>&raquo; The specified command has been {0}d:</b> {1}", args[1], cmd));
                    SaveDisabled();
                }
            }
        }
    }
}