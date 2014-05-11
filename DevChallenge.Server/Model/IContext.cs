using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevChallenge.Server.Model
{
    public interface IContext
    {
        IScenarioManager ScenarioManager { get; }
        IEnumerable<IAgentEntryPoint> EntryPoints { get; }
    }
}
