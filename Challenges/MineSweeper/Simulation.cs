using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DevChallenge.Server.Scenarios.MineSweeper
{
    public class Simulation
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int MaxMineCount { get; private set; }
        public int MineRegenerationTime { get; private set; }


        public Simulation(int width, int height, int maximummines, int mineregenerationtime)
        {
            Width = width;
            Height = height;
            MaxMineCount = maximummines;
            MineRegenerationTime = mineregenerationtime;
        }

        public class Mine
        {
            public Position Position { get; set; }
            public Mine(Position p)
            {
                Position = p;
            }
        }


        List<Mine> mines = new List<Mine>();

        public Mine CreateMine()
        {
            var p = AvailablePosition;
            return CreateMine(p.X, p.Y);
        }

        public Mine CreateMine(int x, int y)
        {
            var ret = new Mine(new Position(x, y));
            mines.Add(ret);
            return ret;
        }


        public enum Direction
        {
            None,
            Up,
            Right,
            Down,
            Left,
        }

        List<Sweeper> sweepers = new List<Sweeper>();

        public class Position
        {
            public int X { get; set; }
            public int Y { get; set; }

            public Position(int x, int y)
            {
                X = x;
                Y = y;
            }

            public Position Next(Direction dir)
            {
                switch (dir)
                {
                    case Direction.Up:
                        return new Position(X, Y - 1);
                    case Direction.Down:
                        return new Position(X, Y + 1);
                    case Direction.Left:
                        return new Position(X - 1, Y);
                    case Direction.Right:
                        return new Position(X + 1, Y);
                    default:
                        return new Position(X, Y);
                }
            }

            public override int GetHashCode()
            {
                return X ^ Y;
            }

            public override bool Equals(object obj)
            {
                Position p = obj as Position;
                if ((object)p == null) return false;
                return X == p.X && Y == p.Y;
            }

            public static bool operator ==(Position a, Position b)
            {
                if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return true;
                if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return false;
                return a.X == b.X && a.Y == b.Y;
            }

            public static bool operator !=(Position a, Position b)
            {
                if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return false;
                if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return true;
                return a.X != b.X || a.Y != b.Y;
            }

        }

        public class Sweeper
        {
            public int Id { get; private set; }
            public Position Position { get; set; }


            static int lastid = 0;

            public Sweeper(Position p)
            {
                Position = p;
                Id = lastid++;
            }

            public Action ScoreDelegate { get; set; }
            public Action<int,int> NewMineAddedDelegate { get; set; }

            public Func<XElement, TimeSpan, Direction> MoveDelegate { get; set; }
        };


        public Sweeper CreateSweeper()
        {
            var pos = AvailablePosition;
            return CreateSweeper(pos);
        }

        public Sweeper CreateSweeper(Position p)
        {
            return new Sweeper(p);
        }

        public void RegisterSweeper(Sweeper sweeper)
        {
            lock (sweepers)
            {
                sweepers.Add(sweeper);
                Monitor.PulseAll(sweepers);
            }
        }

        public void UnregisterSweeper(Sweeper sweeper)
        {
            lock (sweepers)
            {
                sweepers.Remove(sweeper);
                Monitor.PulseAll(sweepers);
            }
        }

        Random rand = new Random();

        public Position AvailablePosition
        {
            get
            {
                int attempts = 10000;
                while (attempts-- != 0)
                {
                    var p = new Position(rand.Next(Width), rand.Next(Height));

                    if (sweepers.All(s => s.Position != p)
                        && mines.All(m => m.Position != p))
                    {
                        return p;
                    }
                }
                throw new Exception("World is too crowded!");
            }
        }

        public TimeSpan timeout = TimeSpan.FromMilliseconds(300);


        public async Task Start()
        {
            await Task.Run(() =>
                {
                    long iteration = 0;
                    while (true)
                    {
                        try
                        {

                            lock (sweepers)
                            {


                                while (sweepers.Count == 0) 
                                {
                                    Monitor.Wait(sweepers, TimeSpan.FromMilliseconds(500));
                                }


                                var sw = System.Diagnostics.Stopwatch.StartNew();

                                List<Tuple<Sweeper, Direction>> commandlist = GetSweeperCommands().ToList();
                                ExecuteCommands(AdjustForCollisions(commandlist));



                                CollectMines();


                                if (iteration % MineRegenerationTime == 0 && mines.Count < MaxMineCount) //respawn mine every n:th tick if needed
                                {
                                    var newmine = CreateMine();

                                    foreach (var sweeper in sweepers)
                                    {
                                        if (sweeper.NewMineAddedDelegate != null) sweeper.NewMineAddedDelegate(newmine.Position.X, newmine.Position.Y);
                                    }
                                }


                                TimeSpan timeleft = TimeSpan.FromMilliseconds(100) - sw.Elapsed;
                                if (timeleft > TimeSpan.Zero)
                                    System.Threading.Thread.Sleep(timeleft);


                            }
                            iteration++;
                        }
                        catch(Exception e)
                        {
                            System.Console.WriteLine(e.Message);
                            System.Console.WriteLine(e.StackTrace);
                        }
                    }
                });
        }

        private void CollectMines()
        {
            foreach (var sweeper in sweepers)
            {
                var hitmines = mines.Where(x => x.Position == sweeper.Position).ToList();

                foreach (var hit in hitmines)
                {
                    if (sweeper.ScoreDelegate != null) sweeper.ScoreDelegate();
                    hit.Position = AvailablePosition;
                    mines.Remove(hit);
                }
            }
        }

        private void ExecuteCommands(IEnumerable<Tuple<Sweeper, Direction>> commandlist)
        {
            foreach (var item in commandlist)
            {
                item.Item1.Position = item.Item1.Position.Next(item.Item2);
            }
        }

        public IEnumerable<Tuple<Sweeper, Direction>> AdjustForCollisions(List<Tuple<Sweeper, Direction>> commandlist)
        {
            var dict = commandlist.ToDictionary(x => x.Item1, x => x.Item2);

            var items = dict.Select(x =>
                new
                {
                    Sweeper = x.Key,
                    OldPosition = x.Key.Position,
                    NewPosition = x.Key.Position.Next(x.Value),
                    Direction = x.Value,
                });

            foreach (var item in items.ToList())
            {
                if (item.NewPosition.X < 0 || item.NewPosition.X >= Width || item.NewPosition.Y < 0 || item.NewPosition.Y >= Width)
                {
                    dict[item.Sweeper] = Direction.None;
                }
            }

            while (true)
            {
                var collisions = (from item in items group item by item.NewPosition into g where g.Count() > 1 select g).ToList();
                if (collisions.Count == 0) break;
                foreach (var collision in collisions)
                {
                    foreach (var item in collision)
                    {
                        dict[item.Sweeper] = Direction.None;
                    }
                }
            }

            return dict.Select(x => new Tuple<Sweeper, Direction>(x.Key, x.Value));

        }

        //Returns an ienumerable for each sweeper in the list
        public IEnumerable<Tuple<Sweeper, Direction>> GetSweeperCommands()
        {
            lock (sweepers)
            {
                XElement state = State;
                var ret = sweepers.Select((sweeper,index) =>
                    new
                    {
                        Sweeper = sweeper,
                        Task = Task.Run(() =>
                        {
                            if (sweeper.MoveDelegate == null)
                                return Direction.None;
                            else return sweeper.MoveDelegate(state, timeout);
                        })
                    }).ToList();
                Task.WaitAll(ret.Select(x => x.Task).ToArray(), timeout);

                return ret.Select(x =>
                    {
                        Direction retdir = Direction.None;
                        if (x.Task.IsCompleted) retdir = x.Task.Result;
                        return new Tuple<Sweeper, Direction>(x.Sweeper, retdir);
                    });
            }
        }

        public XElement State
        {
            get
            {
                return new XElement("state",
                    new XAttribute("maxtimeout", (int)timeout.TotalMilliseconds),
                    new XElement("mines",
                        mines.Select(mine => new XElement("mine",
                            new XAttribute("x", mine.Position.X),
                            new XAttribute("y", mine.Position.Y)))),
                    new XElement("sweepers",
                        sweepers.Select(sweeper => new XElement("sweeper",
                            new XAttribute("id", sweeper.Id),
                            new XAttribute("x", sweeper.Position.X),
                            new XAttribute("y", sweeper.Position.Y)))));
            }
        }




    }
}
