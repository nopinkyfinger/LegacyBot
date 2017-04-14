using lulzbot.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Timers;

namespace lulzbot.Extensions
{
    public class BDS
    {
        public static Dictionary<String, Types.BotDef> _botdef_database             = new Dictionary<String, Types.BotDef>();
        public static Dictionary<String, Types.BotInfo> _botinfo_database           = new Dictionary<String, Types.BotInfo>();
        public static Dictionary<String, Types.ClientInfo> _clientinfo_database     = new Dictionary<String, Types.ClientInfo>();
        public static Dictionary<String, Types.SeenInfo> _seen_database             = new Dictionary<String, Types.SeenInfo>();
        private static Dictionary<String, String> _info_requests                    = new Dictionary<String, String>();
        public static List<String> TranslateLangs                                   = new List<String>() { "ar", "bg", "zh-CN", "hr", "cs", "da", "nl", "en", "fi", "fr", "de", "el", "hi", "it", "ja", "ko", "no", "pl", "pt", "ro", "ru", "es", "sv" };
        public static Dictionary<String, String> LanguageAliases                    = new Dictionary<String, String>() { { "arabic", "ar" }, { "bulgarian", "bg" }, { "chinese", "zh-CN" }, { "croatian", "hr" }, { "czech", "cs" }, { "danish", "da" }, { "dutch", "nl" }, { "english", "en" }, { "finnish", "fi" }, { "french", "fr" }, { "german", "de" }, { "greek", "el" }, { "hindi", "hi" }, { "italian", "it" }, { "japanese", "ja" }, { "korean", "ko" }, { "norwegian", "no" }, { "polish", "pl" }, { "portugese", "pt" }, { "romanian", "ro" }, { "russian", "ru" }, { "spanish", "es" }, { "swedish", "sv" } };
        private static List<String> _translate_requests                             = new List<String>();
        private static List<String> _botcheck_privclasses                           = new List<String>() { "Bots", "TestBots", "BrokenBots", "SuspiciousBots", "PoliceBot" };
        private static List<String> _clientcheck_privclasses                        = new List<String>() { "Clients", "BrokenClients", "Members", "Seniors", "CoreTeam" };
        private static Dictionary<string, string> KickTimers                        = new Dictionary<string, string>();
        private static List<string> SeenProviders                                   = new List<string>();
        public  static List<string> GateChecks                                      = new List<string>();
        private static Dictionary<string, KickInfo> Kicks                           = new Dictionary<string, KickInfo>();
        private const int UPDATE_TIME = 604800;
        private static bool Policing = false;
        public const double Version = 0.4;

        public static bool syncing = false, isrequester = false;
        public static string syncwith;
        public static Stopwatch syncwatch;
        private static string syncrns;
        private static int bots_synced = 0;
        private static int clients_synced = 0;

        /// <summary>
        /// Set this to false to overwrite automated saving of the database.
        /// </summary>
        public static bool AutoSave = true;

        public BDS ()
        {
            var info = new ExtensionInfo("BDS", "DivinityArcane", "1.0");

            Events.AddEvent("recv_msg", new Event(this, "ParseBDS", "Parses BDS messages.", ext: info));
            Events.AddEvent("join", new Event(this, "evt_onjoin", "Handles BDS related actions on joining datashare.", ext: info));

            Events.AddCommand("bot", new Command(this, "cmd_bot", "DivinityArcane", 25, "Gets information from the database.", "[trig]bot info username<br/>[trig]bot count<br/>[trig]bot online <i>type</i><br/>[trig]bot owner username <i>online</i><br/>[trig]bot trigger trig", ext: info));
            Events.AddCommand("client", new Command(this, "cmd_client", "DivinityArcane", 25, "Gets information from the database.", "[trig]client info username<br/>[trig]client count<br/>[trig]client online <i>type</i>", ext: info));
            Events.AddCommand("bds", new Command(this, "cmd_bds", "DivinityArcane", 75, "Manage BDS database.", "[trig]bds save<br/>[trig]bds sync username<br/>[trig]bds update", ext: info));
            Events.AddCommand("seen", new Command(this, "cmd_seen", "DivinityArcane", 25, "Retreives information on the last time a username was seen", "[trig]seen username", ext: info));
            Events.AddCommand("translate", new Command(this, "cmd_translate", "DivinityArcane", 25, "Translates text using BDS.", "[trig]translate languages<br/>[trig]translate from_lang to_lang msg", ext: info));
            Events.AddCommand("police", new Command(this, "cmd_police", "DivinityArcane", 99, "Changes policebot status.", "[trig]police status<br/>[trig]police on/off", ext: info));

            if (Program.Debug)
                ConIO.Write("Loading databases...", "BDS");

            // Load saved data, if we can.
            _botdef_database = Storage.Load<Dictionary<String, Types.BotDef>>("bds_botdef_database");
            _botinfo_database = Storage.Load<Dictionary<String, Types.BotInfo>>("bds_botinfo_database");
            _clientinfo_database = Storage.Load<Dictionary<String, Types.ClientInfo>>("bds_clientinfo_database");
            _seen_database = Storage.Load<Dictionary<String, Types.SeenInfo>>("bds_seen_database");

            // Values can be null if the file is empty or doesn't exist.
            if (_botdef_database == null)
                _botdef_database = new Dictionary<string, Types.BotDef>();

            if (_botinfo_database == null)
                _botinfo_database = new Dictionary<string, Types.BotInfo>();

            if (_clientinfo_database == null)
                _clientinfo_database = new Dictionary<string, Types.ClientInfo>();

            if (_seen_database == null)
                _seen_database = new Dictionary<string, Types.SeenInfo>();

            if (Program.Debug)
                ConIO.Write(String.Format("Loaded databases. Got {0} BotDEF entries, {1} BotINFO entries, {2} ClientINFO entries, and {3} SEEN entries.", _botdef_database.Count, _botinfo_database.Count, _clientinfo_database.Count, _seen_database.Count), "BDS");


            foreach (var b in _botinfo_database)
            {
                _botinfo_database[b.Key].Online = false;
            }

            foreach (var c in _clientinfo_database)
            {
                _clientinfo_database[c.Key].Online = false;
            }

            // We will save on a timer. 
            if (AutoSave)
            {
                // Saves once per five minutes.
                Timer save_timer = new Timer(300000);

                save_timer.Elapsed += delegate { if (BDS.AutoSave) BDS.Save(); };

                save_timer.Start();
            }

            Policing = Storage.Load<bool>("pbstatus");

            syncing = false;
        }

        public static void evt_onjoin (Bot bot, dAmnPacket packet)
        {
            if (packet.Parameter.ToLower() == "chat:datashare")
            {
                // IDS-NOTE, XFER, BOTCHECK-SYNC ?
                String[] caps = new String[] { "BOTCHECK", "BOTCHECK-EXT", "SEEN"};
                bot.NPSay(packet.Parameter, "BDS:PROVIDER:CAPS:" + String.Join(",", caps));
            }
        }

        /// <summary>
        /// Saves the databases to disk
        /// </summary>
        private static void Save ()
        {
            if (Program.Debug)
                ConIO.Write("Saving databases.", "BDS");

            lock (_botdef_database)
                Storage.Save("bds_botdef_database", _botdef_database);

            lock (_botinfo_database)
                Storage.Save("bds_botinfo_database", _botinfo_database);

            lock (_clientinfo_database)
                Storage.Save("bds_clientinfo_database", _clientinfo_database);

            lock (_seen_database)
                Storage.Save("bds_seen_database", _seen_database);
        }

        public static void ToggleOnline (string who)
        {
            var key = who.ToLower();

            if (Core.ChannelData.ContainsKey("chat:datashare"))
            {
                lock (_botdef_database)
                {
                    if (_botinfo_database.ContainsKey(key))
                        _botinfo_database[key].Online = Core.ChannelData["chat:datashare"].Members.ContainsKey(key);

                    if (_clientinfo_database.ContainsKey(key))
                        _clientinfo_database[key].Online = Core.ChannelData["chat:datashare"].Members.ContainsKey(key);
                }
            }
        }

