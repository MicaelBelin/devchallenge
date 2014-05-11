using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Linq;
using System.Linq;

namespace Test_DevChallengeServer
{
    /// <summary>
    /// Summary description for Test_ScenarioLog
    /// </summary>
    [TestClass]
    public class Test_ScenarioLog
    {
        public Test_ScenarioLog()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        static FakeDataContext DataContext;

        [ClassInitialize()]
        public static void Initialize(TestContext testContext)
        {
            DataContext = new FakeDataContext();
        }

        [ClassCleanup()]
        public static void Shutdown()
        {
            DataContext.DeleteDatabase();
        }

        [TestCleanup()]
        public void Clear()
        {
            //            DataContext.Clear();
        }


        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        //
        // Use ClassCleanup to run code after all tests in a class have run
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion
/*
        [TestMethod]
        public void CreateScenarioDbObject()
        {
            lock (DataContext)
            {
                var scenario = new DevChallenge.Server.Model.Fakes.StubIScenario();

                scenario.NameGet = () => { return "testscenario"; };
                scenario.CodeGet = () => { return "thecode"; };
                scenario.DescriptionGet = () => { return "thedescription"; };
               
                var log = new DevChallenge.Server.Implementation.ScenarioLog(DataContext, scenario);

                var dbscenario = DataContext.Scenarios.Single(x => x.Name == "testscenario");

                Assert.AreEqual("testscenario", dbscenario.Name);
                Assert.AreEqual("thecode", dbscenario.Code);
                Assert.AreEqual("thedescription", dbscenario.Description);

                var log2 = new DevChallenge.Server.Implementation.ScenarioLog(DataContext, scenario);

                Assert.AreEqual(1, DataContext.Scenarios.Count());

                DataContext.Clear();
            }
        }

        [TestMethod]
        public void CreateInstanceLog()
        {
            lock (DataContext)
            {
                var scenario = new DevChallenge.Server.Model.Fakes.StubIScenario();

                scenario.NameGet = () => { return "testscenario"; };

                var log = new DevChallenge.Server.Implementation.ScenarioLog(DataContext, scenario);

                var instance = new DevChallenge.Server.Model.Fakes.StubIInstance();

                var instancelog = log.CreateInstanceLog(instance);

                instancelog.Start();

                instancelog.Finish(new System.Xml.Linq.XElement("finishdata"));


                var dbinstancelogitem = DataContext.Instances.Single();

                Assert.AreEqual(new System.Xml.Linq.XElement("finishdata").ToString(), System.Xml.Linq.XElement.Parse(dbinstancelogitem.Data).ToString());
                Assert.IsNotNull(dbinstancelogitem.Started);
                Assert.IsNotNull(dbinstancelogitem.Finished);



                DataContext.Clear();
            }
        }

        [TestMethod]
        public void SetScore()
        {
            lock (DataContext)
            {
                var scenario = new DevChallenge.Server.Model.Fakes.StubIScenario();

                scenario.NameGet = () => { return "testscenario"; };

                var scenariolog = new DevChallenge.Server.Implementation.ScenarioLog(DataContext, scenario);
                var instance = new DevChallenge.Server.Model.Fakes.StubIInstance();
                var instancelog = scenariolog.CreateInstanceLog(instance);

                var agent = new DevChallenge.Server.Model.Fakes.StubIAgent();

                var um = new DevChallenge.Server.Implementation.UserManager(DataContext);
                var user = um.Create("testuser", "secret", "test user", "");

                agent.OwnerGet = () => user;
                agent.NameGet = () => "testagent";
                agent.RevisionGet = () => 1;


                instancelog.SetScore(agent, 556);

                var dbagent = DataContext.AgentRecords.Single();

                Assert.AreEqual("testagent", dbagent.Name);
                Assert.AreEqual(1, dbagent.Revision);
                Assert.AreEqual(user.Id, dbagent.UserId);
                Assert.AreEqual(556, dbagent.Score);

                DataContext.Clear();
            }
        }
*/

    }
}
