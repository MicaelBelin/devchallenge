using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DevChallenge.Server.Scenarios.MineSweeper
{
    public class Scenario : DevChallenge.Server.Implementation.SingleUserScenario
    {

        public class Factory : Model.IScenarioFactory
        {
            public Model.IScenario Create(Model.IScenarioLogFactory logfactory)
            {
                return new Scenario(logfactory);
            }
        }


        public Scenario(DevChallenge.Server.Model.IScenarioLogFactory logfactory)
            : base("minesweeper", logfactory)
        {
        }



        static object enginelocker = new object();
        static Simulation int_engine;

        static Simulation engine
        {
            get
            {
                lock (enginelocker)
                {
                    if (int_engine == null)
                    {
                        int_engine = new Simulation(10, 10,10,2);

                        foreach (var index in Enumerable.Range(0, 10))
                        {
                            int_engine.CreateMine();
                        }


                        int_engine.Start();
                    }
                    return int_engine;
                }
            }
        }

        protected override void Execute(Model.IAgent agent, Model.Instance.ILog log, System.Xml.Linq.XElement parameters)
        {
            Simulation.Sweeper sweeper = null;
            try
            {
                var connection = agent.Connection;
                sweeper = engine.CreateSweeper();
                connection.SendNotification(new XElement("init",
                    new XAttribute("sweeperid", sweeper.Id),
                    new XAttribute("worldwidth", engine.Width),
                    new XAttribute("worldheight", engine.Height),
                    new XAttribute("maxminecount", engine.MaxMineCount),
                    new XAttribute("mineregenerationtime", engine.MineRegenerationTime)));

                int steps = 0;
                int scores = 0;
                log.Start();

                sweeper.MoveDelegate = (state, timeout) =>
                    {
                        steps++;
                        try
                        {
                            var response = connection.SendRequest(state, timeout);
                            return (Simulation.Direction)Enum.Parse(typeof(Simulation.Direction), response.Value);
                        }
                        catch (TimeoutException)
                        {
                            connection.SendNotification(new XElement("timeout"));
                            return Simulation.Direction.None;
                        }
                        catch(Exception e)
                        {
                            connection.SendNotification(new XElement("error",e.ToString()));
                            return Simulation.Direction.None;
                        }
                    };
                sweeper.ScoreDelegate = () =>
                    {
                        scores++;
                        connection.SendNotification(new XElement("minecollected"));
                    };
                sweeper.NewMineAddedDelegate = (x, y) =>
                    {
                        connection.SendNotification(new XElement("minecreated",
                            new XAttribute("x", x),
                            new XAttribute("y", y)));
                    };

                engine.RegisterSweeper(sweeper);

                connection.RunUntilClosed();

                log.SetScore(agent, scores);
                log.Finish(new XElement("result",
                    new XAttribute("ticks", steps)));

            }
            finally
            {
                if (sweeper != null) engine.UnregisterSweeper(sweeper);
            }

        }
    }
}