        /// <summary>
        /// Checks whether the specified username is a policebot
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <param name="channel">Channel to check. Default: #DataShare</param>
        /// <returns>true if PoliceBot, false otherwise.</returns>
        public static bool IsPoliceBot (String username, String channel = "chat:datashare", bool pboverride = false)
        {
            channel = channel.ToLower();
            username = username.ToLower();

            if (!Core.ChannelData.ContainsKey(channel))
                return false;

            if (!Core.ChannelData[channel].Members.ContainsKey(username))
                return false;

            if (username == Program.Bot.Config.Username.ToLower() && (!Policing && !pboverride))
                return false;

            if (Core.ChannelData[channel].Members[username].Privclass.ToLower() == "policebot")
                return true;
            else
                return false;
        }

        private string SeenMsg (SeenType type)
        {
            switch (type)
            {
                case SeenType.Joining:
                    return "joining";

                case SeenType.Parting:
                    return "leaving";

                case SeenType.Talking:
                    return "talking in";

                case SeenType.Kicked:
                    return "being kicked from";

                case SeenType.None:
                default:
                    return "in";
            }
        }

        /// <summary>
        /// BDS command
        /// </summary>
        public void cmd_seen (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (args.Length == 1)
                bot.Say(ns, "<b>&raquo; Usage:</b> " + bot.Config.Trigger + "seen username");
            else
            {
                var who = args[1].ToLower();

                if (!_seen_database.ContainsKey(who))
                    bot.Say(ns, "<b>&raquo; I haven't seen that user before, sorry.</b>");
                else
                {
                    var info = _seen_database[who];
                    var time = Tools.FormatTime(Bot.EpochTimestamp - info.Timestamp);
                    bot.Say(ns, String.Format("<b>&raquo; :dev{0}:</b> was last seen {1} {2} {3}.", info.Name, SeenMsg((SeenType)info.Type), Tools.FormatNamespace(info.Channel, NamespaceFormat.Channel), time == "0 seconds" ? "just now" : time + " ago"));
                }
            }
        }

        /// <summary>
        /// BDS command
        /// </summary>
        public void cmd_bot (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String helpmsg = String.Format("<b>&raquo; Usage:</b>{0}bot info username{0}bot count{0}bot online <i>[type]</i>{0}bot owner username <i>[online]</i>{0}bot trigger trigger", "<br/>&raquo; " + bot.Config.Trigger);

            // First arg is the command
            if (args.Length == 1)
            {
                bot.Say(ns, helpmsg);
            }
            else
            {
                if (args[1] == "info")
                {
                    if (args.Length >= 3)
                    {
                        if (_botinfo_database.ContainsKey(args[2].ToLower()))
                        {
                            Types.BotInfo info = _botinfo_database[args[2].ToLower()];
                            ulong ts = Bot.EpochTimestamp - info.Modified;
                            if (ts >= UPDATE_TIME) // 7 days
                            {
                                lock (_info_requests)
                                {
                                    _info_requests.Add(args[2].ToLower(), ns);
                                }

                                bot.NPSay("chat:datashare", "BDS:BOTCHECK:REQUEST:" + args[2]);
                                bot.Say(ns, String.Format("{0}: Data for {1} is outdated, one second while I update it...", from, args[2]));
                                return;
                            }
                            String output = String.Format("<b>&raquo; Information on :dev{0}:</b><br/>", info.Name);
                            output += String.Format("<b>Bot type:</b> {0}<br/>", info.Type);
                            output += String.Format("<b>Bot version:</b> {0}<br/>", info.Version);
                            output += String.Format("<b>Bot owner:</b> :dev{0}:<br/>", info.Owner);
                            output += String.Format("<b>Bot trigger:</b> <b><code>{0}</code></b><br/>", info.Trigger.Replace("&", "&amp;"));
                            output += String.Format("<b>BDS version:</b> {0}<br/>", info.BDSVersion);
                            output += String.Format("<b>Last modified:</b> {0} ago", Tools.FormatTime(ts));
                            bot.Say(ns, output);
                        }
                        else if (_clientinfo_database.ContainsKey(args[2].ToLower()))
                        {
                            bot.Say(ns, String.Format("<b>&raquo; {0} is a client. Use {1}client info {0}</b>", args[2], bot.Config.Trigger));
                        }
                        else
                        {
                            lock (_info_requests)
                            {
                                _info_requests.Add(args[2].ToLower(), ns);
                            }

                            bot.NPSay("chat:datashare", "BDS:BOTCHECK:REQUEST:" + args[2]);
                            bot.Say(ns, String.Format("{0}: {1} isn't in my database yet. Requesting information, please stand by...", from, args[2]));
                        }
                    }
                    else
                    {
                        bot.Say(ns, helpmsg);
                    }
                }
                else if (args[1] == "count")
                {
                    if (_botinfo_database.Count == 0)
                    {
                        bot.Say(ns, "<b>&raquo; There are 0 bots in my local database.</b>");
                        return;
                    }

                    Dictionary<String, int> bots = new Dictionary<string, int>();

                    foreach (BotInfo info in _botinfo_database.Values)
                    {
                        if (!bots.ContainsKey(info.Type))
                            bots.Add(info.Type, 0);

                        bots[info.Type]++;
                    }

                    var bots_sorted =    from pair in bots
                                         orderby pair.Value descending
                                         select pair;

                    String output = String.Empty;
                    int count = 0;

                    foreach (KeyValuePair<String, int> pair in bots_sorted)
                    {
                        output += String.Format("{0} ({1})<b>]</b>, <b>[</b>", pair.Key, pair.Value);
                        count += pair.Value;
                    }

                    bot.Say(ns, String.Format("<b>&raquo; There are {0} bot{1} in my local database:</b><br/><b>&raquo; [</b>", count, count == 1 ? "" : "s") + output.Substring(0, output.Length - 10));
                }
                else if (args[1] == "online")
                {
                    String type = "all";
                    if (args.Length >= 3)
                    {
                        type = msg.Substring(11).ToLower();
                    }

                    if (type == "all")
                    {
                        Dictionary<String, int> bots = new Dictionary<string, int>();

                        if (Core.ChannelData.ContainsKey("chat:datashare"))
                        {
                            ChatData cd = Core.ChannelData["chat:datashare"];

                            foreach (ChatMember m in cd.Members.Values)
                            {
                                if (_botcheck_privclasses.Contains(m.Privclass))
                                {
                                    if (_botinfo_database.ContainsKey(m.Name.ToLower()))
                                    {
                                        if (!bots.ContainsKey(_botinfo_database[m.Name.ToLower()].Type))
                                            bots.Add(_botinfo_database[m.Name.ToLower()].Type, 0);

                                        bots[_botinfo_database[m.Name.ToLower()].Type]++;
                                    }
                                }
                            }
                        }

                        if (bots.Count == 0)
                        {
                            bot.Say(ns, "<b>&raquo; 0 known online bots.</b>");
                            return;
                        }

                        var bots_sorted =    from pair in bots
                                             orderby pair.Value descending
                                             select pair;

                        String output = String.Empty;
                        int count = 0;

                        foreach (KeyValuePair<String, int> pair in bots_sorted)
                        {
                            output += String.Format("{0} ({1})<b>]</b>, <b>[</b>", pair.Key, pair.Value);
                            count += pair.Value;
                        }

                        bot.Say(ns, String.Format("<b>&raquo; {0} known online bot{1}:</b><br/><b>&raquo; [</b>", count, count == 1 ? "" : "s") + output.Substring(0, output.Length - 10));
                    }
                    else
                    {
                        List<String> bots = new List<string>();

                        if (Core.ChannelData.ContainsKey("chat:datashare"))
                        {
                            ChatData cd = Core.ChannelData["chat:datashare"];

                            foreach (ChatMember m in cd.Members.Values)
                            {
                                if (_botcheck_privclasses.Contains(m.Privclass))
                                {
                                    if (_botinfo_database.ContainsKey(m.Name.ToLower()) && _botinfo_database[m.Name.ToLower()].Type.ToLower() == type)
                                        bots.Add(m.Name);
                                }
                            }
                        }

                        bots.Sort();

                        if (bots.Count > 0)
                        {
                            bot.Say(ns, String.Format("<b>&raquo; {0} known online bot{1} of type {2}:</b><br/><b>&raquo; [</b>{3}<b>]</b>", bots.Count, bots.Count == 1 ? "" : "s", type, String.Join("<b>]</b>, <b>[</b>", bots)));
                        }
                        else
                            bot.Say(ns, String.Format("<b>&raquo; 0 known online bots of type {0}.</b>", type));
                    }
                }

                else if (args[1] == "owner" && args.Length >= 3)
                {
                    var who = args[2].ToLower();
                    int max = -1;

                    if (Users.GetPrivs(from) < (int)Privs.Members)
                        max = 50;

                    List<string> bots = new List<string>();

                    foreach (BotInfo info in _botinfo_database.Values)
                    {
                        if (info.Owner.ToLower() == who)
                            bots.Add(info.Name);

                        if (max != -1 && bots.Count >= max) break;
                    }

                    if (bots.Count == 0)
                    {
                        bot.Say(ns, "<b>&raquo; It doesn't look like I have any bots owned by that user in my database.</b>");
                        return;
                    }

                    bots.Sort();

                    if (args.Length == 4 && args[3] == "online")
                    {
                        List<string> online = new List<string>();

                        if (Core.ChannelData.ContainsKey("chat:datashare"))
                        {
                            ChatData cd = Core.ChannelData["chat:datashare"];

                            foreach (var b in bots)
                            {
                                if (cd.Members.ContainsKey(b.ToLower()))
                                    online.Add(b);
                            }
                        }

                        if (online.Count == 0)
                        {
                            bot.Say(ns, "<b>&raquo; It doesn't look like I have any bots owned by that user in my database.</b>");
                            return;
                        }

                        bot.Say(ns, String.Format("<b>&raquo; There's {0} bot{1} owned by :dev{2}: in my database that are online:</b><br/> <b>(</b>{3}<b>)</b>",
                            online.Count, online.Count == 1 ? "" : "s", args[2], String.Join("<b>)</b>, <b>(</b>", online)) + (max == -1 ? "" : "<br/><br/><i>* Guests are limited to 50 or less results.</i>"));
                    }
                    else
                    {
                        bot.Say(ns, String.Format("<b>&raquo; There's {0} bot{1} owned by :dev{2}: in my database:</b><br/> <b>(</b>{3}<b>)</b>",
                            bots.Count, bots.Count == 1 ? "" : "s", args[2], String.Join("<b>)</b>, <b>(</b>", bots)) + (max == -1 ? "" : "<br/><br/><i>* Guests are limited to 50 or less results.</i>"));
                    }
                }

                else if (args[1] == "trigger" && args.Length == 3)
                {
                    var trig = args[2].ToLower();
                    int max = -1;

                    if (Users.GetPrivs(from) < (int)Privs.Members)
                        max = 50;

                    List<string> bots = new List<string>();

                    foreach (BotInfo info in _botinfo_database.Values)
                    {
                        if (info.Trigger == trig)
                            bots.Add(info.Name);

                        if (max != -1 && bots.Count >= max) break;
                    }

                    if (bots.Count == 0)
                    {
                        bot.Say(ns, "<b>&raquo; It doesn't look like I have any bots with that trigger in my database.</b>");
                        return;
                    }

                    bots.Sort();

                    bot.Say(ns, String.Format("<b>&raquo; There's {0} bot{1} using trigger <code>{2}</code> in my database:</b><br/> <b>(</b>{3}<b>)</b>",
                        bots.Count, bots.Count == 1 ? "" : "s", trig, String.Join("<b>)</b>, <b>(</b>", bots)) + (max == -1 ? "" : "<br/><br/><i>* Guests are limited to 50 or less results.</i>"));
                }

                else
                {
                    bot.Say(ns, helpmsg);
                }
            }
        }

