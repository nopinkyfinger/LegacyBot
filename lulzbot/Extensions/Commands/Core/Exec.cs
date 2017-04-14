using System;
using System.Diagnostics;
using System.IO;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_exec (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (args.Length < 2)
            {
                bot.Say(ns, String.Format("<b>&raquo; Usage:</b> {0}exec system call<br/><br/><i>* <b>NOTE:</b> System calls can be dangerous! Only use this command if you know what you're doing!</i>", bot.Config.Trigger));
            }
            else
            {
                try
                {
                    String syscall = msg.Substring(5).Replace("&amp;", "&");
                    StreamReader STDOUT, STDERR;

                    Process p;

                    if (Program.OS.StartsWith("Windows"))
                    {
                        p = new Process()
                        {
                            StartInfo = new ProcessStartInfo("cmd", "/C " + syscall)
                            {
                                CreateNoWindow = true,
                                UseShellExecute = false,
                                RedirectStandardError = true,
                                RedirectStandardOutput = true
                            }
                        };
                    }
                    else
                    {
                        p = new Process()
                        {
                            StartInfo = new ProcessStartInfo("sh", "-c \"" + syscall + "\"")
                            {
                                CreateNoWindow = true,
                                UseShellExecute = false,
                                RedirectStandardError = true,
                                RedirectStandardOutput = true
                            }
                        };
                    }

                    if (p.Start())
                    {
                        p.WaitForExit();

                        STDERR = p.StandardError;
                        STDOUT = p.StandardOutput;

                        String output = "", OUT, ERR;

                        if ((OUT = STDOUT.ReadToEnd()).Length > 0)
                            output += "<b>&raquo; Output:</b><br/><bcode>" + OUT + "</bcode>";

                        if ((ERR = STDERR.ReadToEnd()).Length > 0)
                             output += "<b>&raquo; Errors:</b><br/><bcode>" + ERR + "</bcode>";

                        output += String.Format("<br/><b>Proc time:</b> {0}<br/><b>Exec time:</b> {1}", p.TotalProcessorTime, (p.ExitTime - p.StartTime));

                        bot.Say(ns, output);
                    }
                    else
                    {
                        bot.Say(ns, "<b>&raquo; Unable to start process.</b>");
                    }

                }
                catch (Exception E)
                {
                    bot.Say(ns, "<b>&raquo; Unable to perform system call:</b> " + E.Message);
                }
            }
        }
    }
}

