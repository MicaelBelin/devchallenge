using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DevChallenge.Server.Scenarios.Fragmentation
{
    class Scenario : DevChallenge.Server.Implementation.SingleUserScenario
    {
        public class Factory : Model.IScenarioFactory
        {
            public Model.IScenario Create(Model.IScenarioLogFactory logfactory)
            {
                return new Scenario(logfactory);
            }
        }


        public Scenario(DevChallenge.Server.Model.IScenarioLogFactory logfactory)
            : base("fragmentation", logfactory)
        {
        }


        class Instance
        {
            interface ICrateDeployment
            {
                XElement Request { get; }

                void Initialize(Instance scenario);

                void Validate(Instance scenario);
            }

            class InsertCrate : ICrateDeployment
            {
                public InsertCrate(int id, int size)
                {
                    Crate = new Crate(id, size);
                }

                public Crate Crate { get; private set; }

                public XElement Request
                {
                    get
                    {
                        return new XElement("insert", new XAttribute("id", Crate.Id), new XAttribute("size", Crate.Size));
                    }
                }

                public void Initialize(Instance instance)
                {
                    instance.Bay.Add(Crate);
                }

                public void Validate(Instance instance)
                {
                    if (instance.Bay.Count != 0) throw new StorageException("There are still packets in bay");
                }
            }

            class RemoveCrate : ICrateDeployment
            {
                public RemoveCrate(Crate c)
                {
                    Crate = c;
                }

                public Crate Crate { get; private set; }

                public XElement Request
                {
                    get
                    {
                        return new XElement("remove", new XAttribute("id", Crate.Id));
                    }
                }

                public void Initialize(Instance instance)
                {
                }

                public void Validate(Instance instance)
                {
                    if (instance.Bay.Count != 1 || instance.Bay[0] != Crate) throw new StorageException("Invalid packets in bay");
                    instance.Bay.Clear();
                }
            }

            Random rand = new Random();
            int nextcrateid = 0;

            IEnumerable<ICrateDeployment> Batch(int capacity, int runs, double dtargetfill, double dtargetcratesize)
            {
                int storagesize = 0;
                if (storage.Crates.Count != 0) throw new StorageException("There was an internal error in scenario - storage was not initially empty");

                int targetfill = (int)((double)capacity * dtargetfill);

                int targetcratesize = (int)((double)capacity * dtargetcratesize);

                for (var run = 0; run < runs; ++run)
                {
                    int cratesize = targetcratesize + (rand.Next(targetcratesize) - targetcratesize / 2) / 3;
                    if (cratesize > capacity - storagesize) cratesize = capacity - storagesize;

                    ICrateDeployment ret = null;

                    bool doremove = rand.Next(2) == 0;

                    if (rand.Next(3) != 0)
                        doremove = storagesize > targetfill;

                    if (storage.Crates.Count == 0) doremove = false;
                    if (cratesize <= 0) doremove = true;

                    if (doremove)
                    {
                        Crate cratetoremove = storage.Crates.Skip(rand.Next(storage.Crates.Count)).First().Value;
                        storagesize -= cratetoremove.Size;
                        ret = new RemoveCrate(cratetoremove);
                    }
                    else
                    {
                        ret = new InsertCrate(nextcrateid++, cratesize);
                        storagesize += cratesize;
                    }

                    yield return ret;


                }


                foreach (var crate in storage.Crates.Values.ToList())
                {
                    yield return new RemoveCrate(crate);
                }
            }

            IEnumerable<ICrateDeployment> Deployments(int capacity)
            {
                var targetfills = new double[] { 0.1, 0.7, 0.9, 0.98 };
                var targetsizes = new double[] { 0.1, 0.3, 0.5, 0.7 };

                foreach (var batch in (from targetfill in targetfills
                                       from targetsize in targetsizes
                                       select new { targetfill, targetsize }))
                {
                    foreach (var item in Batch(capacity, 1000, batch.targetfill, batch.targetsize)) yield return item;
                }


            }


            Storage storage;
            List<Crate> Bay = new List<Crate>();
            int movements = 0;

            void ManageCommands(XElement root)
            {
                foreach (var e in root.Elements())
                {
                    movements++;
                    if (e.Name == "move")
                    {
                        int crateid = Convert.ToInt32(e.Attribute("id").Value);
                        int newpos = Convert.ToInt32(e.Attribute("position").Value);
                        Crate crate = null;
                        crate = storage.GetCrate(crateid);
                        storage.Relocate(crate, newpos);
                    }
                    else if (e.Name == "insert")
                    {
                        int crateid = Convert.ToInt32(e.Attribute("id").Value);
                        Crate crate = null;
                        try
                        {
                            crate = Bay.Single(x => x.Id == crateid);
                        }
                        catch (Exception)
                        {
                            throw new StorageException(String.Format("No crate with id {0} exists in bay", crateid));
                        }
                        int newpos = Convert.ToInt32(e.Attribute("position").Value);

                        storage.Add(crate, newpos);
                        Bay.Remove(crate);

                    }
                    else if (e.Name == "remove")
                    {
                        int crateid = Convert.ToInt32(e.Attribute("id").Value);
                        var crate = storage.GetCrate(crateid);
                        storage.Remove(crate);
                        Bay.Add(crate);
                    }
                    else throw new StorageException(String.Format("Invalid command {0}", e.Name));
                }
            }

            public Instance(Model.IAgent agent, Model.Instance.ILog log, System.Xml.Linq.XElement parameters)
            {
                int capacity = 1000;

                agent.Connection.SendNotification(new XElement("capacity", capacity));

                storage = new Storage(capacity);



                log.Start();


                try
                {
                    foreach (var deployment in Deployments(capacity))
                    {
                        deployment.Initialize(this);
                        var response = agent.Connection.SendRequest(deployment.Request);
                        ManageCommands(response);
                        deployment.Validate(this);
                    }
                }
                catch (StorageException e)
                {
                    agent.Connection.SendNotification(new XElement("Error", e.Message));
                }

                log.Finish(null);
                log.SetScore(agent, movements);

            }


        }



        protected override void Execute(Model.IAgent agent, Model.Instance.ILog log, System.Xml.Linq.XElement parameters)
        {

            try
            {

                new Instance(agent, log, parameters);

            }
            catch (Exception e)
            {
                throw e;
            }

        }
    }
}
