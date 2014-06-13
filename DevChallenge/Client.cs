using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DevChallenge
{
    public class Client : IClient
    {
        public string ServerName { get; set;}
        public int ServerPort { get { return serverport; } set { serverport = value; } }
        private static int serverport = 8231;


        public Client(string servername, int port = 8231)
        {
            ServerName = servername;
            ServerPort = port;
        }

        public void RegisterUser(string username, string password, string fullname, string email)
        {
            if (String.IsNullOrWhiteSpace(ServerName) || ServerPort == 0) throw new UninitializedClientException();
            var c = new Connection.Tcp(new System.Net.Sockets.TcpClient(ServerName, ServerPort));
            ApplyMonitors(c);

            var ret = c.SendRequest(new XElement("user.register",
                new XElement("login", username),
                new XElement("password", password),
                new XElement("fullname", fullname),
                new XElement("email", email)));

            if (ret.Name != "ok") throw new RegisterUserException(ret.Attribute("message").Value); 
        }

        public void RegisterMonitor(Action<XElement, MessageDirection> me)
        {
            monitors.Add(me);
        }
        public void UnregisterMonitor(Action<XElement, MessageDirection> me)
        {
            monitors.Remove(me);
        }
        List<Action<XElement,MessageDirection>> monitors = new List<Action<XElement,MessageDirection>>();

        void ApplyMonitors(IConnection conn)
        {
            foreach (var monitor in monitors)
                conn.RegisterMonitor(monitor);
        }


        public IConnection Login(string username, string password, string agentname, int agentrevision = 0)
        {
            if (String.IsNullOrWhiteSpace(ServerName) || ServerPort == 0) throw new UninitializedClientException();
            var c = new Connection.Tcp(new System.Net.Sockets.TcpClient(ServerName, ServerPort));
            ApplyMonitors(c);

            c.WaitForNotification(notification =>
                {
                    if (notification.Message.Name != "devchallenge") return FilterResponse.PassToNext;

                    if (notification.Message.Attribute("version").Value != "1.1") throw new Exception("Version mismatch");

                    return FilterResponse.Consume;
                });

            var response = c.SendRequest(new XElement("devchallenge.login",
                new XElement("login",username),
                new XElement("password",password),
                new XElement("agentname",agentname),
                new XElement("agentrevision",agentrevision)));

            if (response.Name != "ok")
            {
                throw new LoginException(response.Attribute("message").Value);
            }
            return c;
        }

        public class RegisterUserException : Exception
        {
            public RegisterUserException(string msg) : base(msg) { }
        }

        public class LoginException : Exception
        {
            public LoginException(string msg) : base(msg) { }
        }

        public class UninitializedClientException : Exception
        {
            public UninitializedClientException() : base("ServerName or ServerPort properties in Client class are not set correctly") { }
        }
    }
}