        /// <summary>
        /// BDS command
        /// </summary>
        public void cmd_client (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String helpmsg = String.Format("<b>&raquo; Usage:</b><br/>&raquo; {0}client info [username]<br/>&raquo; {0}client count<br/>&raquo; {0}client online [type]", bot.Config.Trigger);

            // First arg is the command
            if (args.Length == 1)
            {
                bot.Say(ns, helpmsg);
            }
            else
            {
                if (args[1] == "info")
                {
                    if (args.Length >= 3)
                    {
                        if (_clientinfo_database.ContainsKey(args[2].ToLower()))
                        {
                            Types.ClientInfo info = _clientinfo_database[args[2].ToLower()];
                            ulong ts = Bot.EpochTimestamp - info.Modified;
                            if (ts >= UPDATE_TIME) // 7 days
                            {
                                lock (_info_requests)
                                {
                                    _info_requests.Add(args[2].ToLower(), ns);
                                }

                                bot.NPSay("chat:datashare", "BDS:BOTCHECK:REQUEST:" + args[2]);
                                bot.Say(ns, String.Format("{0}: Data for {1} is outdated, one second while I update it...", from, args[2]));
                                return;
                            }
                            String output = String.Format("<b>&raquo; Information on :dev{0}:</b><br/>", info.Name);
                            output += String.Format("<b>Client type:</b> {0}<br/>", info.Type);
                            output += String.Format("<b>Client version:</b> {0}<br/>", info.Version);
                            output += String.Format("<b>BDS version:</b> {0}<br/>", info.BDSVersion);
                            output += String.Format("<b>Last modified:</b> {0} ago", Tools.FormatTime(ts));
                            bot.Say(ns, output);
                        }
                        else if (_botinfo_database.ContainsKey(args[2].ToLower()))
                        {
                            bot.Say(ns, String.Format("<b>&raquo; {0} is a bot. Use {1}bot info {0}</b>", args[2], bot.Config.Trigger));
                        }
                        else
                        {
                            lock (_info_requests)
                            {
                                _info_requests.Add(args[2].ToLower(), ns);
                            }

                            bot.NPSay("chat:datashare", "BDS:BOTCHECK:REQUEST:" + args[2]);
                            bot.Say(ns, String.Format("{0}: {1} isn't in my database yet. Requesting information, please stand by...", from, args[2]));
                        }
                    }
                    else
                    {
                        bot.Say(ns, helpmsg);
                    }
                }
                else if (args[1] == "count")
                {
                    if (_clientinfo_database.Count == 0)
                    {
                        bot.Say(ns, "<b>&raquo; There are 0 clients in my local database.</b>");
                        return;
                    }

                    Dictionary<String, int> clients = new Dictionary<string, int>();

                    foreach (ClientInfo info in _clientinfo_database.Values)
                    {
                        if (!clients.ContainsKey(info.Type))
                            clients.Add(info.Type, 0);

                        clients[info.Type]++;
                    }

                    var clients_sorted = from pair in clients
                                         orderby pair.Value descending
                                         select pair;

                    String output = String.Empty;
                    int count = 0;

                    foreach (KeyValuePair<String, int> pair in clients_sorted)
                    {
                        output += String.Format("{0} ({1})<b>]</b>, <b>[</b>", pair.Key, pair.Value);
                        count += pair.Value;
                    }

                    bot.Say(ns, String.Format("<b>&raquo; There are {0} client{1} in my local database:</b><br/><b>&raquo; [</b>", count, count == 1 ? "" : "s") + output.Substring(0, output.Length - 10));
                }
                else if (args[1] == "online")
                {
                    String type = "all";
                    if (args.Length >= 3)
                    {
                        type = msg.Substring(14).ToLower();
                    }

                    if (type == "all")
                    {
                        Dictionary<String, int> clients = new Dictionary<string, int>();

                        if (Core.ChannelData.ContainsKey("chat:datashare"))
                        {
                            ChatData cd = Core.ChannelData["chat:datashare"];

                            foreach (ChatMember m in cd.Members.Values)
                            {
                                if (_clientcheck_privclasses.Contains(m.Privclass))
                                {
                                    if (_clientinfo_database.ContainsKey(m.Name.ToLower()))
                                    {
                                        if (!clients.ContainsKey(_clientinfo_database[m.Name.ToLower()].Type))
                                            clients.Add(_clientinfo_database[m.Name.ToLower()].Type, 0);

                                        clients[_clientinfo_database[m.Name.ToLower()].Type]++;
                                    }
                                }
                            }
                        }

                        if (clients.Count == 0)
                        {
                            bot.Say(ns, "<b>&raquo; 0 known online clients.</b>");
                            return;
                        }

                        var clients_sorted = from pair in clients
                                             orderby pair.Value descending
                                             select pair;

                        String output = String.Empty;
                        int count = 0;

                        foreach (KeyValuePair<String, int> pair in clients_sorted)
                        {
                            output += String.Format("{0} ({1})<b>]</b>, <b>[</b>", pair.Key, pair.Value);
                            count += pair.Value;
                        }

                        bot.Say(ns, String.Format("<b>&raquo; {0} known online client{1}:</b><br/><b>&raquo; [</b>", count, count == 1 ? "" : "s") + output.Substring(0, output.Length - 10));
                    }
                    else
                    {
                        List<String> clients = new List<string>();

                        if (Core.ChannelData.ContainsKey("chat:datashare"))
                        {
                            ChatData cd = Core.ChannelData["chat:datashare"];

                            foreach (ChatMember m in cd.Members.Values)
                            {
                                if (_clientcheck_privclasses.Contains(m.Privclass))
                                {
                                    if (_clientinfo_database.ContainsKey(m.Name.ToLower()) && _clientinfo_database[m.Name.ToLower()].Type.ToLower() == type)
                                        clients.Add(m.Name);
                                }
                            }
                        }

                        clients.Sort();

                        if (clients.Count > 0)
                        {
                            bot.Say(ns, String.Format("<b>&raquo; {0} known online client{1} of type {2}:</b><br/><b>&raquo; [</b>{3}<b>]</b>", clients.Count, clients.Count == 1 ? "" : "s", type, String.Join("<b>]</b>, <b>[</b>", clients)));
                        }
                        else
                            bot.Say(ns, String.Format("<b>&raquo; 0 known online clients of type {0}.</b>", type));
                    }
                }
            }
        }

