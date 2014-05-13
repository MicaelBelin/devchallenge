using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DevChallenge.Server.Model;

namespace DevChallenge.Server.Implementation
{
    public abstract partial class SingleUserScenario : IScenario
    {
        protected abstract void Execute(IAgent agent, Model.Instance.ILog log,XElement parameters);

        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Code { get; private set; }

        public SingleUserScenario(string name, IScenarioLogFactory factory, string description = null, string code = null)
        {
            Name = name;
            Description = description;
            Code = code;
            this.scenariolog = factory.Create(this);
        }
    

        public void AddAgent(IAgent agent, XElement parameters)
        {
            var newinstance = new Instance(this, agent);
            lock (instances)
            {
                instances.Add(newinstance);
            }

            if (InstanceAdded != null) InstanceAdded(newinstance);

            newinstance.Finished += i =>
                {
                    lock (instances)
                    {
                        instances.Remove(newinstance);
                        if (InstanceRemoved != null) InstanceRemoved(newinstance);
                    }
                };

            newinstance.Start(parameters);
        }

        List<Instance> instances = new List<Instance>();
        public IEnumerable<IInstance> Instances
        {
            get 
            {
                lock (instances)
                {
                    return instances.ToArray();
                }
            }
        }

        IScenarioLog scenariolog;
        public IScenarioLog Log { get { return scenariolog; } }

        public event Action<IInstance> InstanceAdded;

        public event Action<IInstance> InstanceRemoved;


    }

    public class SingleUserScenarioDelegate : SingleUserScenario
    {
        public SingleUserScenarioDelegate(string name, IScenarioLogFactory factory, string description = null, string code = null) : base(name,factory,description,code)
        {
        }


        protected override void Execute(IAgent agent, Model.Instance.ILog log, XElement parameters)
        {
            if (ExecuteDelegate != null) ExecuteDelegate(agent, log,parameters);
        }

        public Action<IAgent, Model.Instance.ILog,XElement> ExecuteDelegate { get; set; }

    }
}
