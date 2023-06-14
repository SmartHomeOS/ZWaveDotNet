using Serilog;
using System.Reflection;
using ZWaveDotNet.CommandClasses;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;

namespace ExampleConsole
{
    public static class CCParser
    {
        public static void Generate()
        {
            int total = 0, full = 0, partial = 0;
            Dictionary<CommandClass, CCVersion> attrs = new Dictionary<CommandClass, CCVersion>();
            foreach (Type t in GetTypesWithHelpAttribute(Assembly.GetAssembly(typeof(CCVersion))!))
            { 
                CCVersion? cc = (CCVersion?)t.GetCustomAttribute(typeof(CCVersion));
                if (cc != null && t != typeof(Notification))
                    attrs.Add(cc.commandClass, cc);
                else if (cc != null)
                    attrs.Add(CommandClass.Mark, cc);
            }

            try
            {
                File.Delete("status.md");
            }
            catch (Exception) { }
            FileStream fo = File.OpenWrite("status.md");
            StreamWriter fw = new StreamWriter(fo);
            FileStream fs = File.OpenRead("CCs.csv");
            StreamReader sr = new StreamReader(fs);
            string? line = null;
            fw.WriteLine("Command Class | Max Supported Version | Max Spec Version | Support");
            fw.WriteLine("--------------|-------------------|---------------|---------");
            do
            {
                line = sr.ReadLine();
                if (line != null)
                {
                    string[] parts = line.Split(',', StringSplitOptions.TrimEntries);
                    if (parts.Length != 3)
                        throw new Exception(line);
                    CommandClass cc = (CommandClass)Convert.ToUInt16(parts[1], 16);
                    if (cc != CommandClass.SecurityMark && cc != CommandClass.Mark)
                    {
                        total++;
                        if (attrs.ContainsKey(cc))
                        {
                            CCVersion ccv = attrs[cc];
                            if (cc == CommandClass.Alarm && parts[2] != "2")
                                ccv = attrs[CommandClass.Mark]; //Hack for notification cc
                            bool complete = ccv.complete && int.TryParse(parts[2], out int result) && ccv.maxVersion == result;
                            if (complete)
                                fw.Write("**");
                            else
                                fw.Write("*");
                            fw.Write(parts[0].Replace("COMMAND_CLASS_", ""));
                            if (complete)
                                fw.Write("** | **");
                            else
                                fw.Write("* | *");
                            fw.Write(ccv.maxVersion.ToString());
                            if (complete)
                                fw.Write("** | **");
                            else
                                fw.Write("* | *");
                            fw.Write(parts[2]);
                            if (complete)
                                fw.Write("** | **");
                            else
                                fw.Write("* | *");
                            if (ccv.complete)
                            {
                                fw.Write("Full*");
                                if (complete)
                                    fw.Write('*');
                                fw.WriteLine();
                                full++;
                            }
                            else
                            {
                                fw.WriteLine("Partial*");
                                partial++;
                            }
                        }
                        else
                        {
                            fw.Write(parts[0].Replace("COMMAND_CLASS_", "") + " | ");
                            fw.Write("0 | ");
                            fw.WriteLine(parts[2] + " | None");
                        }
                    }
                }
            } while (line != null);
            fw.WriteLine();
            fw.WriteLine($"- Full Support for {full}/{total} Command Classes.");
            fw.Write($"- Partial Support for {partial}/{total} Command Classes.");
            fw.Close();
            Log.Information("Done");
        }

        static IEnumerable<Type> GetTypesWithHelpAttribute(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(CCVersion), true).Length > 0)
                {
                    yield return type;
                }
            }
        }
    }
}