        /// <summary>
        /// BDS command
        /// </summary>
        public void cmd_bds (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            // First arg is the command
            if (args.Length == 1)
            {
                bot.Say(ns, String.Format("<b>&raquo; Usage:</b><br/>{0}bds save<br/>{0}bds update<br/>{0}bds sync username", " &middot; " + bot.Config.Trigger));
            }
            else
            {
                String arg = args[1].ToLower();

                if (arg == "save")
                {
                    Save();
                    bot.Say(ns, "<b>&raquo; Database has been saved to disk.</b>");
                }
                else if (arg == "update")
                {
                    if (!IsPoliceBot(bot.Config.Username))
                    {
                        bot.Say(ns, "<b>&raquo; Only policebots can do that.</b>");
                        return;
                    }

                    List<String> datas = new List<String>();

                    if (Core.ChannelData.ContainsKey("chat:datashare"))
                    {
                        ChatData cd = Core.ChannelData["chat:datashare"];

                        foreach (ChatMember m in cd.Members.Values)
                        {
                            if (_botcheck_privclasses.Contains(m.Privclass))
                            {
                                if (!_botinfo_database.ContainsKey(m.Name.ToLower()) || Bot.EpochTimestamp - _botinfo_database[m.Name.ToLower()].Modified >= UPDATE_TIME)
                                {
                                    datas.Add(m.Name);
                                }
                            }
                            else if (_clientcheck_privclasses.Contains(m.Privclass))
                            {
                                if (!_clientinfo_database.ContainsKey(m.Name.ToLower()) || Bot.EpochTimestamp - _clientinfo_database[m.Name.ToLower()].Modified >= UPDATE_TIME)
                                {
                                    datas.Add(m.Name);
                                }
                            }
                        }
                    }

                    if (datas.Count > 0)
                    {
                        bot.NPSay("chat:DataShare", "BDS:BOTCHECK:REQUEST:" + String.Join(",", datas));
                        bot.Say(ns, String.Format("<b>&raquo; Requested data for {0} bot{1}/client{1}.</b>", datas.Count, datas.Count == 1 ? "" : "s"));
                    }
                    else
                        bot.Say(ns, "<b>&raquo; No data needs to be updated.</b>");
                }

                else if (arg == "sync" && args.Length == 3)
                {
                    syncing = true;
                    isrequester = true;
                    syncrns = ns;
                    bot.Say(ns, "<b>&raquo; Requesting sync with " + args[2] + "</b>");
                    bot.NPSay("chat:DataShare", "BDS:SYNC:REQUEST:" + args[2]);
                }

                else bot.Say(ns, String.Format("<b>&raquo; Usage:</b><br/>{0}bds save<br/>{0}bds update<br/>{0}bds sync username", " &middot; " + bot.Config.Trigger));
            }
        }

        public void cmd_translate (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String helpmsg = String.Format("<b>&raquo; Usage:</b><br/>{0}translate languages<br/>{0}translate <i>from_lang to_lang</i> message", " &middot; " + bot.Config.Trigger);
            if (args.Length == 1)
            {
                bot.Say(ns, helpmsg);
            }
            else
            {
                if (args[1] == "languages")
                {
                    String output = String.Format("<b>&raquo; There are {0} supported language{1}:</b><br/><br/>", TranslateLangs.Count, TranslateLangs.Count == 1 ? "" : "s");

                    foreach (var pair in LanguageAliases)
                    {
                        output += String.Format("<b>[{0}:</b> {1}<b>]</b> &nbsp; ", pair.Key, pair.Value);
                    }

                    output += "<br/><br/><sub><i>* Note that at least one of the languages used in translation must be English.</i></sub>";

                    bot.Say(ns, output);
                }
                else
                {
                    if (args.Length > 3)
                    {
                        String from_lang = args[1].ToLower(), to_lang = args[2].ToLower();

                        if (!TranslateLangs.Contains(from_lang))
                        {
                            if (LanguageAliases.ContainsKey(from_lang))
                                from_lang = LanguageAliases[from_lang];
                            else
                            {
                                bot.Say(ns, "<b>&raquo; Invalid from_lang.</b>");
                                return;
                            }
                        }

                        if (!TranslateLangs.Contains(to_lang))
                        {
                            if (LanguageAliases.ContainsKey(to_lang))
                                to_lang = LanguageAliases[to_lang];
                            else
                            {
                                bot.Say(ns, "<b>&raquo; Invalid to_lang.</b>");
                                return;
                            }
                        }

                        if (from_lang != "en" && to_lang != "en")
                        {
                            bot.Say(ns, "<b>&raquo; At least one of the languages must be English!</b>");
                            return;
                        }

                        String message = Convert.ToBase64String(Encoding.UTF8.GetBytes(WebUtility.HtmlDecode(msg.Substring(11 + args[1].Length + args[2].Length))));

                        lock (_translate_requests)
                        {
                            _translate_requests.Add(packet.Parameter);
                            bot.NPSay("chat:datashare", String.Format("BDS:TRANSLATE:REQUEST:{0},{1},{2},{3}", packet.Parameter, from_lang, to_lang, message));
                        }
                    }
                    else bot.Say(ns, helpmsg);
                }
            }
        }

        public void cmd_police (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String helpmsg = String.Format("<b>&raquo; Usage:</b><br/>{0}police status<br/>{0}police on|off", " &middot; " + bot.Config.Trigger);
            
            if (args.Length == 1)
            {
                bot.Say(ns, helpmsg);
            }
            else
            {
                var cmd = args[1].ToLower();

                if (cmd == "status")
                {
                    bool ipba = IsPoliceBot(bot.Config.Username, "chat:DSGateway"), ipbb = IsPoliceBot(bot.Config.Username);

                    bot.Say(ns, String.Format("<b>&raquo; Policing is {2}</b><br/><b> &middot; Policing for #DSGateway is:</b> {0}<br/><b> &middot; Policing for #DataShare is:</b> {1}", ipba == true ? "enabled" : "disabled", ipbb == true ? "enabled" : "disabled", Policing == true ? "enabled" : "disabled"));
                }
                else if (cmd == "on" || cmd == "off")
                {
                    Policing = cmd == "on";

                    Storage.Save("pbstatus", Policing);

                    bot.Say(ns, "<b>&raquo; Policing has been turned " + cmd + "</b>");
                }
                else
                    bot.Say(ns, helpmsg);
            }
        }

