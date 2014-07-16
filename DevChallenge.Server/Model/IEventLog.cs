using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevChallenge.Server.Model
{
    public interface IEventLog
    {
        void Add(Model.EventLogType type,string msg);
    }

    public enum EventLogType
    {
        Message,
        Error,
    }


}
