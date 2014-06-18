using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DevChallenge.Server.Model;

namespace DevChallenge.Server.Implementation
{
    public class TcpServer : Model.Agent.ISource
    {
        string connectionstring;
        int port;
        public TcpServer(int port, string connectionstring)
        {
            this.port = port;
            this.connectionstring = connectionstring;
        }

        //Blocking
        public void Exec()
        {
            var server = new TcpListener(IPAddress.Any, port);
            server.Start();

            //Handles acception of connections
            while (true)
            {
                var client = server.AcceptTcpClient();
                client.NoDelay = true;
                Task.Run(()=>HandleClient(client));
            }
        }

        //Blocking until connection is closed
        void HandleClient(TcpClient client)
        {
            try
            {
                var connection = new DevChallenge.Connection.Tcp(client);
                var usermanager = new UserManager(new db.DevChallengeDataContext(connectionstring));
                usermanager.RegisterUserCommands(connection);





                //function filter for login
                connection.RegisterFilter(e =>
                {
                    var request = connection.GetRequest(e);
                    if (request.Message.Name != "devchallenge.login") return FilterResponse.PassToNext;


                    var criteria = new
                    {
                        login = request.Message.Element("login").Value,
                        password = request.Message.Element("password").Value,
                        agentname = request.Message.Element("agentname").Value,
                        agentrevision = Convert.ToInt32(request.Message.Element("agentrevision").Value),
                    };


                    try
                    {
                        if (String.IsNullOrWhiteSpace(criteria.agentname)) throw new ArgumentException("invalid agent name");

                        IUser user = usermanager.Get(criteria.login,criteria.password);
                        var agent = new Agent(connection,user,criteria.agentname,criteria.agentrevision);
                        connection.SendResponse(new XElement("ok"), request.RequestId);

                        if (AgentSpawned != null) AgentSpawned(agent);
                    }
                    catch (Exception exception)
                    {
                        connection.SendResponse(new XElement("error", 
                            new XAttribute("message",exception.Message))
                            ,request.RequestId);
                    }
                    return FilterResponse.Consume;
                });



                //send greeting
                connection.SendNotification(new XElement("devchallenge",
                    new XAttribute("version", "1.1")
                    ));

                connection.RunUntilClosed();

            }
            catch (TimeoutException)
            {
            }
            catch (ClosedException)
            {
            }

        }


        public event Action<IAgent> AgentSpawned;
    }
}
