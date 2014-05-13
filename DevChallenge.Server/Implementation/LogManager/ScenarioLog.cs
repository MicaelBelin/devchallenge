using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevChallenge.Server.Model;

namespace DevChallenge.Server.Implementation
{
    public partial class LogManager
    {

        class ScenarioLog : IScenarioLog
        {
            public db.Scenario ScenarioDbObject {get; private set;}
            public db.DevChallengeDataContext DataContext { get { return LogManager.DataContext; } }
            public LogManager LogManager {get;private set;}

            public ScenarioLog(LogManager owner, IScenario scenario)
            {
                LogManager = owner;
                owner.WorkerDispatcher.Invoke(() =>
                    {
                        ScenarioDbObject = DataContext.Scenarios.SingleOrDefault(x => x.Name == scenario.Name);
                        if (ScenarioDbObject == null)
                        {
                            ScenarioDbObject = new db.Scenario() { Name = scenario.Name, Code = scenario.Code, Description = scenario.Description };
                            DataContext.Scenarios.InsertOnSubmit(ScenarioDbObject);
                            DataContext.SubmitChanges();
                        }
                    });
            }

            public string Name
            {
                get { return ScenarioDbObject.Name; }
            }

            public string Description
            {
                get { return ScenarioDbObject.Description; }
            }

            public string Code
            {
                get { return ScenarioDbObject.Code; }
            }

            public Model.Instance.ILog CreateInstanceLog(IInstance instance)
            {
                return new InstanceLog(this, instance);
            }
        }

    }
}
