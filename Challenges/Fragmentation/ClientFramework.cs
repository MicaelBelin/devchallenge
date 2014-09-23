using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DevChallenge.Server.Scenarios.Fragmentation
{
    public class ClientFramework
    {
        public DevChallenge.IConnection Connection { get; private set; }

        public int Capacity { get; private set; }
        public event Action<int> CapacityChanged;
        public int Size
        {
            get
            {
                return cratesize.Aggregate(0, (src, next) => src + next.Value);
            }
        }
        public event Action<int> SizeChanged;
        public event Action<string> ErrorMessage;

        public delegate IEnumerable<ICommand> InsertDelegate(int crateid, int cratesize);
        public delegate IEnumerable<ICommand> RemoveDelegate(int crateid);
        public InsertDelegate OnInsert { get; set; }
        public RemoveDelegate OnRemove { get; set; }



        public IReadOnlyDictionary<int, int> CratePositions { get { return crateposition; } }
        public IReadOnlyDictionary<int, int> CrateSizes { get { return cratesize; } }


        Dictionary<int, int> crateposition = new Dictionary<int, int>();
        Dictionary<int, int> cratesize = new Dictionary<int, int>();


        public IList<bool> GetFillStatus()
        {
            var ret = new List<bool>(Capacity);
            ret.AddRange(Enumerable.Repeat(false, Capacity));

            foreach (var id in crateposition.Keys)
            {
                foreach (var index in Enumerable.Range(crateposition[id], cratesize[id]))
                    if (index >= 0 && index < Capacity)
                        ret[index] = true;
            }
            return ret;
        }

        public static bool CanFit(IList<bool> fillstatus, int pos, int size)
        {
            return Enumerable.Range(pos, size).All(index => !fillstatus[index]);
        }

        public void DrawStatus(Bitmap bm)
        {
            var fillstatus = GetFillStatus();

            using (var g = Graphics.FromImage(bm))
            {
                g.FillRectangle(Brushes.White, new Rectangle(0, 0, bm.Width, bm.Height));

                double scale = (double)Capacity / (double)bm.Width;
                for (int x = 0; x < bm.Width; ++x)
                {
                    int startpos = (int)(scale * (double)x);
                    int endpos = (int)(scale * (double)(x+1));
                    int fillratio = Enumerable.Range(startpos, endpos - startpos).Count(index => fillstatus[index]);
                    double alpha = (double)fillratio / (double)(endpos - startpos);
                    g.DrawLine(new Pen(Color.FromArgb((int)(alpha * 255.0), 0, 200, 0), 1), x, 0, x, bm.Height);
                }
                g.DrawRectangle(Pens.Black, new Rectangle(0, 0, bm.Width-1, bm.Height-1));
            }
        }
       
        public ClientFramework(DevChallenge.IConnection connection)
        {
            Connection = connection;
        }


        public interface ICommand
        {
            XElement Serialized { get; }
            void AdjustPositionAndSize(IDictionary<int, int> positions, IDictionary<int, int> sizes);
        }

        public class Insert : ICommand
        {
            public int Id { get; private set; }
            public int Position { get; private set; }

            public Insert(int id, int position)
            {
                Id = id;
                Position = position;
            }

            public XElement Serialized
            {
                get
                {
                    return new XElement("insert",
                    new XAttribute("id", Id),
                    new XAttribute("position", Position)
                    );
                }
            }

            public void AdjustPositionAndSize(IDictionary<int, int> positions, IDictionary<int, int> sizes)
            {
                positions[Id] = Position;
            }
        }

        public class Remove : ICommand
        {
            public int Id { get; private set; }

            public Remove(int id)
            {
                Id = id;
            }

            public XElement Serialized
            {
                get
                {
                    return new XElement("remove",
                    new XAttribute("id", Id)
                    );
                }
            }

            public void AdjustPositionAndSize(IDictionary<int, int> positions, IDictionary<int, int> sizes)
            {
                positions.Remove(Id);
                sizes.Remove(Id);
            }

        }

        public class Move : ICommand
        {
            public int Id { get; private set; }
            public int Position { get; private set; }

            public Move(int id, int newposition)
            {
                Id = id;
                Position = newposition;
            }

            public XElement Serialized
            {
                get
                {
                    return new XElement("move",
                    new XAttribute("id", Id),
                    new XAttribute("position", Position)
                    );
                }
            }

            public void AdjustPositionAndSize(IDictionary<int, int> positions, IDictionary<int, int> sizes)
            {
                positions[Id] = Position;
            }
        }


        void SendCommands(string requestid, ICollection<ICommand> commands)
        {
            foreach (var command in commands)
            {
                command.AdjustPositionAndSize(crateposition,cratesize);
            }
            Connection.SendResponse(
            new XElement("commands",
            commands.Select(command => command.Serialized))
            , requestid);
        }

        public void RunUntilClosed()
        {
            if (OnInsert == null || OnRemove == null) throw new NotImplementedException("Please bind OnInsert and OnRemove to implementations");

            var lobby = new DevChallenge.ConnectionInterface.Lobby(Connection);

            Connection.RegisterFilter(e =>
            {
                var notification = Connection.GetNotification(e);
                if (notification.Message.Name != "capacity") return FilterResponse.PassToNext;

                Capacity = Convert.ToInt32(notification.Message.Value);
                if (CapacityChanged != null) CapacityChanged(Capacity);

                return FilterResponse.Consume;
            });

            Connection.RegisterFilter(e =>
            {
                var notification = Connection.GetNotification(e);
                if (notification.Message.Name != "error") return FilterResponse.PassToNext;

                if (ErrorMessage != null)
                    ErrorMessage(notification.Message.Value);

                return FilterResponse.Consume;
            });

            Connection.RegisterFilter(e =>
            {
                var request = Connection.GetRequest(e);
                if (request.Message.Name != "insertrequest") return DevChallenge.FilterResponse.PassToNext;

                if (OnInsert != null)
                {
                    int id = Convert.ToInt32(request.Message.Attribute("id").Value);
                    int size = Convert.ToInt32(request.Message.Attribute("size").Value);
                    var commands = OnInsert(id,size).ToList();

                    if (SizeChanged != null) SizeChanged(Size);

                    cratesize[id] = size;
                    SendCommands(request.RequestId, commands);

                    

                }

                return DevChallenge.FilterResponse.Consume;
            });

            Connection.RegisterFilter(e =>
            {
                var request = Connection.GetRequest(e);
                if (request.Message.Name != "removerequest") return DevChallenge.FilterResponse.PassToNext;

                if (OnRemove != null)
                {
                    int id = Convert.ToInt32(request.Message.Attribute("id").Value);
                    var commands = OnRemove(id).ToList();

                    if (SizeChanged != null) SizeChanged(Size);

                    SendCommands(request.RequestId, commands);

                }


                return DevChallenge.FilterResponse.Consume;
            });



            lobby.Join("fragmentation");
            Connection.RunUntilClosed();

        }

    }
}
