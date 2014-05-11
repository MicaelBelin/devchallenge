using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevChallenge.Server.Model
{
    public interface IScenarioManager
    {
        void AddScenario(IScenario scenario);
        void RemoveScenario(IScenario scenario);

        IEnumerable<IScenario> Scenarios { get; }

        event Action<IScenario> ScenarioAdded;
        event Action<IScenario> ScenarioRemoved;

        void AddAgent(IAgent agent);
    }
}
