using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test_DevChallenge
{
    [TestClass]
    public class Test_TcpConnection
    {

        static int port = 9845;
        static System.Net.Sockets.TcpListener listener;


        [ClassInitialize()]
        public static void Initialize(TestContext ctx) 
        {
            listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, port);
            listener.Start();
        }

        [ClassCleanup()]
        public static void Cleanup() 
        {
 
        }


        public Tuple<DevChallenge.Connection.Tcp, DevChallenge.Connection.Tcp> CreatePair()
        {



            var server = Task<DevChallenge.Connection.Tcp>.Run(() =>
            {
                var c = listener.AcceptTcpClient();
                return new DevChallenge.Connection.Tcp(c);
            });

            var client = new DevChallenge.Connection.Tcp(new System.Net.Sockets.TcpClient("localhost", port));

            return new Tuple<DevChallenge.Connection.Tcp, DevChallenge.Connection.Tcp>(server.Result, client);

        }

        [TestMethod]
        public void SendAndReceiveNotification()
        {

            var pair = CreatePair();

            pair.Item1.SendNotification(new System.Xml.Linq.XElement("hej"));

            bool dorun = true;
            pair.Item2.RegisterFilter(e =>
            {
                try
                {
                    var n = DevChallenge.Connection.Tcp.Notification.Get(e);
                    Assert.AreEqual("hej", n.Message.Name);
                }
                catch (Exception)
                {
                    Assert.Fail();
                }
                dorun = false;
                return DevChallenge.FilterResponse.Consume;
            });

            pair.Item2.ExecWhile(() => dorun);

        }

        [TestMethod]
        public void RunUntilClosed()
        {

            var pair = CreatePair();

            var task = Task.Run(() =>
                {
                    pair.Item1.RunUntilClosed();
                });

            pair.Item2.Close(); //Sever the connection

            Assert.AreEqual(true, task.Wait(5000));
        }

        [TestMethod]
        [ExpectedException(typeof(DevChallenge.ClosedException))]
        public void ThrowIfConnectionIsClosed_Send()
        {
            var pair = CreatePair();
            pair.Item1.Close();
            pair.Item1.SendNotification(new System.Xml.Linq.XElement("hej"));
        }

        [TestMethod]
        [ExpectedException(typeof(DevChallenge.ClosedException))]
        public void ThrowIfConnectionIsClosed_Receive()
        {
            var pair = CreatePair();
            pair.Item1.Close();
            pair.Item2.SendRequest(new System.Xml.Linq.XElement("hej"));
        }

        [TestMethod]
        [ExpectedException(typeof(DevChallenge.ClosedException))]
        public void ThrowIfConnectionIsClosed_Exec()
        {
            var pair = CreatePair();
            pair.Item1.Close();
            pair.Item2.Exec(TimeSpan.FromSeconds(1));
        }

        [TestMethod]
        public void SendRequestAndResponse()
        {
            var pair = CreatePair();

            var task = Task.Run(() =>
                {
                    bool dorun = true;
                    pair.Item2.RegisterFilter(e =>
                        {
                            var req = pair.Item2.GetRequest(e);
                            Assert.AreEqual("ping", req.Message.Name);
                            pair.Item2.SendResponse(new System.Xml.Linq.XElement("pong"), req.RequestId);
                            dorun = false;
                            return DevChallenge.FilterResponse.Consume;
                        });

                    pair.Item2.ExecWhile(() => dorun);
                });


            var ret = pair.Item1.SendRequest(new System.Xml.Linq.XElement("ping"));
            Assert.AreEqual("pong", ret.Name);

            Assert.AreEqual(true, task.Wait(5000));

        }
        
    

    }
}
