using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DevChallenge.Server.Model;

namespace DevChallenge.Server.Implementation
{
    public class Agent : IAgent
    {


        public Agent(IConnection connection, IUser user,string name, int revision)
        {
            Owner = user;
            Name = name;
            Connection = connection;
        }




        public IUser Owner {get; private set;}

        public string Name {get;private set;}

        public int Revision {get; private set;}

        public IConnection Connection {get; private set;}


    }
}
