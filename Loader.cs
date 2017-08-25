namespace SMHackMiniLoader {
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.IO;
    using SMHackCore;

    public class Loader : ServerInterface {
        private readonly BlockingCollection<int> _pids = new BlockingCollection<int>();

        private Loader(string s) {
            PluginConfig = new PluginConfig(s);
            PluginConfigDirectory = Path.GetDirectoryName(s);
        }

        public override PluginConfig PluginConfig { get; }
        public override string PluginConfigDirectory { get; }

        public override void Connect(int id, string image) { }

        public override void DoLog(params ClientLogPacket[] packet) { }

        public override void DoInject(int id) {
            _pids.Add(id);
            base.DoInject(id);
        }

        public static void Main(string[] args) {
            if (args.Length < 2)
                return;
            var loader = new Loader(args[0]);
            loader.Run();
        }

        private void Run() {
            try {
                var args = GetRest(Environment.CommandLine, 2);
                if (GetCommandLine(args, out var name, out var restargs))
                    DoCreateAndInject(name, restargs);
                else
                    throw new ArgumentException(args);
                while (_pids.TryTake(out var pid)) {
                    var process = Process.GetProcessById(pid);
                    process.WaitForExit();
                }
            } catch (Exception ex) {
                Console.WriteLine(ex);
                // ignored
            }
        }
    }
}