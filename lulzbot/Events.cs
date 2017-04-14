using lulzbot.Extensions;
using lulzbot.Types;
using System;
using System.Collections.Generic;

namespace lulzbot
{
    /// <summary>
    /// This is our event system.
    /// </summary>
    public class Events
    {
        /// <summary>
        /// This is the events dictionary. Each event is a list of Event objects.
        /// </summary>
        private static Dictionary<String, List<Event>> _events          = new Dictionary<string, List<Event>>();
        private static Dictionary<String, List<Event>> _external_events = new Dictionary<string, List<Event>>();
        private static Dictionary<String, Command> _commands            = new Dictionary<string, Command>();
        private static Dictionary<String, Command> _external_commands   = new Dictionary<string, Command>();
        public static Dictionary<String, UInt32> HitCounts              = new Dictionary<string, uint>();
        private static Dictionary<String, UInt64> _last_command            = new Dictionary<string, ulong>();

        /// <summary>
        /// Adds the default event names and lists.
        /// </summary>
        public static void InitEvents ()
        {
            ConIO.Write("Initializing events...");

            // dAmn events
            AddEventType("dAmnServer");
            AddEventType("disconnect");
            AddEventType("get");
            AddEventType("join");
            AddEventType("kick");
            AddEventType("kicked");
            AddEventType("kill");
            AddEventType("login");
            AddEventType("on_connect");
            AddEventType("part");
            AddEventType("ping");
            AddEventType("property");
            AddEventType("recv_action");
            AddEventType("recv_admin");
            AddEventType("evt_recv_admin_show");
            AddEventType("recv_join");
            AddEventType("recv_kicked");
            AddEventType("recv_msg");
            AddEventType("recv_part");
            AddEventType("recv_privchg");
            AddEventType("send");
            AddEventType("set");
            AddEventType("whois");

            // non dAmn events

            AddEventType("log_msg");

            // rp logger events
            AddEventType("start_rp");


            ConIO.Write(String.Format("Initialized {0} events.", _events.Count));
        }

        /// <summary>
        /// Returns the events dict.
        /// </summary>
        /// <returns>events dictionary</returns>
        public static Dictionary<String, List<Types.Event>> GetEvents ()
        {
            return _events;
        }

        /// <summary>
        /// Returns the external events dict.
        /// </summary>
        /// <returns>external events dictionary</returns>
        public static Dictionary<String, List<Types.Event>> GetExternalEvents ()
        {
            return _external_events;
        }

        /// <summary>
        /// Adds a new event list for the specified event name.
        /// </summary>
        /// <param name="event_name">Event name. i.e. recv_msg, do_something</param>
        private static void AddEventType (String event_name)
        {
            lock (_events)
            {
                if (!_events.ContainsKey(event_name))
                {
                    _events.Add(event_name, new List<Event>());
                    _external_events.Add(event_name, new List<Event>());
                    HitCounts.Add(event_name, 0);
                }
            }
        }

        /// <summary>
        /// Checks whether or not an event of the specified name exists
        /// </summary>
        /// <param name="event_name">Event name</param>
        /// <returns>true or false</returns>
        public static bool ValidateEventName (String event_name)
        {
            return _events.ContainsKey(event_name) || _external_events.ContainsKey(event_name);
        }

        /// <summary>
        /// Checks whether or not an event of the specified name exists
        /// </summary>
        /// <param name="cmd_name">Command name</param>
        /// <returns>true or false</returns>
        public static bool ValidateCommandName (String cmd_name)
        {
            return _commands.ContainsKey(cmd_name) || _external_commands.ContainsKey(cmd_name);
        }

        /// <summary>
        /// Get the access level of a specific command
        /// </summary>
        /// <param name="cmd_name">Command name</param>
        /// <returns>Priv level or -1</returns>
        public static int GetCommandAccess (String cmd_name, bool ignore_override = false)
        {
            if (!ignore_override && Core._command_overrides.ContainsKey(cmd_name))
                return (int)Core._command_overrides[cmd_name];
            else if (_commands.ContainsKey(cmd_name))
                return _commands[cmd_name].MinimumPrivs;
            else if (_external_commands.ContainsKey(cmd_name))
                return _external_commands[cmd_name].MinimumPrivs;
            else return -1;
        }

