using lulzbot.Networking;
using lulzbot.Types;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace lulzbot.Extensions
{
    public class ExtensionContainer
    {
        public static String CurrentFile = String.Empty;
        public static List<ExtensionInfo> Extensions = new List<ExtensionInfo>();

        public ExtensionContainer ()
        {
            Load();
        }

        public void Load ()
        {
            if (!Directory.Exists("./Extensions/Enabled"))
            {
                // If it doesn't exist, there's no extensions. Create and leave.
                Directory.CreateDirectory("./Extensions/Enabled");
                return;
            }

            Extensions.Clear();

            Events.ClearExternalEvents();

            String[] files = Directory.GetFiles("./Extensions/Enabled", "*.cs");

            foreach (String file in files)
            {
                LoadFile(file);
            }
        }

        private void LoadFile (String filename)
        {
            CurrentFile = Path.GetFileName(filename);

            CodeDomProvider codeDomProvider         = CSharpCodeProvider.CreateProvider("C#");
            CompilerParameters compilerParams       = new CompilerParameters();
            compilerParams.GenerateExecutable       = false;
            compilerParams.GenerateInMemory         = true;
            compilerParams.IncludeDebugInformation  = false;

            compilerParams.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
            compilerParams.ReferencedAssemblies.Add("System.dll");
            compilerParams.ReferencedAssemblies.Add("System.Core.dll");
            compilerParams.ReferencedAssemblies.Add("System.Net.dll");
            compilerParams.ReferencedAssemblies.Add("System.Data.dll");
            compilerParams.ReferencedAssemblies.Add("System.Xml.dll");
            compilerParams.ReferencedAssemblies.Add("Newtonsoft.Json.dll");
            compilerParams.ReferencedAssemblies.Add("SRCDSQuery.dll");
            compilerParams.ReferencedAssemblies.Add("MCQuery.dll");
            compilerParams.ReferencedAssemblies.Add("mysql.data.dll");

            CompilerResults results = codeDomProvider.CompileAssemblyFromFile(compilerParams, filename);

            if (results.Errors.Count > 0)
            {
                foreach (CompilerError error in results.Errors)
                {
                    ConIO.Warning("Extension.LoadFile[Compilation error]", error.ToString());
                }
            }
            else
            {
                try
                {
                    var compiled_type = results.CompiledAssembly.GetType("Extension");
                    System.Attribute[] attrs = System.Attribute.GetCustomAttributes(compiled_type);
                    ExtensionInfo ext_info = null;
                    if (attrs.Length == 0 || (ext_info = attrs[0] as ExtensionInfo) == null)
                    {
                        ConIO.Warning("Extensions", "No valid ExtensionInfo attribute: " + filename);
                    }
                    else
                    {
                        ConIO.Write(String.Format("Loaded extension: {0} v{1} by {2}.", ext_info.Name, ext_info.Version, ext_info.Author));
                        Extensions.Add(ext_info);
                        object class_instance = Activator.CreateInstance(compiled_type);

                        foreach (MethodInfo method in compiled_type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                        {
                            if (Attribute.IsDefined(method, typeof(BindCommand)))
                            {
                                object[] m_attrs = method.GetCustomAttributes(true);
                                foreach (object potential in m_attrs)
                                {
                                    BindCommand cmd_info = potential as BindCommand;
                                    if (cmd_info != null)
                                    {
                                        Events.AddExternalCommand(cmd_info.Command, new Command(class_instance, method.Name, ext_info.Author, cmd_info.Privileges, cmd_info.Description, cmd_info.Usage, ext_info));
                                    }
                                }
                            }
                            else if (Attribute.IsDefined(method, typeof(BindEvent)))
                            {

                                object[] m_attrs = method.GetCustomAttributes(true);
                                foreach (object potential in m_attrs)
                                {
                                    BindEvent evt_info = potential as BindEvent;
                                    if (evt_info != null)
                                    {
                                        Events.AddExternalEvent(evt_info.Event, new Event(class_instance, method.Name, evt_info.Description, ext_info.Name, ext_info));
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception E)
                {
                    ConIO.Warning("Extension.LoadFile[Execution error]", E.ToString());
                }
            }

            CurrentFile = String.Empty;
        }
    }

    /// <summary>
    /// Static class for external extensions. Gives an easy way to do most things with a friendlier interface
    /// </summary>
    public class LulzBot
    {
        public static String Trigger = Program.Bot.Config.Trigger;
        public static String Username = Program.Bot.Config.Username;
        public static String Owner = Program.Bot.Config.Owner;

        public static void Print (String msg)
        {
            ConIO.Write(msg);
        }

        public static void Say (String chan, String msg)
        {
            var c = Tools.FormatNamespace(chan, NamespaceFormat.Channel).ToLower();
            if (
                Program.OfficialChannels.Contains(c) ||
                Program.NoDisplay.Contains(c)) return;

            Program.Bot.Say(chan, msg);
        }

        public static void NPSay (String chan, String msg)
        {
            var c = Tools.FormatNamespace(chan, NamespaceFormat.Channel).ToLower();
            if (
                Program.OfficialChannels.Contains(c) ||
                Program.NoDisplay.Contains(c)) return;

            Program.Bot.NPSay(chan, msg);
        }

        public static void Act (String chan, String msg)
        {
            var c = Tools.FormatNamespace(chan, NamespaceFormat.Channel).ToLower();
            if (
                Program.OfficialChannels.Contains(c) ||
                Program.NoDisplay.Contains(c)) return;

            Program.Bot.Act(chan, msg);
        }

        public static void Join (String chan)
        {
            Program.Bot.Join(chan);
        }

        public static void Part (String chan)
        {
            Program.Bot.Part(chan);
        }

        public static void Kick (String chan, String who, String why = null)
        {
            Program.Bot.Kick(chan, who, why);
        }

        public static void Promote (String chan, String who, String privclass = null)
        {
            Program.Bot.Promote(chan, who, privclass);
        }

        public static void Demote (String chan, String who, String privclass = null)
        {
            Program.Bot.Promote(chan, who, privclass);
        }

        public static void Ban (String chan, String who)
        {
            Program.Bot.Ban(chan, who);
        }

        public static void UnBan (String chan, String who)
        {
            Program.Bot.UnBan(chan, who);
        }

        public static void Admin (String chan, String cmd)
        {
            Program.Bot.Admin(chan, cmd);
        }

        public static void Topic (String chan, String content)
        {
            Program.Bot.Send(dAmnPackets.Set(chan, "topic", content));
        }

        public static void Title (String chan, String content)
        {
            Program.Bot.Send(dAmnPackets.Set(chan, "title", content));
        }

        public static void Save (String filename, object data)
        {
            Storage.Save(filename, data);
        }

        public static T Load<T> (String filename)
        {
            T data = Storage.Load<T>(filename);
            return data;
        }
    }
}
