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
        public Context(IScenarioManager scenariomanager, IEnumerable<IAgentEntryPoint> entrypoints)
        {
            this.scenariomanager = scenariomanager;
            if (entrypoints != null) foreach (var ep in entrypoints) AddEntryPoint(ep);
        }

        IScenarioManager scenariomanager;
        public IScenarioManager ScenarioManager
        {
            get { return scenariomanager; }
        }

        List<IAgentEntryPoint> entrypoints = new List<IAgentEntryPoint>();
        public IEnumerable<IAgentEntryPoint> EntryPoints
        {
            get 
            {
                lock (entrypoints)
                {
                    return entrypoints.ToArray();
                }
            }
        }

        public void AddEntryPoint(IAgentEntryPoint ep)
        {
            lock(entrypoints)
            {
                entrypoints.Add(ep);
                ep.AgentSpawned += scenariomanager.AddAgent;
            }
        }

        public void RemoveEntryPoint(IAgentEntryPoint ep)
        {
            if (!entrypoints.Contains(ep)) throw new InvalidOperationException("entry point not found");
            ep.AgentSpawned -= scenariomanager.AddAgent;
            entrypoints.Remove(ep);
        }

    }
}
