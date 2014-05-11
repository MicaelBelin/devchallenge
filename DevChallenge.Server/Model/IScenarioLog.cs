using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevChallenge.Server.Model
{
    public interface IScenarioLog
    {

        string Name {get;}
        string Description {get;}
        string Code {get;}

        IInstanceLog CreateInstanceLog(IInstance instance);
    }
}
