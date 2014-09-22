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

        public Program(Model.IEventLog eventlog)
        {
            this.eventlog = new Model.EventLog.Labeled("Program",eventlog);
            scenariomanager = new Implementation.ScenarioManager(eventlog);
        }

        Implementation.ScenarioManager scenariomanager;
        Model.EventLog.Labeled eventlog;


        public void LoadInternalChallenges()
        {
            eventlog.Add(Model.EventLogType.Message, String.Format("Loading internal challenges..."));




            var scenariologfactory = new Implementation.LogManager(new db.DevChallengeDataContext());

            var factories = new List<Model.IScenarioFactory>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                eventlog.Add(Model.EventLogType.Message, string.Format("Checking for scenarios in {0}", assembly.FullName));
                var typestoload = assembly.GetTypes().Where(x => x.GetInterfaces().
                    Contains(typeof(DevChallenge.Server.Model.IScenarioFactory)));

                foreach (var typetoload in typestoload)
                {
                    eventlog.Add(Model.EventLogType.Message, string.Format("Loading \"{0}\"", typetoload.FullName));
                    var factory = (Model.IScenarioFactory)Activator.CreateInstance(typetoload);
                    var scenario = factory.Create(scenariologfactory);
                    scenariomanager.AddScenario(scenario);
                }
            }



            eventlog.Add(Model.EventLogType.Message, String.Format("Loading challenges completed"));
        }

        public void LoadChallenges(DirectoryInfo path)
        {

            eventlog.Add(Model.EventLogType.Message, String.Format("Loading challenges..."));


            eventlog.Add(Model.EventLogType.Message, String.Format("Load directory: \"{0}\"", path.FullName));

            if (!path.Exists)
            {
                eventlog.Add(Model.EventLogType.Error, String.Format("Directory \"{0}\" was not found. Skipping load.", path.FullName));
                return;
            }

            var files = path.EnumerateFiles().Where(x => x.Extension.ToLower() == ".dll");

            //            var domain = AppDomain.CreateDomain("challenges");
            var scenariologfactory = new Implementation.LogManager(new db.DevChallengeDataContext());

            var factories = new List<Model.IScenarioFactory>();

            foreach (var file in files)
            {
                eventlog.Add(Model.EventLogType.Message, string.Format("Checking file: \"{0}\"", file.FullName));
                var assembly = Assembly.LoadFile(file.FullName);
                var typestoload = assembly.GetTypes().Where(x => x.GetInterfaces().
                    Contains(typeof(DevChallenge.Server.Model.IScenarioFactory)));

                foreach (var typetoload in typestoload)
                {
                    eventlog.Add(Model.EventLogType.Message, string.Format("Loading \"{0}\"", typetoload.FullName));
                    var factory = (Model.IScenarioFactory)Activator.CreateInstance(typetoload);
                    var scenario = factory.Create(scenariologfactory);
                    scenariomanager.AddScenario(scenario);
                }
            }



            eventlog.Add(Model.EventLogType.Message, String.Format("Loading challenges completed"));

        }

        public void Run()
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



}
