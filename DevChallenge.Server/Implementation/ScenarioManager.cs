using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DevChallenge.Server.Model;
using Microsoft.CSharp;

namespace DevChallenge.Server.Implementation
{
    public class ScenarioManager : IScenarioManager
    {

        public void AddScenario(IScenario scenario)
        {
            if (scenario == null) throw new ArgumentNullException("scenario");
            lock (scenarios)
            {
                scenarios.Add(scenario);
                eventlog.Add(EventLogType.Message, String.Format("Added Scenario \"{0}\"", scenario.Name));
                if (ScenarioAdded != null) ScenarioAdded(scenario);
            }
        }

        public void RemoveScenario(IScenario scenario)
        {
            if (scenario == null) throw new ArgumentNullException("scenario");
            lock (scenarios)
            {
                scenarios.Remove(scenario);
                eventlog.Add(EventLogType.Message, String.Format("Removed scenario \"{0}\"", scenario.Name));
                if (ScenarioRemoved != null) ScenarioRemoved(scenario);
            }
        }

        List<IScenario> scenarios = new List<IScenario>();

        public IEnumerable<IScenario> Scenarios
        {
            get 
            {
                lock (scenarios)
                {
                    return scenarios.ToArray();
                }
            }
        }


        public ScenarioManager(Model.IEventLog eventlog)
        {
            this.eventlog = new Model.EventLog.Labeled("ScenarioManager",eventlog);
        }

        Model.EventLog.Labeled eventlog;


        public void LoadFromDb()
        {
            throw new NotImplementedException();

//            using (var dbc = new db.DevChallengeDataContext())
//            {
//                foreach (var s in dbc.Scenarios.Where(x => x.Code != null))
//                {
//                    System.Console.WriteLine(String.Format("Loading scenarios for {0}", s.Name));
//                    Dictionary<string, string> providerOptions = new Dictionary<string, string>
//                    {
//                        {"CompilerVersion", "v4.0"}
//                    };
//                    CSharpCodeProvider provider = new CSharpCodeProvider(providerOptions);

//                    CompilerParameters compilerParams = new CompilerParameters
//                    {
//                        GenerateInMemory = true,
//                        GenerateExecutable = false
//                    };

//                    compilerParams.ReferencedAssemblies.Add(System.Reflection.Assembly.GetAssembly(typeof(Scenario)).Location);
//                    compilerParams.ReferencedAssemblies.Add(System.Reflection.Assembly.GetAssembly(typeof(ScenarioFactory)).Location);
//                    compilerParams.ReferencedAssemblies.Add(System.Reflection.Assembly.GetAssembly(typeof(IConnection)).Location);
//                    HashSet<string> refs = new HashSet<string>();
//                    foreach (var v in AppDomain.CurrentDomain.GetAssemblies())
//                    {
//                        if (!v.IsDynamic)
//                            refs.Add(v.Location);
//                    }

//                    compilerParams.ReferencedAssemblies.AddRange(refs.ToArray());

//                    string headerdata = @"
//                    using System;
//                    using System.Collections.Generic;
//                    using System.Linq;
//                    using DevChallenge.Server;
//                    using DevChallenge;
//                    using System.Xml.Linq;
//
//                    using System.Threading.Tasks;
//
//                    ";

//                    CompilerResults results = provider.CompileAssemblyFromSource(compilerParams, headerdata + s.Code);
//                    if (results.Errors.Count != 0)
//                    {
//                        foreach (var r in results.Errors)
//                        {
//                            System.Console.WriteLine(r);
//                        }
//                    }
//                    else
//                    {
//                        Assembly assembly = results.CompiledAssembly;

//                        foreach (var ct in assembly.GetTypes())
//                        {
//                            System.Console.WriteLine(ct.FullName);
//                        }

//                        foreach (var classtype in (from x in assembly.GetTypes() where x.IsSubclassOf(typeof(IScenario)) select x))
//                        {
//                            System.Console.WriteLine(String.Format("\tloading {0}", classtype.FullName));
//                            IScenario instance = assembly.CreateInstance(classtype.FullName) as IScenario;
//                            if (instance == null)
//                            {
//                                continue;
//                            }


//                            AddScenario(instance);

//                        }

//                    }

//                }
//            }
        }



        public void AddAgent(IAgent agent)
        {
            if (agent == null) throw new ArgumentNullException("agent");

            var connection = agent.Connection;

            eventlog.Add(EventLogType.Message, String.Format("Agent added: \"{0}\"", agent.Name));
            eventlog.Add(EventLogType.Message, String.Format("Owner: {0}({1}{2})", agent.Owner.UserName, agent.Owner.FullName, agent.Owner.Email));

            var response = agent.Connection.SendRequest(new XElement("scenario.select",
                new XElement("scenarios",
                    Scenarios.Select(x => new XElement("scenario", new XAttribute("name", x.Name))))));


            var scenarioname = response.Attribute("name").Value;


            var scenario = Scenarios.SingleOrDefault(x => x.Name == scenarioname);
            if (scenario == null)
            {
                eventlog.Add(EventLogType.Message, string.Format("Agent \"{0}\" selected invalid scenario: \"{1}\"", agent.Name, scenarioname));
                connection.SendNotification(new XElement("scenario.error", new XElement("message", "Scenario not found")));
                agent.Connection.Close();
            }
            else
            {
                eventlog.Add(EventLogType.Message, string.Format("Agent \"{0}\" selected scenario: \"{1}\"", agent.Name, scenarioname));
                scenario.AddAgent(agent, response);
            }


            /*
                        var connection = agent.Connection;
                        agent.Connection.RegisterFilter(e =>
                        {
                            var request = connection.GetRequest(e);
                            if (request.Message.Name != "scenario.list") return DevChallenge.FilterResponse.Mismatch;

                            connection.SendResponse(new XElement("scenarios",
                                Scenarios.Select(x => new XElement("scenario", new XAttribute("name",x.Name))))
                                , request.RequestId);
                            return DevChallenge.FilterResponse.Match;
                        });

                        agent.Connection.RegisterFilter(e =>
                        {
                            var request = connection.GetRequest(e);
                            if (request.Message.Name != "scenario.join") return DevChallenge.FilterResponse.Mismatch;
                            var scenarioname = request.Message.Attribute("name").Value;


                            var scenario = Scenarios.SingleOrDefault(x => x.Name == scenarioname);
                            if (scenario == null)
                            {
                                connection.SendResponse(new XElement("error", new XElement("message", "Scenario not found")), request.RequestId);
                                return DevChallenge.FilterResponse.Match;
                            }
                            connection.SendResponse(new XElement("ok"),request.RequestId);
                            scenario.AddAgent(agent,request.Message);

                            return DevChallenge.FilterResponse.Match;
                        });
            */
            

        }


        public event Action<IScenario> ScenarioAdded;
        public event Action<IScenario> ScenarioRemoved;
    }
}
