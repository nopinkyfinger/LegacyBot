using System;
using System.Linq;
using System.Collections.Generic;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_commands (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (args.Length == 2 && args[1] == "all")
            {
                List<String> commands = Events.GetAvailableCommands(null);

                if (commands.Count > 0)
                {
                    var output = "<b>&raquo; " + commands.Count + " command" + (commands.Count == 1 ? "" : "s") + "</b><br/>";

                    var mods = new Dictionary<int, List<string>>();

                    foreach (var cmd in commands)
                    {
                        var info = Events.CommandInfo(cmd);

                        if (info != null)
                        {
                            var ps = info.MinimumPrivs;
                            if (!mods.ContainsKey(ps))
                                mods.Add(ps, new List<string>());
                            mods[ps].Add(cmd);
                        }
                    }

                    var query = from pair in mods orderby pair.Key descending select pair;

                    foreach (var pair in query)
                    {
                        if (pair.Value != null)
                        {
                            pair.Value.Sort();
                            output += "<br/> <b>&middot; " + ((Privs)pair.Key).ToString() + ":</b> <b>[</b>" + String.Join("<b>] &middot; [</b>", pair.Value) + "<b>]</b>";
                        }
                    }

                    bot.Say(ns, output);
                }
            }
            else if (args.Length == 2 && args[1] == "mods")
            {
                List<String> commands = Events.GetAvailableCommands(from);

                if (commands.Count > 0)
                {
                    var output = "<b>&raquo; " + commands.Count + " command" + (commands.Count == 1 ? "" : "s") + " available for :dev" + from + ":</b><br/>";

                    var mods = new Dictionary<string, List<string>>();

                    foreach (var cmd in commands)
                    {
                        var info = Events.CommandInfo(cmd);

                        if (info != null)
                        {
                            if (!mods.ContainsKey(info.Extension.Name))
                                mods.Add(info.Extension.Name, new List<string>());
                            mods[info.Extension.Name].Add(cmd);
                        }
                    }

                    var query = from pair in mods orderby pair.Key ascending select pair;

                    foreach (var pair in query)
                    {
                        if (pair.Value != null)
                        {
                            pair.Value.Sort();
                            output += "<br/> <b>&middot; " + pair.Key + ":</b> <b>[</b>" + String.Join("<b>] &middot; [</b>", pair.Value) + "<b>]</b>";
                        }
                    }

                    bot.Say(ns, output);
                }
            }
            else
            {
                List<String> commands = Events.GetAvailableCommands(from);

                bot.Say(ns, String.Format("<b>&raquo; {0} command{1} available for :dev" + from + "::<br/>&raquo;</b> <b>[</b>{2}<b>]</b>", commands.Count, (commands.Count == 1 ? "" : "s"), String.Join("<b>] &middot; [</b>", commands)));
            }
        }
    }
}

