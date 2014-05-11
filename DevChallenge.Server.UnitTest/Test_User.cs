using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Test_DevChallengeServer
{
    /// <summary>
    /// Summary description for Test_User
    /// </summary>
    [TestClass]
    public class Test_User
    {
        public Test_User()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        static FakeDataContext DataContext;

        [ClassInitialize()]
        public static void Initialize(TestContext testContext) 
        {
            DataContext = new FakeDataContext();
        }
       
        [ClassCleanup()]
        public static void Shutdown() 
        {
            DataContext.DeleteDatabase();   
        }

        [TestCleanup()]
        public void Clear() 
        {
//            DataContext.Clear();
        }


        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        //
        // Use ClassCleanup to run code after all tests in a class have run
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void GetUser()
        {
            lock (DataContext)
            {
                var dbuser = new DevChallenge.Server.db.User() { Login = "testuser", FullName = "test user", Email = "testuser@test.com" };
                DataContext.Users.InsertOnSubmit(dbuser);
                DataContext.SubmitChanges();

                var um = new DevChallenge.Server.Implementation.UserManager(DataContext);
                var user = um.Get(dbuser.Id);

                Assert.IsNotNull(user);
                Assert.AreEqual(dbuser.Id, user.Id);
                Assert.AreEqual("testuser", user.UserName);
                Assert.AreEqual("test user", user.FullName);
                Assert.AreEqual("testuser@test.com", user.Email);
                DataContext.Clear();
            }
            //
            // TODO: Add test logic here
            //
        }

        [TestMethod]
        public void CreateUser()
        {
            lock (DataContext)
            {
                var um = new DevChallenge.Server.Implementation.UserManager(DataContext);
                var reference = um.Create("testuser", "password", "fullname", "email");

                Assert.AreEqual("testuser", reference.UserName);
                Assert.AreEqual("fullname", reference.FullName);
                Assert.AreEqual("email", reference.Email);
                Assert.IsTrue(reference.VerifyPassword("password"));

                var user = um.Get(reference.Id);

                Assert.IsNotNull(user);
                Assert.AreEqual(reference.UserName, user.UserName);
                Assert.AreEqual(reference.FullName, user.FullName);
                Assert.AreEqual(reference.Email, user.Email);
                
                user = um.Get(reference.UserName,"password");
                Assert.IsNotNull(user);
                Assert.AreEqual(reference.UserName, user.UserName);
                Assert.AreEqual(reference.FullName, user.FullName);
                Assert.AreEqual(reference.Email, user.Email);
                DataContext.Clear();
            }
            
        }

        [TestMethod]
        public void SetAndVerifyPassword()
        {
            lock (DataContext)
            {

                var um = new DevChallenge.Server.Implementation.UserManager(DataContext);
                var reference = um.Create("testuser", "password", "fullname", "email");

                Assert.IsTrue(reference.VerifyPassword("password"));
                Assert.IsFalse(reference.VerifyPassword("incorrect"));

                reference.ChangePassword("newpassword");

                Assert.IsTrue(reference.VerifyPassword("newpassword"));

                var user = um.Get(reference.Id);

                Assert.IsTrue(user.VerifyPassword("newpassword"));
                DataContext.Clear();
            }
        }
    }
}