        /// <summary>
        /// Adds an event to the list.
        /// </summary>
        /// <param name="event_name">name of the event</param>
        /// <param name="callback">Event object</param>
        public static void AddEvent (String event_name, Event callback)
        {
            lock (_events)
            {
                if (_events.ContainsKey(event_name))
                {
                    _events[event_name].Add(callback);
                }
                else
                {
                    ConIO.Write("Invalid event: " + event_name, "Events");
                }
            }
        }

        /// <summary>
        /// Adds an event for external extensions
        /// </summary>
        /// <param name="event_name">event name</param>
        /// <param name="callback">callback object</param>
        public static void AddExternalEvent (String event_name, Event callback)
        {
            lock (_external_events)
            {
                if (_external_events.ContainsKey(event_name))
                {
                    _external_events[event_name].Add(callback);
                }
                else
                {
                    ConIO.Write("Invalid event: " + event_name, "Events");
                }
            }
        }

        /// <summary>
        /// Calls all the events bound to the specified event name
        /// </summary>
        /// <param name="event_name">event name</param>
        /// <param name="packet">dAmnPacket object</param>
        public static void CallEvent (String event_name, dAmnPacket packet)
        {
            if (Program.Bot == null) return;
            lock (_events)
            {
                if (_events.ContainsKey(event_name))
                {
                    HitCounts[event_name]++;
                    foreach (Event callback in _events[event_name])
                    {
                        try
                        {
                            if (Program.Bot == null) return;
                            if (Core._disabled_extensions.Contains(callback.Extension.Name.ToLower())) return;
                            callback.Method.Invoke(callback.Class, new object[] { Program.Bot, packet });
                        }
                        catch (Exception E)
                        {
                            ConIO.Warning("Extension", String.Format("Failed to call event {0}.{1}: {2}", callback.ClassName, callback.Method.Name, E.InnerException.Message));
                        }
                    }

                    // External events can't have anything to do with datashare
                    if (packet != null && packet.Parameter.ToLower() != "chat:datashare")
                        CallExternalEvent(event_name, packet);
                }
                else
                {
                    ConIO.Write("Unknown event: " + event_name, "Events");
                }
            }
        }

        /// <summary>
        /// Calls an external event. Only to be called from CallEvent()
        /// </summary>
        /// <param name="event_name"></param>
        /// <param name="packet"></param>
        private static void CallExternalEvent (String event_name, dAmnPacket packet)
        {
            if (Program.Bot == null) return;

            // We will not let it see its own messages! This avoids infinite loops.
            if (event_name == "recv_msg" && packet.Arguments["from"].ToLower() == Program.Bot.Config.Username.ToLower())
                return;

            lock (_external_events)
            {
                // We shouldn't have to go through the shenanigans of re-checking, but for safety sake..
                if (_external_events.ContainsKey(event_name))
                {
                    foreach (Event callback in _external_events[event_name])
                    {
                        try
                        {
                            if (Program.Bot == null) return;
                            if (Core._disabled_extensions.Contains(callback.Extension.Name.ToLower())) return;
                            callback.Method.Invoke(callback.Class, new object[] { packet });
                        }
                        catch (Exception E)
                        {
                            ConIO.Warning("Extension", String.Format("Extension bound to event [{0}] threw exception: {1}", event_name, E.InnerException.Message));
                        }
                    }
                }
                else
                {
                    ConIO.Write("Unknown external event: " + event_name, "Events");
                }
            }
        }

        /// <summary>
        /// Calls all the events bound to the specified event name
        /// (For non dAmn events)
        /// </summary>
        /// <param name="event_name">event name</param>
        /// <param name="parameters">list of parameters to be passed to the events</param>
        public static void CallSpecialEvent (String event_name, object[] parameters)
        {
            bool OK = false;
            lock (_events)
            {
                if (_events.ContainsKey(event_name))
                {
                    foreach (Event callback in _events[event_name])
                    {
                        if (Core._disabled_extensions.Contains(callback.Extension.Name.ToLower())) return;
                        callback.Method.Invoke(callback.Class, parameters);
                    }
                    OK = true;
                }

                if (_external_events.ContainsKey(event_name))
                {
                    foreach (Event callback in _external_events[event_name])
                    {
                        if (Core._disabled_extensions.Contains(callback.Extension.Name.ToLower())) return;
                        callback.Method.Invoke(callback.Class, parameters);
                    }
                    OK = true;
                }

                if (!OK)
                    ConIO.Write("Unknown special event: " + event_name, "Events");
                else
                    HitCounts[event_name]++;
            }
        }

