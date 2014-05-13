using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using DevChallenge.Server.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test_DevChallengeServer
{
    [TestClass]
    public class Test_ScenarioManager
    {



        [TestMethod]
        public void ListScenarios_SelectScenario()
        {
            var scenariomanager = new ScenarioManager(null);

            var connection = new DevChallenge.Fakes.FakeConnection();



            var agent = new DevChallenge.Server.Model.Fakes.StubIAgent();
            agent.ConnectionGet = () => connection;


            var scenario = new DevChallenge.Server.Model.Fakes.StubIScenario();
            scenario.NameGet = () => "FakeScenario";

            scenariomanager.AddScenario(scenario);


            bool gotrequest = false;
            connection.SendRequestXElement = e =>
                {
                    Assert.AreEqual("scenario.select", e.Name);
                    Assert.AreEqual(1, e.Elements("scenarios").Count());
                    var scenarios = e.Element("scenarios");
                    Assert.AreEqual(1, scenarios.Elements("scenario").Count());
                    Assert.AreEqual("FakeScenario", scenarios.Element("scenario").Attribute("name").Value);
                    gotrequest = true;
                    return new XElement("scenario.join", new XAttribute("name", "FakeScenario"));
                };

            scenariomanager.AddAgent(agent);

            Assert.IsTrue(gotrequest);
        }

        [TestMethod]
        public void SelectInvalidScenario()
        {
            var scenariomanager = new ScenarioManager(null);

            var connection = new DevChallenge.Fakes.FakeConnection();



            var agent = new DevChallenge.Server.Model.Fakes.StubIAgent();
            agent.ConnectionGet = () => connection;


            var scenario = new DevChallenge.Server.Model.Fakes.StubIScenario();
            scenario.NameGet = () => "FakeScenario";

            scenariomanager.AddScenario(scenario);


            bool gotrequest = false;
            connection.SendRequestXElement = e =>
            {
                Assert.AreEqual("scenario.select", e.Name);
                Assert.AreEqual(1, e.Elements("scenarios").Count());
                var scenarios = e.Element("scenarios");
                Assert.AreEqual(1, scenarios.Elements("scenario").Count());
                Assert.AreEqual("FakeScenario", scenarios.Element("scenario").Attribute("name").Value);
                gotrequest = true;
                return new XElement("scenario.join", new XAttribute("name", "nonexistingscenario"));
            };

            bool gotnotification = false;
            connection.SendNotificationXElement = e =>
                {
                    Assert.AreEqual("scenario.error", e.Name);
                    Assert.AreEqual("Scenario not found", e.Element("message").Value);
                    gotnotification = true;
                };


            scenariomanager.AddAgent(agent);

            Assert.IsTrue(gotrequest);
            Assert.IsTrue(gotnotification);

        }





    }
}
