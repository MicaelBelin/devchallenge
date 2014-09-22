using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DevChallenge.Server.Scenarios.MineSweeper
{
    public class ClientFramework
    {

	    public int Ticks {get; private set;}
	    public int MinesCollected {get; private set;}	
	    public int Width {get; private set;}
	    public int Height {get;private set;}
        public int MyId { get; private set; }
        public int MaxMineCount { get; private set; }
        public int MineRegenerationTime { get; private set; }

        public void DrawCurrentState(Bitmap bm)
        {
            if (Width == 0 || Height == 0) return;

            using (var g = Graphics.FromImage(bm))
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 255)), 0, 0, bm.Width, bm.Height);

                int squarewidth = (bm.Width - 1) / Width;
                int squareheight = (bm.Height - 1) / Height;


                Action<ClientFramework.Position, Color> DrawSquare = (p, c) =>
                {
                    g.FillRectangle(new SolidBrush(c), p.X * squarewidth, p.Y * squareheight, squarewidth, squareheight);
                };

                if (minepositions != null)
                    foreach (var mine in minepositions) DrawSquare(mine, Color.FromArgb(0, 0, 0));

                if (othersweeperpositions != null)
                    foreach (var sweeper in othersweeperpositions) DrawSquare(sweeper, Color.FromArgb(255, 0, 0));

                if (myposition != null)
                    DrawSquare(myposition, Color.FromArgb(0, 0, 255));


                for (int num = 0; num <= Width; ++num) g.DrawLine(new Pen(Color.FromArgb(0, 0, 0), 1), num * squarewidth, 0, num * squarewidth, squareheight * Height);
                for (int num = 0; num <= Height; ++num) g.DrawLine(new Pen(Color.FromArgb(0, 0, 0), 1), 0, num * squareheight, squarewidth * Width, num * squareheight);
            }
        }

        List<Position> minepositions;
        Position myposition;
        List<Position> othersweeperpositions;

        public double MineToTickRatio
	    {
		    get
		    {
			    return (double)MinesCollected/(double)Ticks;
		    }
	    }
	    public DevChallenge.IConnection Connection {get; private set;}
	
	
	    public delegate Direction ProcessDelegate(Position MyPosition, List<Position> MinePositions, List<Position> SweeperPositions);
	    public ProcessDelegate CalculateDirection {get; set;}
	    public Action OnMineCollected {get; set;}
        public Action<Position> OnMineCreated { get; set; }


	    public ClientFramework(DevChallenge.IConnection connection)
	    {
		    Connection = connection;

            connection.RegisterFilter(raw =>
            {
                var notification = connection.GetNotification(raw);
                if (notification.Message.Name != "minecollected") return DevChallenge.FilterResponse.PassToNext;
                MinesCollected++;
                if (OnMineCollected != null) OnMineCollected();
                return DevChallenge.FilterResponse.Consume;
            });

            connection.RegisterFilter(raw =>
            {
                var notification = connection.GetNotification(raw);
                if (notification.Message.Name != "minecreated") return DevChallenge.FilterResponse.PassToNext;
                if (OnMineCreated != null) OnMineCreated(new Position(
                    Convert.ToInt32(notification.Message.Attribute("x").Value),
                    Convert.ToInt32(notification.Message.Attribute("y").Value)
                    ));
                return DevChallenge.FilterResponse.Consume;
            });

            connection.RegisterFilter(raw =>
		    {
			    var req = connection.GetRequest(raw);
			    if (req.Message.Name != "state") return DevChallenge.FilterResponse.PassToNext;

			    Ticks++;
			
			
			    minepositions = req.Message.Element("mines").Elements("mine").Select(x=>
			    new Position(Convert.ToInt32(x.Attribute("x").Value),Convert.ToInt32(x.Attribute("y").Value))).ToList();

			
			    var sweepers = req.Message.Element("sweepers").Elements("sweeper").Select(x=>
			    new
			    {
				    Id = Convert.ToInt32(x.Attribute("id").Value),
				    Position = new Position(Convert.ToInt32(x.Attribute("x").Value),Convert.ToInt32(x.Attribute("y").Value)),
			    }).ToDictionary(x=>x.Id,x=>x.Position);
			

			
			    myposition = sweepers[MyId];
			    othersweeperpositions = sweepers.Where(x=>x.Key!=MyId).Select(x=>x.Value).ToList();
			
			
			    Direction ret = Direction.None;
			    if (CalculateDirection != null)
			    {
				    ret = CalculateDirection(myposition,minepositions,othersweeperpositions);
			    }
						
			
			    connection.SendResponse(new XElement("direction",ret.ToString()),req.RequestId);
			
			    return DevChallenge.FilterResponse.Consume;
		    });
		
	    }
	
	    public void Start()
	    {
		    var lobby = new DevChallenge.ConnectionInterface.Lobby(Connection);			
		    lobby.Join("minesweeper");
		
		    Connection.WaitForNotification(notif=>
		    {
			    if (notif.Message.Name!="init") return DevChallenge.FilterResponse.PassToNext;
			    MyId = Convert.ToInt32(notif.Message.Attribute("sweeperid").Value);
                Width = Convert.ToInt32(notif.Message.Attribute("worldwidth").Value);
                Height = Convert.ToInt32(notif.Message.Attribute("worldheight").Value);
                MaxMineCount = Convert.ToInt32(notif.Message.Attribute("maxminecount").Value);
                MineRegenerationTime = Convert.ToInt32(notif.Message.Attribute("mineregenerationtime").Value);
			    return DevChallenge.FilterResponse.Consume;
		    });
		
		    Connection.RunUntilClosed();
	    }

	
	    public enum Direction
	    {
		    Up,
		    Down,
		    Left,
		    Right,
		    None
	    }
	
	

        public class Position
        {
            public int X { get; set; }
            public int Y { get; set; }

            public Position(int x, int y)
            {
                X = x;
                Y = y;
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
                if (object.ReferenceEquals(a,null) && object.ReferenceEquals(b,null)) return true;
                if (object.ReferenceEquals(a,null) || object.ReferenceEquals(b,null)) return false;
                return a.X == b.X && a.Y == b.Y;
            }

            public static bool operator !=(Position a, Position b)
            {
                if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return false;
                if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return true;
                return a.X != b.X || a.Y != b.Y;
            }

        }



    }
}