        /// <summary>
        /// Adds a command to the list.
        /// </summary>
        /// <param name="cmd_name">name of the command</param>
        /// <param name="callback">Command object</param>
        public static void AddCommand (String cmd_name, Command callback)
        {
            lock (_commands)
            {
                if (!CommandExists(cmd_name.ToLower()))
                {
                    _commands[cmd_name.ToLower()] = callback;
                }
                else
                {
                    ConIO.Write("Duplicate command: " + cmd_name.ToLower(), "Events");
                }
            }
        }

        /// <summary>
        /// Adds an external command
        /// </summary>
        /// <param name="cmd_name">name of the command</param>
        /// <param name="callback">callback object</param>
        public static void AddExternalCommand (String cmd_name, Command callback)
        {
            lock (_external_commands)
            {
                if (!CommandExists(cmd_name.ToLower()))
                {
                    _external_commands[cmd_name.ToLower()] = callback;
                }
                else
                {
                    ConIO.Write("Duplicate external command: " + cmd_name.ToLower(), "Events");
                }
            }
        }

        /// <summary>
        /// Checks whether or not a command name is taken already
        /// </summary>
        /// <param name="cmd_name">command name</param>
        /// <returns>true or false</returns>
        public static bool CommandExists (String cmd_name)
        {
            return _commands.ContainsKey(cmd_name.ToLower()) || _external_commands.ContainsKey(cmd_name.ToLower());
        }

        /// <summary>
        /// Calls the command bound to the specified command name
        /// </summary>
        /// <param name="cmd_name">command name</param>
        public static void CallCommand (String cmd_name, dAmnPacket packet)
        {
            if (Core._disabled_commands.Contains(cmd_name.ToLower())) return;

            bool a = false, b = false;
            if ((a = _commands.ContainsKey(cmd_name.ToLower())) || (b = _external_commands.ContainsKey(cmd_name.ToLower())))
            {
                Command callback = null;

                String from = String.Empty;
                if (packet.Arguments.ContainsKey("from"))
                    from = packet.Arguments["from"];

                if (!_last_command.ContainsKey(from))
                    _last_command.Add(from, 0);

                if (Bot.EpochTimestampMS - _last_command[from] < 1000) return;

                String[] cmd_args;
                String msg = packet.Body.Substring(Program.Bot.Config.Trigger.Length);

                if (packet.Body.Contains(" "))
                    cmd_args = msg.Split(' ');
                else
                    cmd_args = new String[1] { msg };

                if (a)
                {
                    callback = _commands[cmd_name.ToLower()];

                    if (Core._disabled_extensions.Contains(callback.Extension.Name.ToLower())) return;

                    if (cmd_args.Length >= 2 && (cmd_args[1] == "?" || cmd_args[1] == "-?"))
                    {
                        Program.Bot.Say(packet.Parameter, "<b>&raquo; Help for command <i>" + cmd_name.ToLower() + "</i>:</b><br/>" + callback.Help.Replace("[trig]", Program.Bot.Config.Trigger));
                        return;
                    }

                    // Access denied
                    if (!Users.CanAccess(from, GetCommandAccess(cmd_name), cmd_name.ToLower()))
                        return;

                    _last_command[from] = Bot.EpochTimestampMS;

                    try
                    {
                        callback.Method.Invoke(callback.Class, new object[] { Program.Bot, packet.Parameter, cmd_args, msg, from, packet });
                    }
                    catch (Exception E)
                    {
                        ConIO.Warning("Extension", String.Format("Failed to call command {0} ({1}): {2}", callback.Method.Name, callback.Description, E.InnerException.Message));
                    }
                }

                if (b)
                    CallExternalCommand(cmd_name, Tools.FormatChat(packet.Parameter), msg, cmd_args, from);
            }
            else
            {
                ConIO.Write("Unknown command: " + cmd_name.ToLower(), "Events");
            }
        }

