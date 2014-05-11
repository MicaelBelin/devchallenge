using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DevChallenge.Server.Model
{


    public interface IScenario
    {
        string Name { get; }
        string Description { get; }
        string Code { get; }

        void AddAgent(IAgent agent,XElement parameters);


        IScenarioLog Log { get; }

        IEnumerable<IInstance> Instances { get; }
        event Action<IInstance> InstanceAdded;
        event Action<IInstance> InstanceRemoved;

    }

    public interface IScenarioFactory
    {
        IScenario Create(IScenarioLogFactory logfactory);
    }
}
