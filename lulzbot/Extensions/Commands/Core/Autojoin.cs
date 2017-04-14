using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_autojoin (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String helpmsg = String.Format("<b>&raquo; Usage:</b><br/>{0}autojoin list<br/>{0}autojoin add/del #channel", " &middot; " + bot.Config.Trigger);
            if (args.Length < 2)
            {
                bot.Say(ns, helpmsg);
            }
            else
            {
                String arg = args[1];

                if (arg == "list")
                {
                    bot.Config.Channels.Sort();
                    bot.Say(ns, String.Format("<b>&raquo; Autojoined channels:</b><br/> &middot; <b>[</b>{0}<b>]</b>", String.Join("<b>]</b>, <b>[</b>", bot.Config.Channels)));
                }
                else if (args.Length == 3)
                {
                    String chan = args[2].ToLower();

                    if (!chan.StartsWith("#"))
                    {
                        bot.Say(ns, helpmsg);
                        return;
                    }

                    if (arg == "add")
                    {
                        if (!bot.Config.Channels.Contains(chan))
                        {
                            Program.AddChannel(chan);
                            bot.Say(ns, "<b>&raquo; Channel added to autojoin:</b> " + args[2]);
                            bot.Join(chan);
                        }
                        else
                        {
                            bot.Say(ns, "<b>&raquo; Channel already exists in autojoin:</b> " + args[2]);
                        }
                    }
                    else if (arg == "del")
                    {
                        if (bot.Config.Channels.Contains(chan))
                        {
                            Program.RemoveChannel(chan);
                            bot.Say(ns, "<b>&raquo; Channel removed from autojoin:</b> " + args[2]);
                        }
                        else
                        {
                            bot.Say(ns, "<b>&raquo; Channel is not in autojoin:</b> " + args[2]);
                        }
                    }
                    else
                    {
                        bot.Say(ns, helpmsg);
                    }
                }
                else
                {
                    bot.Say(ns, helpmsg);
                }
            }
        }
    }
}

