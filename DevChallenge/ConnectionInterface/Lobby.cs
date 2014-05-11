using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DevChallenge.ConnectionInterface
{
    public class Lobby
    {

        DevChallenge.IConnection connection;

        string requestid;

        List<string> scenarios;

        public Lobby(DevChallenge.IConnection connection)
        {          
            this.connection = connection;
            connection.RegisterFilter(e =>
                {
                    var request = connection.GetRequest(e);
                    if (request.Message.Name != "scenario.select") return FilterResponse.PassToNext;
                    {
                        scenarios = request.Message.Element("scenarios").Elements().Select(x => x.Attribute("name").Value).ToList();
                    }

                    requestid = request.RequestId;

                    return FilterResponse.Consume;
                });
        }

        public IEnumerable<string> Scenarios
        {
            get
            {
                connection.ExecWhile(() => !HasScenarios);
                return scenarios;
            }
        }

        public bool HasScenarios
        {
            get
            {
                return scenarios != null;
            }
        }

        bool hasjoined = false;
        public void Join(string scenarioname)
        {
            if (hasjoined) return;
            hasjoined = true;
            connection.ExecWhile(() => !HasScenarios);
            connection.SendResponse(new XElement("scenario.join", new XAttribute("name", scenarioname)),requestid);
        }

    }
}
