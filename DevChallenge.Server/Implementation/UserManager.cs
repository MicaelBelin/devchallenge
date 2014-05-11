using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DevChallenge.Server.Model;
using System.Data;
using System.Data.Linq;

namespace DevChallenge.Server.Implementation
{
    public class UserManager : IUserManager
    {
        db.DevChallengeDataContext dbc;

        public UserManager(db.DevChallengeDataContext DataContext)
        {
            dbc = DataContext;
        }


        public IUser Get(int UserId)
        {
            var dbitem = dbc.Users.Single(x => x.Id == UserId);
            dbc.Refresh(RefreshMode.KeepChanges,dbitem);
            return new User(dbitem, new db.DevChallengeDataContext(dbc.Connection.ConnectionString));
        }

        public IUser Get(string Username, string password)
        {
            var exception = new AccessViolationException("Invalid login criteria");

            var entry = dbc.Users.Where(x => x.Login == Username).FirstOrDefault();
            if (entry == null) throw exception;

            var ret = new User(entry,new db.DevChallengeDataContext(dbc.Connection.ConnectionString));

            if (!ret.VerifyPassword(password)) throw exception;

            return ret;


        }

        public IUser Create(string Username, string password, string FullName, string Email)
        {
            if (dbc.Users.Any(x => x.Login == Username))
            {
                throw new InvalidOperationException("User already exists");
            }
            var rand = new Random();
            var crypto = new SHA1CryptoServiceProvider();
            var salt = Convert.ToBase64String(crypto.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rand.Next().ToString())));
            var newentry = new db.User
            {
                Login = Username,
                PwDigest = Convert.ToBase64String(crypto.ComputeHash(System.Text.Encoding.UTF8.GetBytes(salt + password ?? ""))),
                FullName = FullName,
                Email = Email,
                Salt = salt,
            };
            dbc.Users.InsertOnSubmit(newentry);
            dbc.SubmitChanges();
            return Get(Username, password);
        }

        public void RegisterUserCommands(IConnection connection)
        {

            //function filter for register new user
            connection.RegisterFilter(e =>
            {
                var request = connection.GetRequest(e);
                if (request.Message.Name != "user.register") return DevChallenge.FilterResponse.PassToNext;

                var newr = new
                {
                    login = request.Message.Element("login").Value,
                    digest = request.Message.Element("digest").Value,
                    fullname = request.Message.Element("fullname").Value,
                    email = request.Message.Element("email").Value,
                };


                try
                {
                    Create(newr.login, newr.digest, newr.fullname, newr.email);
                    connection.SendResponse(new XElement("ok"), request.RequestId);
                }
                catch (Exception exception)
                {
                    connection.SendResponse(new XElement("error",
                        new XAttribute("message", exception.Message))
                        , request.RequestId);
                }
                return DevChallenge.FilterResponse.Consume;
            });
        }

    }
}
