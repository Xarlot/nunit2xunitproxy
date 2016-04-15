using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace nunit2xunitproxy {
    public class CommandLineOptions {
        [Option('p', "project", HelpText = "NUnit test project or dll", Required = true)]
        public string Project { get; set; }
        [Option('r', "result", HelpText = "NUnit test result file name")]
        public string TestResult { get; set; }
    }
}