        /// <summary>
        /// Call an external command
        /// </summary>
        /// <param name="cmd_name">command name</param>
        /// <param name="chan">channel the command originated from</param>
        /// <param name="msg">message the person spoke</param>
        /// <param name="args">message arguments</param>
        /// <param name="from">person who initiated the command</param>
        public static void CallExternalCommand (String cmd_name, String chan, String msg, String[] args, String from)
        {
            if (Core._disabled_commands.Contains(cmd_name.ToLower())) return;

            if (!_last_command.ContainsKey(from))
                _last_command.Add(from, 0);

            if (Bot.EpochTimestampMS - _last_command[from] < 1000) return;

            if (_external_commands.ContainsKey(cmd_name.ToLower()))
            {
                Command callback = _external_commands[cmd_name.ToLower()];

                if (Core._disabled_extensions.Contains(callback.Extension.Name.ToLower())) return;

                if (args.Length >= 2 && (args[1] == "?" || args[1] == "-?"))
                {
                    Program.Bot.Say(chan, "<b>&raquo; Help for command <i>" + cmd_name.ToLower() + "</i>:</b><br/>" + callback.Help.Replace("[trig]", Program.Bot.Config.Trigger));
                    return;
                }

                // Access denied
                if (!Users.CanAccess(from, GetCommandAccess(cmd_name), cmd_name.ToLower()))
                    return;

                _last_command[from] = Bot.EpochTimestampMS;

                try
                {
                    callback.Method.Invoke(callback.Class, new object[] { chan, msg, args, from });
                }
                catch (Exception E)
                {
                    ConIO.Warning("Extension", String.Format("Failed to call command {0} ({1}): {2}", callback.Method.Name, callback.Description, E.InnerException.Message));
                }
            }
            else
            {
                ConIO.Write("Unknown command: " + cmd_name.ToLower(), "Events");
            }
        }

        /// <summary>
        /// Clears all the events and commands.
        /// </summary>
        public static void ClearEvents ()
        {
            lock (_events)
            {
                foreach (String event_name in _events.Keys)
                {
                    _events[event_name].Clear();
                }

                foreach (String event_name in _external_events.Keys)
                {
                    _external_events[event_name].Clear();
                }

                _commands.Clear();
                _external_commands.Clear();
            }
        }

        /// <summary>
        /// Clears all the events and commands.
        /// </summary>
        public static void ClearExternalEvents ()
        {
            lock (_events)
            {
                foreach (String event_name in _external_events.Keys)
                {
                    _external_events[event_name].Clear();
                }

                _external_commands.Clear();
            }
        }

        /// <summary>
        /// Returns a list of all commands accessable to privlevel minimum_priv_level
        /// </summary>
        /// <param name="minimum_priv_level">Minimum privilege level</param>
        /// <returns>Sorted list of command names</returns>
        public static List<String> GetAvailableCommands (String username)
        {
            List<String> list = new List<string>();

            int pl = 25;
            String who = "";

            if (username == null)
                pl = byte.MaxValue;
            else
                who = username.ToLower();

            List<String> whitelist = null, blacklist = null;

            if (Users.userdata.ContainsKey(who))
            {
                pl = Users.userdata[who].PrivLevel;
                whitelist = Users.userdata[who].Access;
                blacklist = Users.userdata[who].Banned;
            }

            if (whitelist == null)
            {
                whitelist = new List<String>();
                blacklist = new List<String>();
            }

            lock (_commands)
            {
                foreach (KeyValuePair<String, Command> KVP in _commands)
                {
                    if (blacklist.Contains(KVP.Key)) continue;
                    if (Core._disabled_commands.Contains(KVP.Key)) continue;
                    if (Core._disabled_extensions.Contains(KVP.Value.Extension.Name.ToLower())) continue;
                    if (GetCommandAccess(KVP.Key) <= pl || whitelist.Contains(KVP.Key))
                        list.Add(KVP.Key);
                }
            }

            lock (_external_commands)
            {
                foreach (KeyValuePair<String, Command> KVP in _external_commands)
                {
                    if (blacklist.Contains(KVP.Key)) continue;
                    if (Core._disabled_commands.Contains(KVP.Key)) continue;
                    if (Core._disabled_extensions.Contains(KVP.Value.Extension.Name.ToLower())) continue;
                    if (GetCommandAccess(KVP.Key) <= pl || whitelist.Contains(KVP.Key))
                        list.Add(KVP.Key);
                }
            }

            list.Sort();

            return list;
        }

        public static String CommandDescription (string cmd)
        {
            if (!ValidateCommandName(cmd)) return String.Empty;
            else if (_commands.ContainsKey(cmd)) return _commands[cmd].Description;
            else if (_external_commands.ContainsKey(cmd)) return _external_commands[cmd].Description;
            return null;
        }

        public static Command CommandInfo (string cmd)
        {
            if (!ValidateCommandName(cmd)) return null;
            else if (_commands.ContainsKey(cmd)) return _commands[cmd];
            else if (_external_commands.ContainsKey(cmd)) return _external_commands[cmd];
            return null;
        }
    }
}
