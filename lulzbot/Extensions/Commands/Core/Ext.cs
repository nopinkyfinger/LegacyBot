using System;
using System.Linq;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_ext (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String helpmsg = String.Format("<b>&raquo; Usage:</b>{0}ext list{0}ext disable/enable extension", "<br/> &middot; " + bot.Config.Trigger);

            if (args.Length < 2)
            {
                bot.Say(ns, helpmsg);
            }
            else
            {
                String arg = args[1], cmd;

                if (arg == "list")
                {
                    if (ExtensionContainer.Extensions.Count == 0)
                        bot.Say(ns, "<b>&raquo; There are currently no loaded extentions.</b>");
                    else
                    {
                        var c = ExtensionContainer.Extensions.Count;
                        var list = String.Format("<b>&raquo; There's {0} extension{1} loaded:</b><br/>", c, c == 1 ? "" : "s");

                        var q = from ext in ExtensionContainer.Extensions orderby ext.Name ascending select ext;

                        foreach (var ext in q)
                        {
                            list += String.Format("<br/><b>{0}:</b> Version {1} by :dev{2}:", _disabled_extensions.Contains(ext.Name.ToLower()) ? "<i>" + ext.Name + "</i>" : ext.Name, ext.Version, ext.Author);
                        }

                        list += "<br/><br/><sub><b>Note:</b> <i>Italicized</i> names are disabled extensions.</sub>";

                        bot.Say(ns, list);
                    }
                }
                else if ((arg == "enable" || arg == "disable") && args.Length == 3)
                {
                    cmd = args[2].ToLower();
                    var found = false;

                    foreach (var ext in ExtensionContainer.Extensions)
                    {
                        if (ext.Name.ToLower() == cmd)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        bot.Say(ns, "<b>&raquo; The specified extension does not exist:</b> " + cmd);
                        return;
                    }

                    bool en = arg == "enable";

                    if (!en && _disabled_extensions.Contains(cmd))
                    {
                        bot.Say(ns, "<b>&raquo; The specified extension is already disabled:</b> " + cmd);
                        return;
                    }
                    else if (en && !_disabled_extensions.Contains(cmd))
                    {
                        bot.Say(ns, "<b>&raquo; The specified extension is not disabled:</b> " + cmd);
                        return;
                    }

                    if (en)
                        _disabled_extensions.Remove(cmd);
                    else
                    {
                        _disabled_extensions.Add(cmd);
                        _disabled_extensions.Sort();
                    }

                    bot.Say(ns, String.Format("<b>&raquo; The specified extension has been {0}d:</b> {1}", args[1], cmd));
                    SaveDisabled();
                }
            }
        }
    }
}