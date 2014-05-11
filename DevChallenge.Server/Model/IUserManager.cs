using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevChallenge.Server.Model
{
    public interface IUserManager
    {
        void RegisterUserCommands(IConnection connection);

        IUser Get(int UserId);
        IUser Get(string Username, string password); //throws AccessViolationException if username/password criteria mismatch
        IUser Create(string Username, string password, string fullname, string email); //throws InvalidOperationException if username is taken
    }
}
