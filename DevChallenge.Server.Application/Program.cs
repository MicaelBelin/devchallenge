using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevChallenge.Server.Application
{
    class Program
    {
        static void Main(string[] args)
        {
            var program = new Server.Program(new Model.EventLog.StdOut());
            program.LoadChallenges(new DirectoryInfo(Properties.Settings.Default.ScenarioPath));
            program.Run();
        }
    }
}
