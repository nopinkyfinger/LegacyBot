using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_update (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String utd = Tools.UpToDate(Program.Version);

            if (utd == "OK")
                bot.Say(ns, "<b>&raquo; The bot is already up to date!</b>");
            else if (utd == "ERR")
                bot.Say(ns, "<b>&raquo; Unable to check at this time. You can also <a href=\"http://j.mp/15ikMg1\">check manually.</a></b>");
            else
                bot.Say(ns, "<b>&raquo; There's a new version available! <a href=\"http://j.mp/15ikMg1\">" + utd + "</a></b>");
        }
    }
}

