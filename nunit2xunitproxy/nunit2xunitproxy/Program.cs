using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using nunit2xunitproxy.nunitTester;
using NLog;

namespace nunit2xunitproxy {
    class Program {
        static NLog.ILogger log = LogManager.GetCurrentClassLogger();
        static void Main(string[] args) {
            var result = Parser.Default.ParseArguments<CommandLineOptions>(args);
            var exitCode = result.MapResult(clo => {
                return DoWork(clo);
            },
            errors => 1);
            Environment.Exit(exitCode);
        }
        static int DoWork(CommandLineOptions clo) {
            var nunitTestProject = NunitHelper.LoadProject(clo.Project);
            if (nunitTestProject == null || nunitTestProject.Configs.Count == 0) {
                log.Error("Project not found or empty");
                return 1;
            }

            return 0;
        }
    }
}
