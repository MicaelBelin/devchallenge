using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevChallenge.Server.Model.EventLog
{
    public class Labeled : IEventLog
    {
        public IEventLog Target { get; set; }
        public string Label {get;set;}

        public Labeled(string label, IEventLog target)
        {
            this.Target = target;
            this.Label = label;
        }

        public void Add(EventLogType type, string msg)
        {
            Target.Add(type, String.Format("{0}: {1}",
                Label,
                msg));
        }

    }
}
