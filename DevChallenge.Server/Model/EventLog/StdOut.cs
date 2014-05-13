using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevChallenge.Server.Model.EventLog
{
    public class StdOut : Model.IEventLog
    {
        public void Add(Model.EventLogType type, string msg)
        {
            System.Console.WriteLine(String.Format("[{0}] {1}",type,msg));
        }
    }
}
