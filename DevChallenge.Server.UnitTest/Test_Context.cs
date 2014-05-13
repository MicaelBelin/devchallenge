using System;
using System.Linq;
using DevChallenge.Server.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test_DevChallengeServer
{
    [TestClass]
    public class Test_Context
    {
        [TestMethod]
        public void TestAgentSpawnedConnection()
        {
            var scenariomanager = new DevChallenge.Server.Model.Fakes.StubIScenarioManager();

            var agent = new DevChallenge.Server.Model.Fakes.StubIAgent();

            bool gotevent = false;

            scenariomanager.AddAgentIAgent = a =>
                {
                    Assert.AreEqual(a, agent);
                    gotevent = true;
                };

            var source = new DevChallenge.Server.Model.Agent.Fakes.StubISource();


            var c = new DevChallenge.Server.Implementation.Context(scenariomanager, Enumerable.Repeat(source,1));


            source.AgentSpawnedEvent(agent);

            Assert.AreEqual(true, gotevent);

        }
    }
}
