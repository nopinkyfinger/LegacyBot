using lulzbot.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace lulzbot.Types
{

    #region BDS related
    /// <summary>
    /// Information on a username for BDS:SEEN.
    /// </summary>
    public class SeenInfo
    {
        public String Name      = String.Empty;
        public String Channel   = String.Empty;
        public UInt64 Timestamp = 0;
        public Byte Type        = 0;
    }

    /// <summary>
    /// BotDef object. Holds information on a bot type definition.
    /// </summary>
    public class BotDef
    {
        public String Name      = String.Empty;
        public String Author    = String.Empty;
        public String Language  = String.Empty;
        public String Link      = String.Empty;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Bot type</param>
        /// <param name="author">Bot's author</param>
        /// <param name="language">Language the bot is programmed in</param>
        /// <param name="link">Link to the bot's wiki page</param>
        public BotDef (String name, String author, String language, String link)
        {
            Name = name;
            Author = author;
            Language = language;
            Link = link;
        }
    }

    public class KickInfo
    {
        private List<ulong> times = new List<ulong>();
        public ulong MaxLife = 5;

        public int Count
        {
            get
            {
                return times.Count;
            }
        }

        public void Kick ()
        {
            var t = Bot.EpochTimestamp;
            if (!times.Contains(t))
                times.Add(t);

            foreach (var k in times.ToArray())
            {
                if (t - k > MaxLife)
                    times.Remove(k);
            }
        }
    }

    /// <summary>
    /// BotInfo object. Holds information on a specific bot.
    /// </summary>
    public class BotInfo
    {
        public String Name          = String.Empty;
        public String Owner         = String.Empty;
        public String Type          = String.Empty;
        public String Version       = String.Empty;
        public String Trigger       = String.Empty;
        public double BDSVersion    = 0.0;
        public ulong Modified       = 0;
        public bool Online          = false; // Online at the time of the last update?

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Bot username</param>
        /// <param name="owner">Bot owner's username</param>
        /// <param name="type">Bot type</param>
        /// <param name="version">Bot version</param>
        /// <param name="trigger">Bot's trigger</param>
        /// <param name="bdsver">Bot's BDS version</param>
        /// <param name="lastmod">Timestamp of last modification</param>
        public BotInfo (String name, String owner, String type, String version, String trigger, double bdsver, ulong lastmod)
        {
            Name = name;
            Owner = owner;
            Type = type;
            Version = version;
            Trigger = trigger;
            BDSVersion = bdsver;
            Modified = lastmod;
        }
    }

    /// <summary>
    /// ClientInfo object. Holds information on a specific client.
    /// </summary>
    public class ClientInfo
    {
        public String Name       = String.Empty;
        public String Type       = String.Empty;
        public String Version    = String.Empty;
        public Double BDSVersion = 0.3;
        public ulong Modified    = 0;
        public bool Online       = false; // Online at the time of the last update?

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Client username</param>
        /// <param name="type">Client type</param>
        /// <param name="version">Client version</param>
        /// <param name="lastmod">Timestamp of last modification</param>
        public ClientInfo (String name, String type, String version, Double bdsver, ulong lastmod)
        {
            Name = name;
            Type = type;
            Version = version;
            Modified = lastmod;
            BDSVersion = bdsver;
        }
    }
    #endregion BDS relater

    #region Channel related
    /// <summary>
    /// Privclass object. Stores information on a privclass of a channel.
    /// </summary>
    public class Privclass
    {
        public String Name  = String.Empty;
        public byte Order   = 0;
    }

    /// <summary>
    /// ChatMember object. Stores information on a member of a channel.
    /// </summary>
    public class ChatMember
    {
        public String Name      = String.Empty;
        public String Symbol    = String.Empty;
        public String RealName  = String.Empty;
        //public String TypeName  = String.Empty;
        public String GPC       = String.Empty;
        public String Privclass = String.Empty;
        public int ConnectionCount = 0;
    }

    /// <summary>
    /// ChatData object. Stores information on a chatroom.
    /// </summary>
    public class ChatData
    {
        public String Name  = String.Empty;
        public String Title = String.Empty;
        public String Topic = String.Empty;

        public Dictionary<String, Privclass> Privclasses    = new Dictionary<String, Privclass>();
        public Dictionary<String, ChatMember> Members       = new Dictionary<String, ChatMember>();
    }
    #endregion Channel related

    public enum SeenType : byte
    {
        None    = 0,
        Joining = 1,
        Parting = 2,
        Talking = 3,
        Kicked  = 4,
    }

    public enum NamespaceFormat : byte
    {
        Channel = 1,
        Username = 2,
        Packet = 3,
        PrivateChat = 4,
    }

    public enum ByteCounts : ulong
    {
        /*YottaByte = (1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024),
        ZettaByte = (1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024),
        ExaByte   = (1024 * 1024 * 1024 * 1024 * 1024 * 1024),
        PetaByte  = (1024 * 1024 * 1024 * 1024 * 1024),
        TeraByte  = (1024 * 1024 * 1024 * 1024),*/
        GigaByte = (1024 * 1024 * 1024),
        MegaByte = (1024 * 1024),
        KiloByte = 1024,
    }

    public class WhoisConnection
    {
        public int ConnectionID = 0;
        public ulong Online = 0, Idle = 0;
        public List<String> Channels = new List<String>();
    }

    public class WhoisData
    {
        public String Name      = String.Empty;
        public String Symbol    = String.Empty;
        public String RealName  = String.Empty;
        //public String TypeName  = String.Empty;
        public String GPC       = String.Empty;
        public List<WhoisConnection> Connections = new List<WhoisConnection>();
    }

    #region strftime

    public enum DayOfWeek : int
    {
        Sunday = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 3,
        Thursday = 4,
        Friday = 5,
        Saturday = 6
    }

    #endregion strftime

    #region Events

    /// <summary>
    /// Event object
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Class's "this" object
        /// </summary>
        public object Class;

        /// <summary>
        /// Class's name.
        /// </summary>
        public String ClassName;

        /// <summary>
        /// Info on the method we will call
        /// </summary>
        public MethodInfo Method;

        /// <summary>
        /// Description of the event
        /// </summary>
        public String Description = String.Empty;

        /// <summary>
        /// Information on the parent extension.
        /// </summary>
        public ExtensionInfo Extension = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="class_obj">Class pointer (i.e. "this")</param>
        /// <param name="method_name">Callback method name</param>
        /// <param name="desc">Description</param>
        public Event (object class_obj, String method_name, String desc = "", String class_name = null, ExtensionInfo ext = null)
        {
            Class = class_obj;
            Method = Class.GetType().GetMethod(method_name);
            Description = desc;
            if (class_name == null)
                ClassName = Class.ToString();
            else
                ClassName = class_name;
            Extension = ext;
        }
    }


    /// <summary>
    /// Command object
    /// </summary>
    public class Command
    {
        /// <summary>
        /// Class's "this" object
        /// </summary>
        public object Class;

        /// <summary>
        /// Information on the method we will call.
        /// </summary>
        public MethodInfo Method;

        /// <summary>
        /// Description of the command.
        /// </summary>
        public String Description = String.Empty;

        /// <summary>
        /// Author's dA username
        /// </summary>
        public String Author = String.Empty;

        /// <summary>
        /// Minimum privilege level. Default: 25 (Owner: 100, Admins: 99, Operators: 75, Members: 50, Guests: 25)
        /// </summary>
        public int MinimumPrivs = 25;

        /// <summary>
        /// Information on the parent extension.
        /// </summary>
        public ExtensionInfo Extension = null;

        /// <summary>
        /// Help/usage for the command.
        /// </summary>
        public String Help = String.Empty;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="class_obj">Class pointer (i.e. "this")</param>
        /// <param name="method_name">Callback method name</param>
        /// <param name="desc">Description</param>
        public Command (object class_obj, String method_name,
            String author = "", int privs = 25, String desc = "", String help = "", ExtensionInfo ext = null)
        {
            Class = class_obj;
            Method = Class.GetType().GetMethod(method_name);
            Author = author;
            Description = desc;
            MinimumPrivs = privs;
            Help = help.Length == 0 ? "No help available." : help;
            Extension = ext;

            if (String.IsNullOrWhiteSpace(Help))
                Help = "No help available.";
        }
    }
    #endregion Events
}

namespace lulzbot.Extensions
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
    public class ExtensionInfo : System.Attribute
    {
        public String Name      = String.Empty;
        public String Author    = String.Empty;
        public String Version   = "1.0";
        //desc?

        public ExtensionInfo (String name, String author, String ver)
        {
            Name = name;
            Author = author;
            Version = ver;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)]
    public class BindCommand : System.Attribute
    {
        public String Command, Description = String.Empty, Usage = String.Empty;
        public int Privileges;
        public BindCommand (String cmd, String desc, Privs privs, String usage = "")
        {
            Command = cmd;
            Description = desc;
            Privileges = (int)privs;
            Usage = usage;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)]
    public class BindEvent : System.Attribute
    {
        public String Event, Description = String.Empty;
        public BindEvent (String evt, String desc = "")
        {
            Event = evt;
            Description = desc;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct UserData
    {
        public String Name;
        public int PrivLevel;
        public List<String> Access;
        public List<String> Banned;
    }

    public enum Privs : int
    {
        Banned = 0,
        Guest = 25,
        Members = 50,
        Operators = 75,
        Admins = 99,
        Owner = 100
    }
}