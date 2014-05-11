using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DevChallenge.Server
{


    public class Program
    {

        static Implementation.ScenarioManager scenariomanager = new Implementation.ScenarioManager();


        public static void LoadChallenges(DirectoryInfo path)
        {
            var files = path.EnumerateFiles().Where(x => x.Extension.ToLower() == ".dll");

//            var domain = AppDomain.CreateDomain("challenges");

            var newassemblies = files.Select(x => Assembly.LoadFile(x.FullName));

            var toload = newassemblies.SelectMany(assembly => assembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(DevChallenge.Server.Model.IScenarioFactory))));


            var scenariologfactory = new Implementation.LogManager(new db.DevChallengeDataContext());

            var factories = toload.Select(x => (Model.IScenarioFactory)Activator.CreateInstance(x));
            var scenarios = factories.Select(x=>x.Create(scenariologfactory));

            foreach (var scenario in scenarios)
            {
                scenariomanager.AddScenario(scenario);
            }


        }

        public static void Run()
        {



            var datacontext = new db.DevChallengeDataContext();
            Implementation.TcpServer Server = new Implementation.TcpServer(8231, datacontext.Connection.ConnectionString);


            Server.AgentSpawned += agent =>
                {
                    scenariomanager.AddAgent(agent);
                };



            Server.Exec();






        }
    }



/*
    public class GuessTheNumber : Scenario
    {
        private static string scenarioname = "guessthenumber";

        public GuessTheNumber()
        {
            CurrentState = State.WaitingForAgents;
        }

        public GuessTheNumber(string customid)
            : base(customid)
        {
        }


        public override XElement ScenarioData()
        {
            return null;
        }

        public override string Name() { return scenarioname; }

        public override void AddAgent(Agent a)
        {
            base.AddAgent(a);
            if (Agents.Count() >= 2)
                CurrentState = State.ReadyToStart;
        }

        public override Task Start()
        {
            if (CurrentState != Scenario.State.ReadyToStart)
            {
                throw new InvalidOperationException("the scenario is not ready to run!");
            }

            CurrentState = State.Running;


            return Task.Run(() =>
            {
                Random rand = new Random();

                Console.WriteLine("scenario started");

                foreach (var v in Agents)
                {
                    v.Finished = true;
                    v.Score = 0;
                }

                var tasks = (from agent in Agents
                             select Task.Run(() =>
                             {
                                 int targetnumber = rand.Next(0, 1000);
                                 int guesses = 0;
                                 Console.WriteLine(String.Format("Starting agent {0}", agent.agentid));
                                 try
                                 {
                                     int lastguess = -1;
                                     while (true)
                                     {
                                         guesses++;
                                         agent.Score = -guesses;
                                         try
                                         {
                                             var response = agent.Connection.SendRequest(new XElement("newguess",
                                                 new XAttribute("minimum", "0"),
                                                 new XAttribute("maximum", "1000"),
                                                 new XElement("lastguess", (lastguess > targetnumber) ? "high" : "low")), 1000);

                                             if (response == null) continue;
                                             lastguess = Convert.ToInt32(response.Value);
                                             if (lastguess == targetnumber) break;
                                         }
                                         catch (TimeoutException)
                                         {
                                         }
                                     }

                                     agent.Connection.SendNotification(new XElement("finished",
                                         new XElement("guesses", guesses)));
                                     agent.Client.Close();
                                 }
                                 catch (ClosedException)
                                 {
                                     agent.Finished = false;
                                 }
                             }));



                Task.WaitAll(tasks.ToArray());

                Console.WriteLine("scenario stopped");


                foreach (var i in Agents.Where(x => x.Finished).OrderByDescending(x => x.Score).Select((x, index) => new { agent = x, index }))
                {
                    i.agent.Position = i.index + 1;
                }

                CurrentState = State.Finished;
            });
        }


        public class Factory : ScenarioFactory
        {
            public override string Name() { return scenarioname; }
            public override XElement LobbyInstancesInformation()
            {
                return null;
            }

            public override Scenario Add(Agent agent, XElement request)
            {
                Scenario ret = null;
                ExecAsExclusive(() =>
                {

                    var openscenarios = (from x in Scenarios where x.CurrentState == Scenario.State.WaitingForAgents select x as GuessTheNumber).ToArray();


                    var rand = new Random();
                    if (openscenarios.Count() > 0)
                    {
                        Console.WriteLine("adding agent to existing guess scenario");
                        var selected = openscenarios[rand.Next(openscenarios.Count())];
                        selected.AddAgent(agent);
                        ret = selected;
                    }
                    else
                    {
                        Console.WriteLine("adding agent to new guess scenario");
                        var newscenario = new GuessTheNumber();
                        AddScenario(newscenario);
                        newscenario.AddAgent(agent);
                        ret = newscenario;
                    }
                });
                return ret;
            }

        }

    }






    class Program
    {
        static void Main(string[] args)
        {



            ScenarioManager.RegisterFactory(new MonitorScenario.Factory());
            ScenarioManager.RegisterFactory(new GuessTheNumber.Factory());
            ScenarioManager.RegisterFactory(new Scenarios.Airplan.Factory());

            ScenarioLoader.Load();


            var server = new TcpListener(IPAddress.Any, 8231);
            server.Start();


            ScenarioManager.StartAutostarter();

            //Handles acception of connections
            while (true)
            {
                var client = server.AcceptTcpClient();
                client.NoDelay = true;

                Task.Factory.StartNew(() =>
                {
                    var peer = ScenarioManager.CreateAgent(client);
                    if (peer == null)
                    {
                        return;
                    }
                    ScenarioManager.AddAgent(peer);
                });

            }



        }
    }
 */ 
}
