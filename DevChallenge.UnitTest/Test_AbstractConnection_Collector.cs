using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test_DevChallenge
{
    [TestClass]
    public class Test_AbstractConnection_Collector
    {

        IEnumerable<XElement> testpool()
        {
            yield return new DevChallenge.Implementation.TcpConnection.Notification(new XElement("notification1")).Serialized;
            yield return new DevChallenge.Implementation.TcpConnection.Notification(new XElement("notification2")).Serialized;
        }
        IEnumerable<XElement> testpoolwithhold()
        {
            yield return new DevChallenge.Implementation.TcpConnection.Notification(new XElement("notification1")).Serialized;
            yield return new DevChallenge.Implementation.TcpConnection.Notification(new XElement("notification2")).Serialized;
            Thread.Sleep(-1);
        }
        IEnumerable<XElement> emptypool()
        {
            yield break;
        }

        [TestMethod]
        public void GetItem()
        {
            var collector = new DevChallenge.Implementation.AbstractConnection.Collector(testpool);

            collector.Start();

            Assert.AreEqual("notification1", DevChallenge.Implementation.TcpConnection.Notification.Get(collector.GetItem()).Message.Name);
            Assert.AreEqual("notification2", DevChallenge.Implementation.TcpConnection.Notification.Get(collector.GetItem()).Message.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(DevChallenge.ClosedException))]
        public void GetItemWithoutStarting()
        {
            var collector = new DevChallenge.Implementation.AbstractConnection.Collector(testpool);

            collector.GetItem();
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public void GetItem_with_delay()
        {
            var collector = new DevChallenge.Implementation.AbstractConnection.Collector(testpoolwithhold);

            collector.Start();

            Assert.AreEqual("notification1", DevChallenge.Implementation.TcpConnection.Notification.Get(collector.GetItem()).Message.Name);
            Assert.AreEqual("notification2", DevChallenge.Implementation.TcpConnection.Notification.Get(collector.GetItem()).Message.Name);

            collector.GetItem(TimeSpan.FromMilliseconds(100));
        }



        [TestMethod]
        public void IsRunning()
        {
            var collector = new DevChallenge.Implementation.AbstractConnection.Collector(testpool);
            Assert.AreEqual(false, collector.IsRunning);

            collector.Start();
            Assert.AreEqual(true, collector.IsRunning);

            collector.GetItem();
            collector.GetItem();

            Thread.Sleep(10); //Possible race condition!
            Assert.AreEqual(false, collector.IsRunning);                        
        }

        [TestMethod]
        [ExpectedException(typeof(DevChallenge.ClosedException))]
        public void TestExceedingGetItem()
        {
            var collector = new DevChallenge.Implementation.AbstractConnection.Collector(testpool);
            Assert.AreEqual(false, collector.IsRunning);
            collector.Start();
            try
            {
                collector.GetItem();
                collector.GetItem();
            }
            catch (Exception)
            {
                Assert.Fail();
            }
            collector.GetItem();
        }


        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),"Collector is already started")]
        public void StartingCollectorTwice()
        {
            var collector = new DevChallenge.Implementation.AbstractConnection.Collector(testpool);
            collector.Start();
            collector.Start();
        }

    }
}
