using Serilog;
using System.Reflection;
using ZWaveDotNet.CommandClasses;
using ZWaveDotNet.Enums;

namespace TestConsole
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
                if (cc != null)
                    attrs.Add(cc.commandClass, cc);
            }

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
                        fw.Write(parts[0].Replace("COMMAND_CLASS_","") + " | ");
                        if (attrs.ContainsKey(cc))
                        {
                            CCVersion ccv = attrs[cc];
                            fw.Write(ccv.maxVersion.ToString() + " | ");
                            fw.Write(parts[2] + " | ");
                            if (ccv.complete)
                            {
                                fw.WriteLine("Full");
                                full++;
                            }
                            else
                            {
                                fw.WriteLine("Partial");
                                partial++;
                            }
                        }
                        else
                        {
                            fw.Write("0 | ");
                            fw.WriteLine(parts[2] + " | None");
                        }
                    }
                }
            } while (line != null);
            fw.WriteLine($"\nFull Support for {full}/{total} Command Classes.\nPartial Support for {partial}/{total} Command Classes.");
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
