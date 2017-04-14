using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_netinfo (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (args.Length >= 2 && args[1] == "reset")
            {
                if (Users.GetPrivs(from.ToLower()) >= (int)Privs.Operators)
                {
                    Program.bytes_sent = 0;
                    Program.bytes_received = 0;
                    Program.packets_in = 0;
                    Program.packets_out = 0;
                    bot.Say(ns, "<b>&raquo; Network usage stats reset.</b>");
                    return;
                }
                else
                {
                    bot.Say(ns, "<b>&raquo; You don't have permission to do that.</b>");
                    return;
                }
            }

            String output = "<bcode>";

            bool verbose = (args.Length >= 2 && args[1] == "verbose");

            output += String.Format("&raquo; Data sent : {0}\n", Tools.FormatBytes(Program.bytes_sent, verbose));
            output += String.Format("&raquo; Data recv : {0}\n", Tools.FormatBytes(Program.bytes_received, verbose));
            output += String.Format("&raquo; Packets   : OUT: {0}\t\tIN: {1}\n", Program.packets_out, Program.packets_in);
            output += String.Format("&raquo; Queues    : OUT: {0}\t\tIN: {1}\n", bot.QueuedOut, bot.QueuedIn);
            output += String.Format("&raquo; Disconnects: {0}\n", Program.Disconnects);
            output += "</bcode>";

            bot.Say(ns, output);
        }
    }
}

