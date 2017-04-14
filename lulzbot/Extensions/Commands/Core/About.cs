using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_about (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String output = String.Empty;

            output += String.Format("<b>&raquo; I am a <a href=\"http://fav.me/d5uviwb\">{0}</a> v{1} <b><sup>{2}</sup></b>, written by :devDivinityArcane:<br/>&raquo;</b> I am owned by :dev{3}:<br/>", Program.BotName, Program.Version, Program.ReleaseName, bot.Config.Owner);
            output += String.Format("<b>&raquo;</b> I've disconnected {0} time{1}, while I've been running for {2}<br/>", Program.Disconnects, Program.Disconnects == 1 ? "" : "s", Tools.FormatTime(bot.uptime));

            bot.Act(ns, output);
        }
    }
}

