using lulzbot.Types;
using System;
using System.Collections.Generic;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static Dictionary<String, Types.ChatData> ChannelData = new Dictionary<String, Types.ChatData>();

        /// <summary>
        /// Keeps track of which channels certain commands were sent from.
        /// </summary>
        public static Dictionary<String, List<String>> CommandChannels = new Dictionary<String, List<String>>();

        public static List<String> _disabled_commands;
        public static List<String> _disabled_extensions;
        public static Dictionary<String, Privs> _command_overrides;

        /// <summary>
        /// Constructor. Add basic events.
        /// </summary>
        public Core ()
        {
            var info = new ExtensionInfo("Core", "DivinityArcane", "1.0");

            Events.AddEvent("on_connect", new Event(this, "evt_connect", ext: info));
            Events.AddEvent("dAmnServer", new Event(this, "evt_preauth", ext: info));
            Events.AddEvent("login", new Event(this, "evt_login", ext: info));
            Events.AddEvent("join", new Event(this, "evt_join", ext: info));
            Events.AddEvent("part", new Event(this, "evt_part", ext: info));
            Events.AddEvent("property", new Event(this, "evt_property", ext: info));
            Events.AddEvent("recv_msg", new Event(this, "evt_recv_msg", ext: info));
            Events.AddEvent("recv_action", new Event(this, "evt_recv_action", ext: info));
            Events.AddEvent("recv_join", new Event(this, "evt_recv_join", ext: info));
            Events.AddEvent("recv_part", new Event(this, "evt_recv_part", ext: info));
            Events.AddEvent("recv_privchg", new Event(this, "evt_recv_privchg", ext: info));
            Events.AddEvent("recv_kicked", new Event(this, "evt_recv_kicked", ext: info));
            Events.AddEvent("recv_admin", new Event(this, "evt_recv_admin", ext: info));
            Events.AddEvent("kicked", new Event(this, "evt_kicked", ext: info));
            Events.AddEvent("disconnect", new Event(this, "evt_disconnect", ext: info));
            Events.AddEvent("send", new Event(this, "evt_send_error", ext: info));
            Events.AddEvent("kick", new Event(this, "evt_kick_error", ext: info));
            Events.AddEvent("get", new Event(this, "evt_get_error", ext: info));
            Events.AddEvent("set", new Event(this, "evt_set_error", ext: info));
            Events.AddEvent("kill", new Event(this, "evt_kill_error", ext: info));
            Events.AddEvent("ping", new Event(this, "evt_ping", ext: info));

            Events.AddCommand("about", new Command(this, "cmd_about", "DivinityArcane", 25, "Displays information about the bot.", "", ext: info));
            Events.AddCommand("autojoin", new Command(this, "cmd_autojoin", "DivinityArcane", 100, "Manages the bots autojoined channels.", "[trig]autojoin list<br/>[trig]autojoin add/del #chan", ext: info));
            Events.AddCommand("act", new Command(this, "cmd_act", "DivinityArcane", 75, "Makes the bot say the specified message to the specified channel.", "[trig]act <i>#chan</i> msg", ext: info));
            Events.AddCommand("admin", new Command(this, "cmd_admin", "DivinityArcane", 75, "Makes the bot send the specified admin command to the specified channel.", "[trig]admin <i>#chan</i> command", ext: info));
            Events.AddCommand("ban", new Command(this, "cmd_ban", "DivinityArcane", 75, "Bans the specified user in the specified channel.", "[trig]ban <i>#chan</i> username", ext: info));
            Events.AddCommand("chat", new Command(this, "cmd_chat", "DivinityArcane", 75, "Makes the bot join a private chat.", "", ext: info));
            Events.AddCommand("channels", new Command(this, "cmd_channels", "DivinityArcane", 50, "Displays the channels the bot has joined.", "", ext: info));
            Events.AddCommand("clear", new Command(this, "cmd_clear", "DivinityArcane", 100, "Clears the console.", "", ext: info));
            Events.AddCommand("cycle", new Command(this, "cmd_cycle", "DivinityArcane", 75, "Makes the bot part and join a channel.", "[trig]cycle <i>#chan</i>", ext: info));
            Events.AddCommand("command", new Command(this, "cmd_command", "DivinityArcane", 100, "Disables certain commands.", "[trig]command list<br/>[trig]command enable/disable command_name", ext: info));
            Events.AddCommand("commands", new Command(this, "cmd_commands", "DivinityArcane", 25, "Displays commands available to the user.", "[trig]commands <i>all/mods</i>", ext: info));
            Events.AddCommand("ctrig", new Command(this, "cmd_ctrig", "DivinityArcane", 100, "Changes the bot's trigger.", "[trig]ctrig new_trigger", ext: info));
            Events.AddCommand("credits", new Command(this, "cmd_credits", "DivinityArcane", 25, "Bot credits", ext: info));
            Events.AddCommand("debug", new Command(this, "cmd_debug", "DivinityArcane", 100, "Toggles debug mode.", "[trig]debug on/off", ext: info));
            Events.AddCommand("demote", new Command(this, "cmd_demote", "DivinityArcane", 75, "Demotes the specified user in the specified channel.", "[trig]demote <i>#chan</i> username <i>privclass</i>", ext: info));
            Events.AddCommand("disconnects", new Command(this, "cmd_disconnects", "DivinityArcane", 25, "Displays how many times the bot has disconnected since startup.", "", ext: info));
            Events.AddCommand("exec", new Command(this, "cmd_exec", "DivinityArcane", 100, "Executes a system command.", "[trig]exec command(s)", ext: info));
            Events.AddCommand("ext", new Command(this, "cmd_ext", "DivinityArcane", 100, "Manages disabled extensions.", "[trig]ext list<br/>[trig]ext enable/disable extension_name", ext: info));
            Events.AddCommand("eval", new Command(this, "cmd_eval", "DivinityArcane", 100, "Evaluates C# code.", "[trig]eval C#_code", ext: info));
            Events.AddCommand("event", new Command(this, "cmd_event", "DivinityArcane", 25, "Gets information on the events system.", "[trig]event hitcount/list<br/>[trig]event info event_name", ext: info));
            Events.AddCommand("get", new Command(this, "cmd_get", "DivinityArcane", 50, "Gets the specified data for the specified channel.", "[trig]get <i>#chan</i> property", ext: info));
            Events.AddCommand("help", new Command(this, "cmd_help", "DivinityArcane", 25, "Checks the description of the specified command.", "[trig]help command_name", ext: info));
            Events.AddCommand("join", new Command(this, "cmd_join", "DivinityArcane", 75, "Makes the bot join the specified channel.", "[trig]join #chan", ext: info));
            Events.AddCommand("kick", new Command(this, "cmd_kick", "DivinityArcane", 75, "Makes the bot kick the specified person in the specified channel.", "[trig]kick <i>#chan</i> username <i>reason</i>", ext: info));
            Events.AddCommand("kill", new Command(this, "cmd_kill", "DivinityArcane", 75, "Makes the bot kill the specified person.", "[trig]kill username <i>reason</i>", ext: info));
            Events.AddCommand("netusage", new Command(this, "cmd_netinfo", "DivinityArcane", 25, "Gets information on the network usage of the bot.", "[trig]netusage <i>verbose</i>", ext: info));
            Events.AddCommand("netinfo", new Command(this, "cmd_netinfo", "DivinityArcane", 25, "Gets information on the network usage of the bot.", "[trig]netinfo <i>verbose</i>", ext: info));
            Events.AddCommand("npsay", new Command(this, "cmd_npsay", "DivinityArcane", 75, "Makes the bot say the specified message to the specified channel.", "[trig]npsay <i>#chan</i> msg", ext: info));
            Events.AddCommand("override", new Command(this, "cmd_override", "DivinityArcane", 100, "Changes the minimum required priv level for a specified command.", "[trig]override command_name priv_level", ext: info));
            Events.AddCommand("part", new Command(this, "cmd_part", "DivinityArcane", 75, "Makes the bot leave the specified channel.", "[trig]part <i>#chan</i>", ext: info));
            Events.AddCommand("ping", new Command(this, "cmd_ping", "DivinityArcane", 25, "Tests the latency between the bot and the server.", "", ext: info));
            Events.AddCommand("promote", new Command(this, "cmd_promote", "DivinityArcane", 75, "Promotes the specified user in the specified channel.", "[trig]promote <i>#chan</i> username <i>privclass</i>", ext: info));
            Events.AddCommand("quit", new Command(this, "cmd_quit", "DivinityArcane", 100, "Closes the bot down gracefully.", "", ext: info));
            Events.AddCommand("reload", new Command(this, "cmd_reload", "DivinityArcane", 100, "Reloads external commands.", "", ext: info));
            Events.AddCommand("say", new Command(this, "cmd_say", "DivinityArcane", 75, "Makes the bot say the specified message to the specified channel.", "[trig]say <i>#chan</i> msg", ext: info));
            Events.AddCommand("set", new Command(this, "cmd_set", "DivinityArcane", 75, "Sets the specified data for the specified channel.", "[trig]set <i>#chan</i> property value", ext: info));
            Events.AddCommand("sudo", new Command(this, "cmd_sudo", "DivinityArcane", 100, "Runs a command as the specified user.", "[trig]sudo username cmd <i>args</i>", ext: info));
            Events.AddCommand("system", new Command(this, "cmd_system", "DivinityArcane", 25, "Gets information on the host machine.", "", ext: info));
            Events.AddCommand("unban", new Command(this, "cmd_unban", "DivinityArcane", 75, "Un-bans the specified user in the specified channel.", "[trig]unban <i>#chan</i> username", ext: info));
            Events.AddCommand("update", new Command(this, "cmd_update", "DivinityArcane", 25, "Checks if the bot is up to date.", "", ext: info));
            Events.AddCommand("uptime", new Command(this, "cmd_uptime", "DivinityArcane", 25, "Returns how long the bot has been running.", "", ext: info));
            Events.AddCommand("whois", new Command(this, "cmd_whois", "DivinityArcane", 25, "Performs a whois on the specified user.", "[trig]whois username", ext: info));

            String[] c_types = new String[] { "join", "part", "send", "set", "kick", "kill", "promote", "demote", "admin", "whois" };
            CommandChannels.Clear();

            foreach (String c_type in c_types)
                CommandChannels.Add(c_type, new List<String>());

            _disabled_commands = Storage.Load<List<String>>("disabled_commands");

            if (_disabled_commands == null)
                _disabled_commands = new List<String>();

            _disabled_extensions = Storage.Load<List<String>>("disabled_extensions");

            if (_disabled_extensions == null)
                _disabled_extensions = new List<String>();

            _command_overrides = Storage.Load<Dictionary<String, Privs>>("overridden_commands");

            if (_command_overrides == null)
                _command_overrides = new Dictionary<String, Privs>();
        }

        private static void SaveDisabled ()
        {
            Storage.Save("disabled_commands", _disabled_commands);
            Storage.Save("disabled_extensions", _disabled_extensions);
        }

        private static void SaveOverrides ()
        {
            Storage.Save("overridden_commands", _command_overrides);
        }
    }
}
