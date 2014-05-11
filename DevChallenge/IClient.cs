using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevChallenge
{
    /// <summary>
    /// Represents a devchallenge client. 
    /// This is the starting point for any client wanting to connect to the server.
    /// 
    /// </summary>
    public interface IClient
    {
        /// <summary>
        /// Specifies the server name
        /// </summary>
        string ServerName { get; }
        /// <summary>
        /// Specifies the server port
        /// </summary>
        int ServerPort { get; }

        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="fullname"></param>
        /// <param name="email"></param>
        void RegisterUser(string username, string password, string fullname, string email);

        /// <summary>
        /// Establishes a new connection to the server, binding the connection to the specific user and agent
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="agentname"></param>
        /// <param name="agentrevision"></param>
        /// <returns></returns>
        IConnection Login(string username, string password, string agentname, int agentrevision = 0);
    }
}
