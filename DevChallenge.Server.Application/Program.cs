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
            Server.Program.LoadChallenges(new DirectoryInfo(Properties.Settings.Default.ScenarioPath));
            Server.Program.Run();
        }
    }
}
