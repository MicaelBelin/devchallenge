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
        class InstanceLog : Model.Instance.ILog
        {
            public ScenarioLog ScenarioLog {get; private set;}
            public db.DevChallengeDataContext DataContext { get { return ScenarioLog.DataContext; } }
            public LogManager LogManager { get { return ScenarioLog.LogManager; } }

            public db.Instance InstanceDbObject { get; private set; }

            public InstanceLog(ScenarioLog owner, IInstance instance)
            {
                ScenarioLog = owner;
                Instance = instance;

                LogManager.WorkerDispatcher.Invoke(() =>
                    {
                        InstanceDbObject = new db.Instance() { ScenarioId = ScenarioLog.ScenarioDbObject.Id };
                        DataContext.Instances.InsertOnSubmit(InstanceDbObject);
                        DataContext.SubmitChanges();
                    });
            }

            public IInstance Instance {get;private set;}

            public void Start()
            {
                var now = DateTime.Now;
                LogManager.WorkerDispatcher.BeginInvoke(new Action(() =>
                    {
                        InstanceDbObject.Started = now;
                    }));
            }

            public void Finish(System.Xml.Linq.XElement finishlog)
            {
                var now = DateTime.Now;
                LogManager.WorkerDispatcher.BeginInvoke(new Action(() =>
                    {
                        InstanceDbObject.Finished = now;
                        InstanceDbObject.Data = finishlog == null ? null : finishlog.ToString();
                        DataContext.SubmitChanges();
                    }));
            }


            Dictionary<IAgent, db.AgentRecord> agentRecord = new Dictionary<IAgent, db.AgentRecord>();

            public void SetScore(IAgent agent, int? score)
            {
                LogManager.WorkerDispatcher.BeginInvoke(new Action(() =>
                    {
                        db.AgentRecord record = null;

                        if (agentRecord.ContainsKey(agent)) record = agentRecord[agent];
                        else
                        {
                            record = new db.AgentRecord() { Instance = InstanceDbObject, UserId = agent.Owner.Id, Name = agent.Name, Revision = agent.Revision };
                            DataContext.AgentRecords.InsertOnSubmit(record);
                            agentRecord.Add(agent, record);
                        }
                        record.Score = score;
                        DataContext.SubmitChanges();
                    }));
            }
        }
    }
}
