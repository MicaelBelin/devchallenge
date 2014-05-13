using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevChallenge.Server.Model;

namespace DevChallenge.Server.Implementation
{
    public class Context : IContext
    {
        public Context(IScenarioManager scenariomanager, IEnumerable<Model.Agent.ISource> entrypoints)
        {
            this.scenariomanager = scenariomanager;
            if (entrypoints != null) foreach (var ep in entrypoints) AddEntryPoint(ep);
        }

        IScenarioManager scenariomanager;
        public IScenarioManager ScenarioManager
        {
            get { return scenariomanager; }
        }

        List<Model.Agent.ISource> agentsources = new List<Model.Agent.ISource>();
        public IEnumerable<Model.Agent.ISource> AgentSources
        {
            get 
            {
                lock (agentsources)
                {
                    return agentsources.ToArray();
                }
            }
        }

        public void AddEntryPoint(Model.Agent.ISource ep)
        {
            lock(agentsources)
            {
                agentsources.Add(ep);
                ep.AgentSpawned += scenariomanager.AddAgent;
            }
        }

        public void RemoveEntryPoint(Model.Agent.ISource ep)
        {
            if (!agentsources.Contains(ep)) throw new InvalidOperationException("entry point not found");
            ep.AgentSpawned -= scenariomanager.AddAgent;
            agentsources.Remove(ep);
        }

    }
}
