using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace DevChallenge.Server.Model
{
    public interface IAgent
    {
        IConnection Connection { get; }

        IUser Owner { get; }
        string Name { get; }
        int Revision { get; }


    }
}
