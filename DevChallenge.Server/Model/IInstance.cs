using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevChallenge.Server.Model
{
    public interface IInstance
    {
        IScenario Scenario { get; }

        IEnumerable<IAgent> Agents { get; }

        IInstanceLog Log { get; }

        event Action<IInstance> Started;
        event Action<IInstance> Finished;
    }
}
