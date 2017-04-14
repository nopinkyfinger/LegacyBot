using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_credits (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String output = String.Empty;

            output += "&raquo; lulzBot is a bot written by :devDivinityArcane: <sup><i>Formerly Kyogo</i></sup> and it couldn't have been done without the following people:";
            output += "<br/><br/><b>Absolutely fucking no one!</b> <sub><sub>No, not really.</sub></sub><br/><br/>";
            output += ":devOrrinFox:, who thought of the name and inspired me to write the bot.<br/>";
            output += ":devDeathShadow--666:, for letting me look at n00ds of his sister when I did well.<br/>";
            output += ":devSubjectX52873M:, for teaching me quite a few fun things that bots could do, as well as proving that PHP sucks for bots. :cough: <sub><sub>Dante</sub></sub><br/>";
            output += ":devdoofsmack: and :devtwexler:, who wrote dAmnBot, which inspired me to write bots for dAmn.<br/>";
            output += ":devNoodleMan:, for writing NoodleBot and Gyn, which inspired me to do better, way back when.<br/>";
            output += ":develectricnet:, for writing Futurism, which gave me quite a few ideas on what to do for user<->bot interaction.<br/>";
            output += ":devphotofroggy:, for writing dAmnPHP and Contra, which gave me ideas on how to do certain things I was conflicted about.<br/>";
            output += "And the whole #Botdom team, who put up with my bullshit for many, many years!<br/>";
            output += "<br/>But, most of all, <b>you</b>, for continuing to use lulzBot and letting me know how I can improve it.";

            bot.Act(ns, output);
        }
    }
}

