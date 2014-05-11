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

            var entrypoint = new DevChallenge.Server.Model.Fakes.StubIAgentEntryPoint();


            var c = new DevChallenge.Server.Implementation.Context(scenariomanager, Enumerable.Repeat(entrypoint,1));


            entrypoint.AgentSpawnedEvent(agent);

            Assert.AreEqual(true, gotevent);

        }
    }
}
