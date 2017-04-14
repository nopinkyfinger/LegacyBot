using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_eval (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (args.Length < 2)
            {
                bot.Say(ns, String.Format("<b>&raquo; Usage:</b> {0}eval <i>[--using Some.Namespace,Another.Namespace]</i> code", bot.Config.Trigger));
            }
            else
            {
                try
                {
                    String usercode = msg.Substring(5).Replace("&amp;", "&");
                    List<String> usings = new List<String>(); ;
                    String nusings = "";

                    if (args[1] == "--using")
                    {
                        if (args[2].Contains(','))
                        {
                            usings = new List<String>(args[2].Split(new char[] { ',' }));
                        }
                        else usings.Add(args[2]);
                        usercode = usercode.Substring(9 + args[2].Length);
                    }
                    else if (args[1] == "--show")
                    {
                        usercode = usercode.Substring(7);
                    }

                    if (usercode.StartsWith("<bcode>"))
                    {
                        usercode = usercode.Substring(7).Replace("</bcode>", "").Replace("\n", "\n\t\t");
                    }

                    usings.Add("System");
                    usings.Add("System.Net");
                    usings.Add("System.Linq");
                    usings.Add("System.Linq.Expressions");
                    usings.Add("System.Collections");
                    usings.Add("System.Collections.Generic");
                    usings.Add("lulzbot");
                    usings.Add("lulzbot.Extensions");
                    usings.Add("SRCDSQuery");
                    usings.Add("MCQuery");
                    usings.Add("MySql.Data");

                    nusings = "using " + String.Join(";\nusing ", usings) + ";\n";

                    String code = nusings + "\n" +
                        "public class c_eval {\n\t#pragma warning disable 162\n\t" +
                        "#pragma warning disable 219\n\t" +
                        "public static object v_eval () {\n\t\t" +
                        "String ns = \"" + ns + "\", from = \"" + from + "\";\n\t\t" +
                        "Bot bot = Program.Bot, self = Program.Bot;\n\t\t" +
                        usercode + "\n\t\treturn null;\n\t}\n}";

                    if (args[1] == "--show")
                    {
                        bot.Say(ns, "<b>&raquo; Generated code:</b><br/><bcode>" + code + "</bcode>");
                        return;
                    }

                    CodeDomProvider codeDomProvider = CSharpCodeProvider.CreateProvider("C#");
                    CompilerParameters compilerParams = new CompilerParameters();
                    compilerParams.ReferencedAssemblies.Add("System.dll");
                    compilerParams.ReferencedAssemblies.Add("System.Core.dll");
                    compilerParams.ReferencedAssemblies.Add("System.Net.dll");
                    compilerParams.ReferencedAssemblies.Add("System.Data.dll");
                    compilerParams.ReferencedAssemblies.Add("System.Xml.dll");
                    compilerParams.ReferencedAssemblies.Add("Newtonsoft.Json.dll");
                    compilerParams.ReferencedAssemblies.Add("SRCDSQuery.dll");
                    compilerParams.ReferencedAssemblies.Add("MCQuery.dll");
                    compilerParams.ReferencedAssemblies.Add("mysql.data.dll");
                    compilerParams.GenerateExecutable = false;
                    compilerParams.GenerateInMemory = true;
                    compilerParams.IncludeDebugInformation = false;
                    compilerParams.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);

                    CompilerResults results = codeDomProvider.CompileAssemblyFromSource(compilerParams, code);

                    if (results.Errors.Count > 0)
                    {
                        String output = String.Format("<b>&raquo; Evaluation of code failed. {0} error{1}:</b>", results.Errors.Count, results.Errors.Count == 1 ? "" : "s");
                        foreach (CompilerError error in results.Errors)
                        {
                            output += String.Format("<br/><b> &middot; Error {0}:</b> {1}", error.ErrorNumber, error.ErrorText);
                        }
                        bot.Say(ns, output);
                    }
                    else
                    {
                        try
                        {
                            var compiled_type = results.CompiledAssembly.GetType("c_eval");
                            var method = compiled_type.GetMethod("v_eval");
                            object res = method.Invoke(null, null);
                            if (res != null)
                                bot.Say(ns, String.Format("<b>&raquo; Output:</b><bcode>{0}</bcode>", res.ToString().Length <= 0 ? "Code returned NULL" : res.ToString()));
                            else
                                bot.Say(ns, "<b>&raquo; Code returned <i>NULL/NIL</i>");
                        }
                        catch (Exception E)
                        {
                            bot.Say(ns, "<b>&raquo; Error while executing code:</b> " + E.ToString());
                        }
                    }
                }
                catch (Exception E)
                {
                    bot.Say(ns, "<b>&raquo; Unable to evaluate code:</b> " + E.ToString());
                }
            }
        }
    }
}