        public static string BDBHash ()
        {
            List<string> uns = new List<string>();
            foreach (var u in _botinfo_database.Keys)
                uns.Add(u.ToLower());
            uns.Sort();
            return Tools.md5(String.Join("", uns).Replace(" ", ""));
        }

        /// <summary>
        /// Parses BDS messages
        /// </summary>
        /// <param name="bot">Bot instance</param>
        /// <param name="packet">Packet object</param>
        public void ParseBDS (Bot bot, dAmnPacket packet)
        {
            if (packet.Parameter == "chat:Botdom" && packet.Body.ToLower().StartsWith("<abbr title=\"" + bot.Config.Username.ToLower() + ": botcheck\"></abbr>"))
            {
                String hash = Tools.md5((bot.Config.Trigger + packet.Arguments["from"] + bot.Config.Username).Replace(" ", "").ToLower());
                bot.Say(packet.Parameter, String.Format("Beep! <abbr title=\"botresponse: {0} {1} {2} {3} {4} {5}\"></abbr>", packet.Arguments["from"], bot.Config.Owner, Program.BotName, Program.Version, hash, bot.Config.Trigger));
                return;
            }

            // Not from DS? Ignore it.
            if (packet.Parameter.ToLower() != "chat:datashare" && packet.Parameter.ToLower() != "chat:dsgateway" && !syncing)
                return;

            // Doesn't contain segments? Ignore it.
            if (!packet.Body.Contains(":"))
                return;

            String msg      = packet.Body;
            String[] bits   = msg.Split(':');
            String ns       = packet.Parameter;
            String from     = packet.Arguments["from"];
            String username = bot.Config.Username;
            String trigger  = bot.Config.Trigger;
            String owner    = bot.Config.Owner;

            bool from_policebot = IsPoliceBot(from, packet.Parameter);

            if (bits[0] == "BDS")
            {
                if (bits.Length >= 3 && bits[1] == "SYNC")
                {
                    if (bits.Length == 4 && bits[2] == "REQUEST" && bits[3].ToLower() == username.ToLower())
                    {
                        if (!syncing && !isrequester && IsPoliceBot(username, "chat:DataShare", true))
                        {
                            syncwith = from.ToLower();
                            bot.NPSay(ns, String.Format("BDS:SYNC:RESPONSE:{0},{1},{2}", from, BDBHash(), _botinfo_database.Count));
                        }
                    }
                    else if (bits[2] == "BEGIN" && !isrequester && ns.StartsWith("pchat:") && syncing && ns.ToLower().Contains(syncwith) && from.ToLower() != username.ToLower())
                    {
                        bots_synced = 0;
                        clients_synced = 0;
                        foreach (var x in _botinfo_database.Values)
                        {
                            bot.NPSay(ns, String.Format("BDS:SYNC:INFO:{0},{1},{2},{3}/{4},{5},{6}", x.Name, x.Owner, x.Type, x.Version, x.BDSVersion, x.Modified, x.Trigger));
                            bots_synced++;

                            if (bots_synced % 100 == 0)
                                System.Threading.Thread.Sleep(250);
                        }
                        foreach (var x in _clientinfo_database.Values)
                        {
                            bot.NPSay(ns, String.Format("BDS:SYNC:CLIENTINFO:{0},{1},{2}/{3},{4}", x.Name, x.Type, x.Version, x.BDSVersion, x.Modified));
                            clients_synced++;

                            if (clients_synced % 100 == 0)
                                System.Threading.Thread.Sleep(250);
                        }
                        bot.NPSay(ns, "BDS:SYNC:FINISHED");
                        bot.NPSay(ns, "BDS:LINK:CLOSED");
                        bot.Part(ns);
                        syncwith = "";
                        bots_synced = 0;
                        clients_synced = 0;
                        syncing = false;
                    }
                    else if (bits.Length == 4 && bits[2] == "RESPONSE")
                    {
                        if (!bits[3].Contains(","))
                            return;

                        String[] data = bits[3].Split(',');

                        if (data.Length != 3)
                            return;

                        if (data[0].ToLower() != username.ToLower())
                            return;

                        if (data[1] != BDBHash())
                        {
                            syncwith = from.ToLower();
                            bot.NPSay(ns, "BDS:LINK:REQUEST:" + from);
                        }
                        else
                        {
                            bot.NPSay(ns, "BDS:SYNC:OKAY:" + from);
                            syncing = false;
                        }
                    }

                    else if (bits[2] == "FINISHED")
                    {
                        if (syncrns != "")
                        {
                            syncwatch.Stop();
                            bot.Say(syncrns, String.Format("<b>&raquo; Finished syncing for {0} bot{1} and {2} client{3} took <abbr title=\"{4}\">{5}</abbr></b>", bots_synced, bots_synced == 1 ? "" : "s", clients_synced, clients_synced == 1 ? "" : "s", syncwatch.Elapsed, Tools.FormatTime((ulong)syncwatch.Elapsed.TotalSeconds)));
                        }
                        syncwith = "";
                        syncrns = "";
                        bots_synced = 0;
                        clients_synced = 0;
                        bot.NPSay(ns, "BDS:LINK:CLOSED");
                        bot.Part(ns);
                        syncing = false;
                        isrequester = false;
                    }

                    else if (bits.Length >= 4 && bits[2] == "INFO")
                    {
                        if (!bits[3].Contains(","))
                            return;

                        String input = String.Empty;

                        for (byte b = 3; b < bits.Length; b++)
                        {
                            if (b >= bits.Length - 1)
                                input += bits[b];
                            else
                                input += bits[b] + ":";
                        }

                        String[] data = input.Split(',');

                        if (data.Length < 6 || !data[3].Contains("/"))
                            return;

                        String who = data[0].ToLower();
                        String[] versions = data[3].Split('/');
                        String botver = versions[0];
                        ulong ts = Bot.EpochTimestamp;
                        String trig = data[5];
                        double bdsver = 0.0;

                        // trigger contains a comma?
                        if (data.Length > 6)
                        {
                            trig = String.Empty;
                            for (int b = 6; b < data.Length; b++)
                            {
                                if (b >= data.Length - 1)
                                    trig += data[b];
                                else
                                    trig += data[b] + ",";
                            }
                        }

                        if (!Double.TryParse(versions[1], out bdsver))
                            bdsver = 0.2;

                        Types.BotInfo bot_info = new Types.BotInfo(data[0], data[1], data[2], botver, trig, bdsver, ts);

                        if (!_botinfo_database.ContainsKey(who))
                            _botinfo_database.Add(who, bot_info);
                        else
                            _botinfo_database[who] = bot_info;

                        if (Program.Debug)
                            ConIO.Write("Updated information for bot: " + data[0], "BDS");

                        bots_synced++;
                    }

                    else if (bits.Length >= 4 && bits[2] == "CLIENTINFO")
                    {
                        if (!bits[3].Contains(","))
                            return;

                        String input = String.Empty;

                        for (byte b = 3; b < bits.Length; b++)
                        {
                            if (b >= bits.Length - 1)
                                input += bits[b];
                            else
                                input += bits[b] + ":";
                        }

                        String[] data = input.Split(',');

                        if (data.Length < 4 || !data[2].Contains("/"))
                            return;

                        String who = data[0].ToLower();
                        String[] versions = data[2].Split('/');
                        String clientver = versions[0];
                        ulong ts = Bot.EpochTimestamp;
                        double bdsver = 0.0;

                        if (!Double.TryParse(versions[1], out bdsver))
                            bdsver = 0.2;

                        Types.ClientInfo client_info = new Types.ClientInfo(data[0], data[1], clientver, bdsver, ts);

                        if (!_clientinfo_database.ContainsKey(who))
                            _clientinfo_database.Add(who, client_info);
                        else
                            _clientinfo_database[who] = client_info;

                        if (Program.Debug)
                            ConIO.Write("Updated information for client: " + data[0], "BDS");

                        clients_synced++;
                    }
                }

                else if (bits.Length >= 3 && bits[1] == "LINK")
                {
                    if (bits.Length == 4 && bits[3].ToLower() == username.ToLower())
                    {
                        if (bits[2] == "ACCEPT" && from.ToLower() == syncwith)
                        {
                            bot.Join(Tools.FormatPCNS(from, username));
                        }
                        else if (bits[2] == "REJECT" && from.ToLower() == syncwith)
                        {
                            syncwith = "";
                            syncing = false;
                            isrequester = false;
                        }
                        else if (bits[2] == "REQUEST" && !syncing && from.ToLower() == syncwith)
                        {
                            syncing = true;
                            bot.NPSay(ns, "BDS:LINK:ACCEPT:" + from);
                            bot.Join(Tools.FormatPCNS(from, username));
                        }
                    }
                }

                else if (bits.Length >= 4 && bits[1] == "SEEN" && IsPoliceBot(username, pboverride: true))
                {
                    if (bits[2] == "REQUEST" && bits[3].Contains(','))
                    {
                        var payload = bits[3].Split(',');

                        if (payload.Length >= 2 && payload[0].ToLower() == username.ToLower())
                        {
                            var who = payload[1].ToLower();

                            if (!BDS._seen_database.ContainsKey(who))
                                bot.NPSay(ns, "BDS:SEEN:NODATA:" + from + "," + payload[1]);
                            else
                            {
                                var info = BDS._seen_database[who];
                                bot.NPSay(ns, String.Format("BDS:SEEN:RESPONSE:{0},{1},{2},{3},{4}", from, info.Name, info.Type, info.Channel, info.Timestamp));
                            }
                        }
                    }

                    else if (bits[2] == "PROVIDER" && bits[3].ToLower() == username.ToLower())
                    {
                        if (!SeenProviders.Contains(from.ToLower()) && IsPoliceBot(from, ns))
                            SeenProviders.Add(from.ToLower());
                    }
                }

                else if (bits.Length >= 4 && bits[1] == "PROVIDER" && bits[2] == "CAPS" && IsPoliceBot(username, pboverride: true))
                {
                    if (bits[3].Contains(',') && from.ToLower() != username.ToLower())
                    {
                        var payload = new List<String>(bits[3].ToLower().Split(','));

                        if (payload.Contains("seen"))
                            bot.NPSay(ns, "BDS:SEEN:PROVIDER:" + from);
                    }

                    else if (bits[3].ToLower() == "seen")
                    {
                        bot.NPSay(ns, "BDS:SEEN:PROVIDER:" + from);
                    }
                }

                else if (bits.Length >= 3 && bits[1] == "BOTCHECK")
                {
                    if (bits[2] == "OK" && bits.Length >= 4 && bits[3].ToLower() == username.ToLower())
                    {
                        if (!from_policebot || from.ToLower() == username.ToLower())
                            return;

                        if (!IsPoliceBot(username, "chat:DSGateway", true))
                            bot.Part("chat:DSGateWay");

                        bot.Join("chat:DataShare");
                    }
                    else if (bits[2] == "DENIED" && bits.Length >= 4 && bits[3].ToLower().StartsWith(username.ToLower() + ','))
                    {
                        if (!from_policebot)
                            return;

                        // Look for a valid string
                        if (!bits[3].Contains(","))
                            return;

                        String input = String.Empty;

                        for (byte b = 3; b < bits.Length; b++)
                        {
                            if (b >= bits.Length - 1)
                                input += bits[b];
                            else
                                input += bits[b] + ":";
                        }

                        String reason = input.Substring(username.Length + 1);

                        ConIO.Warning("#DataShare", "Denied access: " + reason);

                        bot.Part("chat:DSGateway");
                    }
                    else if (bits[2] == "ALL" || (bits.Length >= 4 && bits[2] == "DIRECT" && bits[3].ToLower() == username.ToLower()))
                    {
                        // If it's not a police bot, return.
                        if (bits[2] == "ALL" && !from_policebot)
                            return;

                        String hashkey = Tools.md5((trigger + from + username).Replace(" ", "").ToLower());
                        bot.NPSay(ns, String.Format("BDS:BOTCHECK:RESPONSE:{0},{1},{2},{3}/{4},{5},{6}", from, owner, Program.BotName, Program.Version, Version, hashkey, trigger));
                    }
                    else if (bits[2] == "DIRECT" && bits[3].ToLower().Contains(","))
                    {
                        List<String> bots = new List<String>(bits[3].ToLower().Split(new char[] { ',' }));

                        if (bots.Contains(username.ToLower()))
                        {
                            String hashkey = Tools.md5((trigger + from + username).Replace(" ", "").ToLower());
                            bot.NPSay(ns, String.Format("BDS:BOTCHECK:RESPONSE:{0},{1},{2},{3}/{4},{5},{6}", from, owner, Program.BotName, Program.Version, Version, hashkey, trigger));
                        }
                    }
                    else if (bits[2] == "RESPONSE" && bits.Length >= 4)
                    {
                        // Look for a valid string
                        if (!bits[3].Contains(","))
                            return;

                        // Possibly add privclass/client checks

                        // Handle it
                        String input = String.Empty;

                        for (byte b = 3; b < bits.Length; b++)
                        {
                            if (b >= bits.Length - 1)
                                input += bits[b];
                            else
                                input += bits[b] + ":";
                        }

                        String[] data = input.Split(',');

                        // Invalid data
                        if (data.Length < 6 || !data[3].Contains("/"))
                            return;

                        String[] versions = data[3].Split('/');
                        String botver = versions[0];
                        String hash = data[4];
                        String trig = data[5];
                        double bdsver = 0.0;

                        // trigger contains a comma?
                        if (data.Length > 6)
                        {
                            trig = String.Empty;
                            for (int b = 6; b < data.Length; b++)
                            {
                                if (b >= data.Length - 1)
                                    trig += data[b];
                                else
                                    trig += data[b] + ",";
                            }
                        }

                        if (!Double.TryParse(versions[1], out bdsver))
                            bdsver = 0.2;

                        Types.BotInfo bot_info = new Types.BotInfo(from, data[1], data[2], botver, trig, bdsver, Bot.EpochTimestamp);

                        String hashkey = Tools.md5((trig + data[0] + from).ToLower().Replace(" ", "")).ToLower();

                        ClearKickTimers(from);

                        if (hashkey != hash)
                        {
                            // Invalid hash supplied
                            // For now, we ignore this. Though I'd like to see policebots send and error like:
                            //  BDS:BOTCHECK:ERROR:INVALID_RESPONSE_HASH

                            // Police bot stuff.
                            if ((ns == "chat:DSGateway" || ns == "chat:DataShare") && IsPoliceBot(username, ns))
                            {
                                if (ns == "chat:DSGateway")
                                    bot.NPSay(ns, "BDS:BOTCHECK:DENIED:" + from + ",Invalid BDS:BOTCHECK");

                                bot.Kick(ns, from, "No response to or invalid BDS:BOTCHECK. If you are not a bot, please do not join this room. Thanks.");
                                bot.Promote("chat:DataShare", from, "BrokenBots");

                                if (!Kicks.ContainsKey(from))
                                    Kicks.Add(from, new KickInfo());

                                Kicks[from].Kick();

                                if (Kicks[from].Count >= 3)
                                {
                                    bot.Ban(ns, from);
                                    var t = new Timer(5000);
                                    t.Elapsed += delegate { bot.UnBan(ns, from); };
                                    t.Start();
                                }
                            }

                            if (Program.Debug)
                                ConIO.Warning("BDS", "Invalid hash for bot: " + from);
                        }
                        else
                        {
                            // Police bot stuff.
                            if ((ns == "chat:DSGateway" || ns == "chat:DataShare") && IsPoliceBot(username, ns))
                            {
                                if (ns == "chat:DSGateway")
                                {
                                    if (!GateChecks.Contains(from))
                                        GateChecks.Add(from);
                                    bot.NPSay(ns, "BDS:BOTCHECK:OK:" + from);
                                }

                                bot.Promote("chat:DataShare", from, "Bots");
                            }

                            lock (_botinfo_database)
                            {
                                if (_botinfo_database.ContainsKey(from.ToLower()))
                                {
                                    _botinfo_database[from.ToLower()] = bot_info;

                                    if (Program.Debug)
                                        ConIO.Write("Updated database for bot: " + from, "BDS");
                                }
                                else
                                {
                                    _botinfo_database.Add(from.ToLower(), bot_info);

                                    if (Program.Debug)
                                        ConIO.Write("Added bot to database: " + from, "BDS");
                                }
                            }
                        }

                    }
                    else if (bits[2] == "CLIENT" && bits.Length >= 4)
                    {
                        // Look for a valid string
                        if (!bits[3].Contains(","))
                            return;

                        // Handle it
                        String input = String.Empty;

                        for (byte b = 3; b < bits.Length; b++)
                        {
                            if (b >= bits.Length - 1)
                                input += bits[b];
                            else
                                input += bits[b] + ":";
                        }

                        String[] data = input.Split(',');

                        // Invalid data
                        if (data.Length < 4)
                            return;

                        String name   = data[1];
                        String[] vers;
                        String ver    = String.Empty;
                        String hash   = data[3];
                        double bdsver = 0.0;

                        if (data[2].Contains('/'))
                        {
                            vers = data[2].Split('/');
                            ver = vers[0];

                            if (!Double.TryParse(vers[vers.Length - 1], out bdsver))
                                bdsver = 0.2;

                            if (vers.Length > 2)
                                ver = data[2].Substring(0, data[2].LastIndexOf('/'));
                        }
                        else
                        {
                            ver = data[2];
                        }

                        Types.ClientInfo client_info = new ClientInfo(from, name, ver, bdsver, Bot.EpochTimestamp);

                        String hashkey = String.Empty;

                        if (bdsver == 0.0)
                        {
                            hashkey = Tools.md5((name + ver + from + data[0]).Replace(" ", "").ToLower()).ToLower();
                        }
                        else
                        {
                            hashkey = Tools.md5((name + ver + "/" + bdsver + from + data[0]).Replace(" ", "").ToLower()).ToLower();
                        }

                        ClearKickTimers(from);

                        if (hashkey != hash)
                        {
                            // Invalid hash supplied
                            // For now, we ignore this. Though I'd like to see policebots send and error like:
                            //  BDS:BOTCHECK:ERROR:INVALID_RESPONSE_HASH

                            // Police bot stuff.
                            if ((ns == "chat:DSGateway" || ns == "chat:DataShare") && IsPoliceBot(username, ns))
                            {
                                if (ns == "chat:DSGateway")
                                    bot.NPSay(ns, "BDS:BOTCHECK:DENIED:" + from + ",Invalid BDS:BOTCHECK");

                                bot.Kick(ns, from, "No response to or invalid BDS:BOTCHECK. If you are not a bot, please do not join this room. Thanks.");
                                bot.Promote("chat:DataShare", from, "BrokenClients");

                                if (!Kicks.ContainsKey(from))
                                    Kicks.Add(from, new KickInfo());

                                Kicks[from].Kick();

                                if (Kicks[from].Count >= 3)
                                {
                                    bot.Ban(ns, from);
                                    var t = new Timer(5000);
                                    t.Elapsed += delegate { bot.UnBan(ns, from); };
                                    t.Start();
                                }
                            }

                            if (Program.Debug)
                                ConIO.Warning("BDS", "Invalid hash for client: " + from);
                        }
                        else
                        {
                            // Police bot stuff.
                            if ((ns == "chat:DSGateway" || ns == "chat:DataShare") && IsPoliceBot(username, ns))
                            {
                                if (ns == "chat:DSGateway")
                                {
                                    if (!GateChecks.Contains(from))
                                        GateChecks.Add(from);
                                    bot.NPSay(ns, "BDS:BOTCHECK:OK:" + from);
                                }

                                bot.Promote("chat:DataShare", from, "Clients");
                            }

                            lock (_clientinfo_database)
                            {
                                if (_clientinfo_database.ContainsKey(from.ToLower()))
                                {
                                    _clientinfo_database[from.ToLower()] = client_info;

                                    if (Program.Debug)
                                        ConIO.Write("Updated database for client: " + from, "BDS");
                                }
                                else
                                {
                                    _clientinfo_database.Add(from.ToLower(), client_info);

                                    if (Program.Debug)
                                        ConIO.Write("Added client to database: " + from, "BDS");
                                }
                            }
                        }

                    }
                    else if (bits.Length >= 4 && bits[2] == "INFO")
                    {
                        if (!bits[3].Contains(","))
                            return;

                        // Handle it
                        String input = String.Empty;

                        for (byte b = 3; b < bits.Length; b++)
                        {
                            if (b >= bits.Length - 1)
                                input += bits[b];
                            else
                                input += bits[b] + ":";
                        }

                        String[] data = input.Split(',');

                        // Invalid data
                        if (data.Length < 5 || !data[2].Contains("/"))
                            return;

                        String[] versions = data[2].Split('/');
                        String botver = versions[0];
                        String trig = data[4];
                        double bdsver = 0.0;

                        // trigger contains a comma?
                        if (data.Length > 5)
                        {
                            trig = String.Empty;
                            for (int b = 5; b < data.Length; b++)
                            {
                                if (b >= data.Length - 1)
                                    trig += data[b];
                                else
                                    trig += data[b] + ",";
                            }
                        }

                        if (!Double.TryParse(versions[1], out bdsver))
                            bdsver = 0.2;

                        Types.BotInfo bot_info = new Types.BotInfo(data[0], data[3], data[1], botver, trig, bdsver, Bot.EpochTimestamp);

                        lock (_botinfo_database)
                        {
                            if (_botinfo_database.ContainsKey(data[0].ToLower()))
                            {
                                _botinfo_database[data[0].ToLower()] = bot_info;

                                if (Program.Debug)
                                    ConIO.Write("Updated database for bot: " + data[0], "BDS");
                            }
                            else
                            {
                                _botinfo_database.Add(data[0].ToLower(), bot_info);

                                if (Program.Debug)
                                    ConIO.Write("Added bot to database: " + data[0], "BDS");
                            }
                        }

                        lock (_info_requests)
                        {
                            if (_info_requests.ContainsKey(data[0].ToLower()))
                            {
                                String chan = _info_requests[data[0].ToLower()];
                                _info_requests.Remove(data[0].ToLower());

                                String output = String.Format("<b>&raquo; Information on :dev{0}:</b><br/>", bot_info.Name);
                                output += String.Format("<b>Bot type:</b> {0}<br/>", bot_info.Type);
                                output += String.Format("<b>Bot version:</b> {0}<br/>", bot_info.Version);
                                output += String.Format("<b>Bot owner:</b> :dev{0}:<br/>", bot_info.Owner);
                                output += String.Format("<b>Bot trigger:</b> <b><code>{0}</code></b><br/>", bot_info.Trigger.Replace("&", "&amp;"));
                                output += String.Format("<b>BDS version:</b> {0}<br/>", bot_info.BDSVersion);
                                bot.Say(chan, output);
                            }
                        }
                    }
                    else if (bits.Length >= 4 && bits[2] == "CLIENTINFO")
                    {
                        if (!bits[3].Contains(","))
                            return;

                        // Handle it
                        String input = String.Empty;

                        for (byte b = 3; b < bits.Length; b++)
                        {
                            if (b >= bits.Length - 1)
                                input += bits[b];
                            else
                                input += bits[b] + ":";
                        }

                        String[] data = input.Split(',');

                        // Invalid data
                        if (data.Length < 3)
                            return;

                        String name = data[1];
                        String[] vers = data[2].Split('/');
                        String ver = vers[0];

                        double bdsver = 0.2;

                        if (!Double.TryParse(vers[vers.Length - 1], out bdsver))
                            bdsver = 0.2;

                        if (vers.Length > 2)
                            ver = data[2].Substring(0, data[2].LastIndexOf('/'));

                        Types.ClientInfo client_info = new ClientInfo(data[0], name, ver, bdsver, Bot.EpochTimestamp);

                        lock (_clientinfo_database)
                        {
                            if (_clientinfo_database.ContainsKey(data[0].ToLower()))
                            {
                                _clientinfo_database[data[0].ToLower()] = client_info;

                                if (Program.Debug)
                                    ConIO.Write("Updated database for client: " + data[0], "BDS");
                            }
                            else
                            {
                                _clientinfo_database.Add(data[0].ToLower(), client_info);

                                if (Program.Debug)
                                    ConIO.Write("Added client to database: " + data[0], "BDS");
                            }
                        }

                        lock (_info_requests)
                        {
                            if (_info_requests.ContainsKey(data[0].ToLower()))
                            {
                                String chan = _info_requests[data[0].ToLower()];
                                _info_requests.Remove(data[0].ToLower());

                                String output = String.Format("<b>&raquo; Information on :dev{0}:</b><br/>", client_info.Name);
                                output += String.Format("<b>Client type:</b> {0}<br/>", client_info.Type);
                                output += String.Format("<b>Client version:</b> {0}", client_info.Version);
                                bot.Say(chan, output);
                            }
                        }
                    }
                    else if (bits.Length >= 4 && bits[2] == "NODATA")
                    {
                        // Ignore data from non-police bots
                        if (!from_policebot)
                            return;

                        lock (_info_requests)
                        {
                            if (_info_requests.ContainsKey(bits[3].ToLower()))
                            {
                                String chan = _info_requests[bits[3].ToLower()];
                                _info_requests.Remove(bits[3].ToLower());
                                bot.Say(chan, "<b>&raquo; Bot/client doesn't exist:</b> " + bits[3]);
                            }
                        }
                    }
                    else if (bits.Length >= 4 && bits[2] == "BADBOT")
                    {
                        // Ignore data from non-police bots
                        if (!from_policebot)
                            return;

                        if (!bits[3].Contains(","))
                            return;

                        String[] data = bits[3].Split(',');

                        lock (_info_requests)
                        {
                            if (_info_requests.ContainsKey(data[0].ToLower()))
                            {
                                String chan = _info_requests[data[0].ToLower()];
                                _info_requests.Remove(data[0].ToLower());
                                bot.Say(chan, "<b>&raquo; Bot is banned:</b> " + data[0]);
                            }
                        }

                        // Maybe store this later.
                    }
                    else if (bits.Length >= 4 && bits[2] == "BADCLIENT")
                    {
                        // Ignore data from non-police bots
                        if (!from_policebot)
                            return;

                        if (!bits[3].Contains(","))
                            return;

                        String[] data = bits[3].Split(',');

                        lock (_info_requests)
                        {
                            if (_info_requests.ContainsKey(data[0].ToLower()))
                            {
                                String chan = _info_requests[data[0].ToLower()];
                                _info_requests.Remove(data[0].ToLower());
                                bot.Say(chan, "<b>&raquo; Client is banned:</b> " + data[0]);
                            }
                        }

                        // Maybe store this later.
                    }
                }
                else if (bits.Length >= 4 && bits[1] == "BOTDEF")
                {
                    if (bits[2] == "REQUEST" && bits[3] == username.ToLower())
                    {
                        // If it's not the police bot, return.
                        if (from_policebot)
                            return;

                        String hashkey = Tools.md5((from + Program.BotName + "DivinityArcane").ToLower());
                        bot.NPSay(ns, String.Format("BDS:BOTDEF:RESPONSE:{0},{1},{2},{3},{4},{5}", from, Program.BotName, "C#", "DivinityArcane", "http://botdom.com/wiki/LulzBot", hashkey));
                    }
                }
                else if (bits.Length >= 4 && bits[1] == "TRANSLATE" && bits[2] == "RESPONSE")
                {
                    // Ignore data from non-police bots
                    if (!from_policebot)
                        return;

                    if (!bits[3].Contains(","))
                        return;

                    String input = String.Empty;

                    for (byte b = 3; b < bits.Length; b++)
                    {
                        if (b >= bits.Length - 1)
                            input += bits[b];
                        else
                            input += bits[b] + ":";
                    }

                    String[] data = input.Split(',');

                    if (data[0].ToLower() != username.ToLower() || data.Length < 3) return;

                    lock (_translate_requests)
                    {
                        if (_translate_requests.Contains(data[1]))
                        {
                            int id = _translate_requests.IndexOf(data[1]);
                            String chan = _translate_requests[id];
                            _translate_requests.RemoveAt(id);
                            bot.Say(chan, "<b>&raquo; Translated text:</b> " + Tools.HtmlEncode(Encoding.UTF8.GetString(Convert.FromBase64String(data[2]))));
                        }
                    }
                }
            }
            else if (bits[0] == "LDS")
            {
                if (bits.Length >= 4 && bits[1] == "UPDATE")
                {
                    if (bits[2] == "PING" && bits[3].ToLower() == username.ToLower())
                    {
                        bot.NPSay(ns, String.Format("LDS:UPDATE:PONG:{0},{1},{2}", from, Program.BotName, Program.Version));
                    }
                    else if (bits[2] == "NOTIFY")
                    {
                        if (from_policebot || from.ToLower() == "divinityarcane")
                        {
                            if (bits[3].Contains(","))
                            {
                                String[] pars = bits[3].Split(new char[] { ',' });
                                if (pars.Length == 3 && pars[0].ToLower() == username.ToLower())
                                {
                                    int secs = 0;
                                    bool ok = int.TryParse(pars[2], out secs);
                                    if (ok)
                                    {
                                        ConIO.Notice(String.Format("A new version of lulzBot is available: version {0} (Released {1} ago)", pars[1], Tools.FormatTime((ulong)(Tools.Timestamp() - secs))));
                                        //ConIO.Notice(String.Format("To update, use the update command."));
                                    }
                                }
                            }
                        }
                    }
                }
                else if (bits.Length >= 3 && bits[1] == "BOTCHECK")
                {
                    if (bits[2] == "ALL")
                    {
                        if (from_policebot || from.ToLower() == "divinityarcane")
                        {
                            // from, owner, botname, botversion, uptime, disconnects, bytes_sent, bytes_received
                            bot.NPSay(ns, String.Format("LDS:BOTCHECK:RESPONSE:{0},{1},{2},{3},{4},{5},{6},{7}",
                                from, owner, Program.BotName, Program.Version, bot.uptime, Program.Disconnects, Program.bytes_sent, Program.bytes_received));
                        }
                    }
                    else if (bits.Length >= 4 && bits[2] == "DIRECT" && bits[3].ToLower() == username.ToLower())
                    {
                        // from, owner, botname, botversion, uptime, disconnects, bytes_sent, bytes_received
                        bot.NPSay(ns, String.Format("LDS:BOTCHECK:RESPONSE:{0},{1},{2},{3},{4},{5},{6},{7}",
                            from, owner, Program.BotName, Program.Version, bot.uptime, Program.Disconnects, Program.bytes_sent, Program.bytes_received));
                    }
                }
            }
        }

        public static void KickAfter (string chan, string who, int delay, string reason = null)
        {
            var stamp = Timers.Add(1000 * delay, delegate
            {
                Program.Bot.Kick(chan, who, reason);
                ClearKickTimers(who);
            });

            if (!KickTimers.ContainsKey(who.ToLower()))
                KickTimers.Add(who.ToLower(), stamp);

            else
            {
                ClearKickTimers(who);
                KickTimers.Add(who.ToLower(), stamp);
            }
        }

        public static void ClearKickTimers (string who)
        {
            if (KickTimers.ContainsKey(who.ToLower()))
            {
                var stamp = KickTimers[who.ToLower()];
                Timers.Remove(stamp);
                KickTimers.Remove(who.ToLower());
            }
        }
    }
}
