using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text;

namespace JxCode.Common
{
    public class ArgumentCommand : IEnumerable<string>
    {
        public string CmdCode { get; private set; }
        public List<string> Args { get; private set; }
        public int ArgCount { get => Args.Count; }

        public ArgumentCommand(string cmdCode, List<string> args)
        {
            this.CmdCode = cmdCode;
            this.Args = args;
        }
        public string GetFirstArg()
        {
            return Args[0];
        }
        public IEnumerator<string> GetEnumerator()
        {
            return Args.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Args.GetEnumerator();
        }
    }

    public class CheckInvalidResult
    {
        public bool IsSuccess { get; set; }
        public string Infomation { get; set; }

        public CheckInvalidResult(bool isSuccess, string infomation)
        {
            this.IsSuccess = isSuccess;
            this.Infomation = infomation;
        }
    }

    public class ArgumentConfig
    {
        public string CmdCode { get; set; }
        public int ArgCount { get; set; }
        public bool IsRequired { get; set; }
        public Func<List<string>, CheckInvalidResult> CheckInvalidHandler;

        public ArgumentConfig(
            string record,
            int argCount,
            bool isRequired,
            Func<List<string>, CheckInvalidResult> checkInvalidHandler)
        {
            this.CmdCode = record;
            this.ArgCount = argCount;
            this.IsRequired = isRequired;
            this.CheckInvalidHandler = checkInvalidHandler;
        }
        public override string ToString()
        {
            return this.CmdCode;
        }
    }
    public class ArgumentConfigMessage : ArgumentConfig
    {
        public ArgumentConfigMessage(string cmdCode = "-help")
            : base(cmdCode, 0, false, null)
        {
        }
    }
    public class ArgumentConfigFile : ArgumentConfig
    {
        private bool hasExists;
        private string[] extensions;
        private bool HasExt(string filename)
        {
            string ext = Path.GetExtension(filename);
            return Array.IndexOf(extensions, ext) != -1;
        }
        private CheckInvalidResult CheckFileInvalid(List<string> str)
        {
            foreach (var filename in str)
            {
                if (hasExists && !File.Exists(filename))
                {
                    return new CheckInvalidResult(false, "not exists: " + filename);
                }
                if (!HasExt(filename))
                {
                    return new CheckInvalidResult(false, "format error: " + filename);
                }
            }
            return new CheckInvalidResult(true, null);
        }
        public ArgumentConfigFile(
            string cmdCode,
            int argCount = 1,
            bool isRequired = true,
            bool hasExists = true,
            string[] extensions = null
            ) : base(cmdCode, argCount, isRequired, null)
        {
            this.hasExists = hasExists;
            this.extensions = extensions;
            base.CheckInvalidHandler = this.CheckFileInvalid;
        }

        public static ArgumentConfigFile InputOnce(string cmdCode = "-i", string ext = null)
        {
            string[] extensions = ext == null ? null : new string[] { ext };
            return new ArgumentConfigFile(cmdCode, 1, true, true, extensions);
        }
        public static ArgumentConfigFile OutputOnce(string cmdCode = "-o", string ext = null)
        {
            string[] extensions = ext == null ? null : new string[] { ext };
            return new ArgumentConfigFile(cmdCode, 1, true, false, extensions);
        }
    }
    public class ArgumentConfigDirectory : ArgumentConfig
    {
        private static CheckInvalidResult CheckInvalidDir(List<string> str)
        {
            return default;
        }
        public ArgumentConfigDirectory(
            string cmdCode,
            int argCount = 1,
            bool isRequired = true
            ) : base(cmdCode, argCount, isRequired, CheckInvalidDir)
        {

        }
    }

    public class ArgumentParserException : ApplicationException
    {
        public ArgumentParserException(string msg) : base(msg) { }
    }
    public class ArgumentParserNotFindCommandException : ArgumentParserException
    {
        public ArgumentParserNotFindCommandException(string msg) : base(msg) { }
    }
    public class ArgumentParserInvalidArgumentException : ArgumentParserException
    {
        public ArgumentParserInvalidArgumentException(string msg) : base(msg) { }
    }

