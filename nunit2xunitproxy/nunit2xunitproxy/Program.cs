using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CommandLine;
using Ionic.Zip;
using nunit2xunitproxy.nunitTester;
using NLog;
using NUnit.Util;

namespace nunit2xunitproxy {
    class Program {
        private static string xunitfiles = "nunit2xunitproxy.tools.zip";
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

            ExtractXUnut();

            ProjectConfig config = nunitTestProject.ActiveConfig;
            return ExecuteTests(config.Assemblies);
        }
        private static int ExecuteTests(AssemblyList assemblies) {
            StringBuilder accumulator = new StringBuilder();
            var assembliesParameter = assemblies.Cast<string>().Aggregate(accumulator, (builder, s) => builder.Append($"{s} ")).ToString();
            var process = Process.Start("xunit.console.exe", $@"{assembliesParameter} -nunit TestResult.xml -nologo -noappdomain -paralleloption none -noshadow");
            process.WaitForExit();
            return process.ExitCode;
        }
        static void ExtractXUnut() {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(xunitfiles)) {
                var file = ZipFile.Read(stream);
                file.ExtractAll(Path.GetDirectoryName(assembly.Location), ExtractExistingFileAction.OverwriteSilently);
            }
        }
    }
}
