using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test_DevChallengeServer
{
    [TestClass]
    public class Test_SingleUserScenario
    {



        [TestMethod]
        public void ExecuteIsCalled()
        {
            var scenariologfactory = new DevChallenge.Server.Model.Fakes.StubIScenarioLogFactory();
            scenariologfactory.CreateIScenario = (a) => { return new DevChallenge.Server.Model.Fakes.StubIScenarioLog(); };

            AutoResetEvent resetevent = new AutoResetEvent(false);

            var scenario = new DevChallenge.Server.Implementation.SingleUserScenarioDelegate("testscenario",scenariologfactory)
            {
                ExecuteDelegate = (a, l,p) =>
                    {
                        resetevent.Set();
                    }
            };

            var agent = new DevChallenge.Server.Model.Fakes.StubIAgent();
            var connection = new DevChallenge.Fakes.StubIConnection();
            agent.ConnectionGet = () => connection;
            scenario.AddAgent(agent,null);

            Assert.IsTrue(resetevent.WaitOne(TimeSpan.FromSeconds(5)));

        }




    }
}
