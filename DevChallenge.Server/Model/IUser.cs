using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevChallenge.Server.Model
{
    public interface IUser
    {
        int Id { get; }
        string UserName { get; }
        string FullName { get; }
        string Email { get; }
        bool VerifyPassword(string pw);
        void ChangePassword(string pw);
    }
}
