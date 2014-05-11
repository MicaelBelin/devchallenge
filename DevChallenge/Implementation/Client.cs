using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DevChallenge.Implementation
{
    public class Client : IClient
    {
        public string ServerName { get; set;}
        public int ServerPort { get { return serverport; } set { serverport = value; } }
        private static int serverport = 8231;

        public void RegisterUser(string username, string pwhash, string fullname, string email)
        {
            if (String.IsNullOrWhiteSpace(ServerName) || ServerPort == 0) throw new UninitializedClientException();
            var c = new TcpConnection(new System.Net.Sockets.TcpClient(ServerName, ServerPort));

            var ret = c.SendRequest(new XElement("user.register",
                new XElement("login", username),
                new XElement("digest", pwhash),
                new XElement("fullname", fullname),
                new XElement("email", email)));

            if (ret.Name != "ok") throw new RegisterUserException(ret.Attribute("message").Value); 
        }


        public IConnection Login(string username, string pwdigest, string agentname, int agentrevision = 0)
        {
            if (String.IsNullOrWhiteSpace(ServerName) || ServerPort == 0) throw new UninitializedClientException();
            var c = new TcpConnection(new System.Net.Sockets.TcpClient(ServerName, ServerPort));
            var response = c.SendRequest(new XElement("devchallenge.login",
                new XElement("login",username),
                new XElement("digest",pwdigest),
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
