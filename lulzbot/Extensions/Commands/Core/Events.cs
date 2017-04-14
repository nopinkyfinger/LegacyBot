using lulzbot.Types;
using System;
using System.Collections.Generic;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_event (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (args.Length == 1 || (args[1] != "hitcount" && args[1] != "list" && (args[1] != "info" || args.Length != 3)))
            {
                bot.Say(ns, String.Format("<b>&raquo; Usage</b><br/>&raquo; {0}event hitcount<br/>&raquo; {0}event list<br/>&raquo; {0}event info [event]", bot.Config.Trigger));
            }
            else
            {
                if (args[1] == "hitcount")
                {
                    Dictionary<String, UInt32> hitcounts = Events.HitCounts;
                    List<String> keys = new List<String>(hitcounts.Keys);
                    uint total = 0;

                    String output = String.Empty;

                    keys.Sort();

                    foreach (String key in keys)
                    {
                        uint value = hitcounts[key];
                        if (value <= 0)
                            continue;
                        output += String.Format("\n&raquo; {0}: {1} hit{2}", key.PadRight(15, ' '), value, (value == 1 ? "" : "s"));
                        total += value;
                    }

                    bot.Say(ns, String.Format("<b>&raquo; {0} event hits:</b><bcode>{1}</bcode><i>&raquo; Events with 0 hits are not displayed.</i>", total, output));
                }
                else if (args[1] == "list")
                {
                    List<String> keys = new List<String>(Events.GetEvents().Keys);

                    String output = String.Empty;

                    keys.Sort();

                    foreach (String key in keys)
                    {
                        output += String.Format("<br/>&raquo; <b>{0}</b>", key);
                    }

                    bot.Say(ns, String.Format("<b>&raquo; {0} events:</b>{1}", keys.Count, output));
                }
                else if (args[1] == "info")
                {
                    Dictionary<String, List<Types.Event>> events = Events.GetEvents();
                    Dictionary<String, List<Types.Event>> external_events = Events.GetExternalEvents();

                    if (!events.ContainsKey(args[2]))
                    {
                        bot.Say(ns, "<b>&raquo; That event is not valid.</b> Events are case sensitive.");
                        return;
                    }

                    String output = String.Empty;
                    int bound_count = 0;

                    foreach (Event evt in events[args[2]])
                    {
                        bound_count++;
                        output += String.Format("\nCallback {0}\n\tClass: {1}\n\tMethod: {2}\n\tDescription: {3}\n", bound_count, evt.ClassName, evt.Method.Name, evt.Description);
                    }

                    if (external_events.ContainsKey(args[2]))
                    {
                        foreach (Event evt in external_events[args[2]])
                        {
                            bound_count++;
                            output += String.Format("\nCallback {0}\n\tExtension: {1}\n\tMethod: {2}\n\tDescription: {3}\n", bound_count, evt.ClassName, evt.Method.Name, evt.Description);
                        }
                    }

                    bot.Say(ns, String.Format("<b>&raquo; {0} callbacks bound to event '{1}':</b><bcode>{2}</bcode>", bound_count, args[2], output));
                }
            }
        }
    }
}