    public class ArgumentParserBuilder
    {
        private Dictionary<string, ArgumentConfig> cfgs;
        public ArgumentParserBuilder()
        {
            cfgs = new Dictionary<string, ArgumentConfig>();
        }
        public ArgumentParserBuilder AddConfig(ArgumentConfig cfg)
        {
            cfgs.Add(cfg.CmdCode, cfg);
            return this;
        }

        private List<string> GetRequired()
        {
            List<string> list = new List<string>();
            foreach (var item in cfgs)
            {
                if (item.Value.IsRequired)
                {
                    list.Add(item.Value.CmdCode);
                }
            }
            return list;
        }

        public ArgumentParser Build(string[] args)
        {
            //检查必要参数
            var required = GetRequired();
            if ((args == null || args.Length == 0) && required.Count != 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in required)
                {
                    sb.Append(item);
                    sb.Append(", ");
                }
                throw new ArgumentParserInvalidArgumentException("[error] missing cmd: " + sb.ToString());
            }

            ArgumentParser cmd = new ArgumentParser();
            if (args == null || args.Length == 0)
            {
                return cmd;
            }

            //对参数循环
            var cmdCode = new Dictionary<string, ArgumentCommand>();
            ArgumentCommand curCmd = null;
            for (int i = 0; i < args.Length; i++)
            {
                string item = args[i];
                if (!cmdCode.ContainsKey(item) && cfgs.ContainsKey(item))
                {
                    curCmd = new ArgumentCommand(item, new List<string>());
                    cmdCode.Add(item, curCmd);
                    continue;
                }
                else if (cmdCode.ContainsKey(item) && cfgs.ContainsKey(item))
                {
                    throw new ArgumentParserInvalidArgumentException("[error] repeat command: " + item);
                }
                if (curCmd == null)
                {
                    throw new ArgumentParserNotFindCommandException("[error] not find command: " + item);
                }
                curCmd.Args.Add(item);
            }

            //检查数据有效性
            foreach (KeyValuePair<string, ArgumentConfig> item in cfgs)
            {
                string recordName = item.Key;
                ArgumentConfig cfg = item.Value;

                //必须存在但是没存在
                if (cfg.IsRequired && !cmdCode.ContainsKey(recordName))
                {
                    throw new ArgumentParserNotFindCommandException("[error] not find command: " + cfg.CmdCode);
                }
                //已存在
                if (cmdCode.ContainsKey(recordName))
                {
                    //无参数的指令却给了参数
                    if (cfg.ArgCount == 0 && cmdCode[recordName].ArgCount != 0)
                    {
                        throw new ArgumentParserInvalidArgumentException("[error] invalid argument count: " + cfg.CmdCode);
                    }
                    //有限的参数但长度不一
                    if (cfg.ArgCount > 0 && cmdCode[recordName].ArgCount != cfg.ArgCount)
                    {
                        throw new ArgumentParserInvalidArgumentException("[error] missing arguments: " + cfg.CmdCode);
                    }

                    var result = cfg?.CheckInvalidHandler(cmdCode[recordName].Args);
                    if (result != null && !result.IsSuccess)
                    {
                        throw new ArgumentParserInvalidArgumentException("[error] invalid arguments: " + result.Infomation);
                    }
                }

            }

            cmd.CmdCode = cmdCode;

            return cmd;
        }
    }

    public class ArgumentParser
    {
        public Dictionary<string, ArgumentCommand> CmdCode;

        public bool HasCmd(string record)
        {
            return CmdCode.ContainsKey(record);
        }
        public ArgumentCommand GetCmd(string cmdCode)
        {
            ArgumentCommand rec = null;
            CmdCode.TryGetValue(cmdCode, out rec);
            return rec;
        }
        public List<string> GetCmdArgs(string cmdCode)
        {
            ArgumentCommand rec = GetCmd(cmdCode);
            if (rec == null)
            {
                return null;
            }
            return rec.Args;
        }

    }
}
