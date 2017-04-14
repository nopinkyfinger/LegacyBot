using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_ctrig (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (args.Length < 2)
            {
                bot.Say(ns, String.Format("<b>&raquo; Usage:</b> {0}ctrig new_trigger", bot.Config.Trigger));
            }
            else
            {
                String trig = msg.Substring(6);
                if (trig == bot.Config.Trigger)
                {
                    bot.Say(ns, "<b>&raquo; Cannot set trigger to current trigger.</b>");
                    return;
                }
                else if (trig.Length < 2)
                {
                    bot.Say(ns, "<b>&raquo; Cannot set the trigger to something less than two characters in length.</b>");
                    return;
                }

                bot.Say(ns, String.Format("<b>&raquo; Trigger changed:</b> from <b><code>{0}</code></b> to <b><code>{1}</code></b>", bot.Config.Trigger, trig));
                Program.Change_Trigger(trig);
            }
        }
    }
}

