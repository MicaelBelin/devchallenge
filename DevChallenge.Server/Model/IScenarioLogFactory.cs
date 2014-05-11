using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevChallenge.Server.Model;

namespace DevChallenge.Server.Model
{
    public interface IScenarioLogFactory
    {
        IScenarioLog Create(IScenario scenario);
    }
}
