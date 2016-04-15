using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading;
using CommandLine;
using Ionic.Zip;
using NLog;
using NUnit.Util;

namespace nunit2xunitproxy {
    class Program {
        static string xunitfiles = "nunit2xunitproxy.tools.zip";
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        static void Main(string[] args) {
            var result = Parser.Default.ParseArguments<CommandLineOptions>(args);
            var exitCode = result.MapResult(clo => {
                try {
                    return DoWork(clo);
                }
                catch (Exception ex) {
                    Log.Error(ex, "exception thrown");
                    return 1;
                }
            },
            errors => 1);
            Environment.Exit(exitCode);
        }
        static int DoWork(CommandLineOptions clo) {
            var nunitTestProject = NunitHelper.LoadProject(clo.Project);
            if (nunitTestProject == null || nunitTestProject.Configs.Count == 0) {
                Log.Error("Project not found or empty");
                return 1;
            }

            ExtractXUnut();

            ProjectConfig config = nunitTestProject.ActiveConfig;
            return ExecuteTests(config.Assemblies);
        }

        static int ExecuteTests(AssemblyList assemblies) {
            try {
                List<string> parameters = new List<string>(assemblies.ToArray());
                parameters.Add("-nunit");
                parameters.Add("TestResult.xml");
                //parameters.Add("-nologo");
                parameters.Add("-noappdomain");
                parameters.Add("-paralleloption");
                parameters.Add("none");
                parameters.Add("-noshadow");
                ProcessStartInfo info = new ProcessStartInfo("xunit.console.exe");
                info.RedirectStandardOutput = true;
                info.UseShellExecute = false;
                info.Arguments = parameters.Aggregate(new StringBuilder(), (builder, s) => builder.Append($"{s} ")).ToString();
                var process = Process.Start(info);
                while (!process.HasExited) {
                    Console.Write(process.StandardOutput.ReadToEnd());
                    Thread.Sleep(100);
                }
                return process.ExitCode;
            }
            catch (Exception ex) {
                Log.Info("xunit domain thrown exception");
                Log.Error(ex);
                return 1;
            }
        }
        static void ExtractXUnut() {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(xunitfiles)) {
                var file = ZipFile.Read(stream);
                file.ExtractAll(Path.GetDirectoryName(assembly.Location), ExtractExistingFileAction.DoNotOverwrite);
            }
        }
    }
}
