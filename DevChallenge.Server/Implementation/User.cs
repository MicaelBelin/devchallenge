using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DevChallenge.Server.Model;

namespace DevChallenge.Server.Implementation
{
    public class User : IUser
    {
        int userid;
        string digest, salt;

        string connectionstring;
        public User(db.User user, db.DevChallengeDataContext datacontext)
        {
            userid = user.Id;
            UserName = user.Login;
            FullName = user.FullName;
            digest = user.PwDigest;
            salt = user.Salt;
            Email = user.Email;
            DataContext = datacontext;
        }

        public db.DevChallengeDataContext DataContext { get; private set; }

        public string UserName {get; private set;}
        public string FullName {get; private set;}
        public bool VerifyPassword(string pw)
        {
            var crypto = new SHA1CryptoServiceProvider();
            var hashhash = Convert.ToBase64String(crypto.ComputeHash(System.Text.Encoding.UTF8.GetBytes(salt + pw ?? "")));
            return hashhash == digest;
        }

        public string Email {get; private set;}

        public void ChangePassword(string pw)
        {
            db.User user = null;
            try
            {
                user = DataContext.Users.Single(x => x.Id == userid);
            }
            catch (Exception)
            {
                throw new NullReferenceException("Unable to find user in database");
            }


            var rand = new Random();
            var crypto = new SHA1CryptoServiceProvider();
            user.Salt = Convert.ToBase64String(crypto.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rand.Next().ToString())));
            user.PwDigest = Convert.ToBase64String(crypto.ComputeHash(System.Text.Encoding.UTF8.GetBytes(user.Salt + pw ?? "")));
            salt = user.Salt;
            digest = user.PwDigest;
            DataContext.SubmitChanges();
        }

        public int Id
        {
            get { return userid; }
        }
    }
}
