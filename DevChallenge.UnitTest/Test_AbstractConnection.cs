using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test_DevChallenge
{
    [TestClass]
    public class Test_AbstractConnection
    {

        class DelegatedConnection : DevChallenge.Connection.Abstract
        {
            protected override System.Collections.Generic.IEnumerable<System.Xml.Linq.XElement> ReadXmlItems()
            {
                if (ReadXmlItemsDelegate == null) throw new NotImplementedException();
                return ReadXmlItemsDelegate();
            }

            protected override void SendXmlItem(System.Xml.Linq.XElement e)
            {
                if (SendXmlItemDelegate == null) throw new NotImplementedException();
                SendXmlItemDelegate(e);
            }

            public override void Close()
            {
                if (CloseDelegate == null) throw new NotImplementedException();
                CloseDelegate();
            }

            public Action CloseDelegate {get;set;}
            public Func<IEnumerable<System.Xml.Linq.XElement>> ReadXmlItemsDelegate { get; set; }
            public Action<XElement> SendXmlItemDelegate { get; set; }
        }

        IEnumerable<XElement> IndefinateExecQueue()
        {
            Thread.Sleep(Timeout.Infinite);
            yield break;
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public void SendRequest_Timeout()
        {
            DelegatedConnection connection = new DelegatedConnection()
            {
                SendXmlItemDelegate = e => { },
                ReadXmlItemsDelegate = IndefinateExecQueue
            };
            connection.StartCollector();
            connection.SendRequest(new XElement("test"), TimeSpan.FromMilliseconds(500));
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public void WaitForRequest_Timeout()
        {
            DelegatedConnection connection = new DelegatedConnection()
            {
                SendXmlItemDelegate = e => { },
                ReadXmlItemsDelegate = IndefinateExecQueue
            };
            connection.StartCollector();
            connection.WaitForRequest(req => DevChallenge.FilterResponse.Consume, TimeSpan.FromMilliseconds(1));
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public void WaitForNotification_Timeout()
        {
            DelegatedConnection connection = new DelegatedConnection()
            {
                SendXmlItemDelegate = e => { },
                ReadXmlItemsDelegate = IndefinateExecQueue
            };
            connection.StartCollector();
            connection.WaitForNotification(notification => DevChallenge.FilterResponse.Consume, TimeSpan.FromMilliseconds(1));
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public void Exec_Timeout()
        {
            DelegatedConnection connection = new DelegatedConnection()
            {
                SendXmlItemDelegate = e => { },
                ReadXmlItemsDelegate = IndefinateExecQueue
            };
            connection.StartCollector();
            connection.Exec(TimeSpan.FromMilliseconds(1));
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public void ExecWhile_Timeout()
        {
            DelegatedConnection connection = new DelegatedConnection()
            {
                SendXmlItemDelegate = e => { },
                ReadXmlItemsDelegate = IndefinateExecQueue
            };
            connection.StartCollector();
            connection.ExecWhile(()=>true,TimeSpan.FromMilliseconds(1));
        }



        IEnumerable<XElement> WaitWithoutRunningExecQueue()
        {
            yield return new DevChallenge.Connection.Abstract.Notification(new XElement("notification1")).Serialized;
            yield return new DevChallenge.Connection.Abstract.Notification(new XElement("notification2")).Serialized;
        }

        [TestMethod]
        public void WaitUntilFilterMatch_NoRunningExec()
        {
            var connection = new DelegatedConnection();

            connection.ReadXmlItemsDelegate = WaitWithoutRunningExecQueue;

            connection.StartCollector();


            bool isok = false;
            connection.WaitUntilFilterMatch(e =>
            {
                var notif = connection.GetNotification(e);
                if (notif.Message.Name != "notification2") return DevChallenge.FilterResponse.PassToNext;
                isok = true;
                return DevChallenge.FilterResponse.Consume;
            });
            Assert.AreEqual(true, isok);
        }

        [TestMethod]
        public void WaitUntilFilterMatch_NestedExec()
        {
            var connection = new DelegatedConnection();

            connection.ReadXmlItemsDelegate = WaitWithoutRunningExecQueue;

            connection.StartCollector();

            bool isok = false;

            connection.WaitUntilFilterMatch(e =>
            {
                var notif = connection.GetNotification(e);
                if (notif.Message.Name != "notification1") return DevChallenge.FilterResponse.PassToNext;
                connection.WaitUntilFilterMatch(e2=>
                    {
                        var notif2 = connection.GetNotification(e2);
                        if (notif2.Message.Name != "notification2") return DevChallenge.FilterResponse.PassToNext;
                        isok = true;
                        return DevChallenge.FilterResponse.Consume;
                    });
                return DevChallenge.FilterResponse.Consume;
            });

            Assert.AreEqual(true, isok);
        }







        [TestMethod]
        public void SendRequest_InterThread()
        {
            var connection = new DelegatedConnection();

            var queue = new DevChallenge.Util.AsyncQueue<XElement>();
            connection.ReadXmlItemsDelegate = () => queue.AsEnumerable();
            connection.SendXmlItemDelegate = item => queue.Add(item);


            connection.CloseDelegate = () => queue.Close();

            connection.StartCollector();


            bool gotresponse = false;


            connection.RegisterFilter(raw =>
                {
                    var r = connection.GetRequest(raw);
                    connection.SendResponse(new XElement("answer"), r.RequestId);
                    return DevChallenge.FilterResponse.Consume;
                });

            var task = Task.Run(() =>
                {
                    gotresponse = connection.SendRequest(new XElement("question")).Name == "answer";
                    connection.Close();
                });

            try
            {
                connection.Exec(TimeSpan.FromSeconds(10));
            }
            catch (DevChallenge.ClosedException)
            {
            }





        }


    }
}
