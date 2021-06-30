using Newtonsoft.Json;
using System;
using System.IO;
using System.Xml.Serialization;

using JxCode.Common;


namespace StarUMLParser
{

    public class Program
    {
        public static int kOk = 0;
        public static int kArgError = 1;
        public static int kExecuteError = 2;

        public const string info =
            "-i path : input file path\n" +
            "-o path : output file path";

        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(info);
                return kArgError;
            }
            var argb = new ArgumentParserBuilder();
            argb.AddConfig(new ArgumentConfigMessage("-help"))
                .AddConfig(ArgumentConfigFile.InputOnce(ext: ".xmi"))
                .AddConfig(ArgumentConfigFile.OutputOnce(ext: ".json"));

            ArgumentParser pargs = null;
            try
            {
                pargs = argb.Build(args);
            }
            catch (ArgumentParserException e)
            {
                Console.WriteLine(e.ToString());
                return kArgError;
            }
            
            if (pargs.HasCmd("-help"))
            {
                Console.WriteLine(info);
            }

            string inPath = pargs.GetCmd("-i").GetFirstArg();
            string outPath = pargs.GetCmd("-o").GetFirstArg();

            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(XmiDocument));

                XmiDocument xmi = null;
                using (var fs = File.OpenRead(inPath))
                {
                    xmi = (XmiDocument)ser.Deserialize(fs);
                }

                UmlNodesData data = UmlNodesParser.Parse(xmi);

                var jsonStr = JsonConvert.SerializeObject(data);

                File.WriteAllText(outPath, jsonStr);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return kExecuteError;
            }

            Console.WriteLine("complete!");
            return kOk;
        }
    }
}
