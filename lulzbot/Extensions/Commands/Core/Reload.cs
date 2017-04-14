using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_reload (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            bot.Say(ns, "<b>&raquo; Reloading extensions...</b>");
            ulong start = Bot.EpochTimestampMS;

            Events.ClearExternalEvents();
            Bot.Extensions.Load();

            bot.Say(ns, "<b>&raquo; Done!</b> Took " + Tools.FormatTime((ulong)(Bot.EpochTimestampMS - start) / 1000));
        }
    }
}

