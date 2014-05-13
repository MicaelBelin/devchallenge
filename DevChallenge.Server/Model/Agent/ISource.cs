using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevChallenge.Server.Model.Agent
{
    public interface ISource
    {
        event Action<IAgent> AgentSpawned;
    }
}
