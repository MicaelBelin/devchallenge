using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DevChallenge.Server.Model;

namespace DevChallenge.Server.Implementation
{
    public partial class SingleUserScenario
    {
        class Instance : IInstance
        {

            SingleUserScenario owner;
            IAgent agent;
            public Instance(SingleUserScenario owner, IAgent agent)
            {
                Log = owner.Log.CreateInstanceLog(this);
                this.owner = owner;
                this.agent = agent;
            }

            public void Start(XElement parameters)
            {
                if (Started != null) Started(this);
                try
                {
                    owner.Execute(agent,Log,parameters);
                }
                finally
                {
                    agent.Connection.Close();
                    if (Finished != null) Finished(this);
                }
            }

            public IScenario Scenario
            {
                get { return owner; }
            }


            public IEnumerable<IAgent> Agents
            {
                get 
                {
                    return Enumerable.Repeat(agent, 1);
                }
            }

            public event Action<IInstance> Started;
            public event Action<IInstance> Finished;

            public Model.Instance.ILog Log {get;set;}
        }
    }
}
