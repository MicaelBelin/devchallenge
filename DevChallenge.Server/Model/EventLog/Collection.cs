using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevChallenge.Server.Model.EventLog
{
    public class Collection : IEventLog
    {
        IEventLog[] Targets { get; set; }


        public void Add(EventLogType type, string msg)
        {
            if (Targets == null) return;
            foreach (var target in Targets)
            {
                target.Add(type, msg);
            }
        }
    }
}
