using System;
using System.Xml.Linq;
using DevChallenge;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test_DevChallenge
{
    [TestClass]
    public class TcpConnection_Notification
    {
        [TestMethod]
        public void MessageIntegrity()
        {
            var message = new XElement("hej");
            var n = new DevChallenge.Connection.Tcp.Notification(message);
            Assert.AreEqual(message, n.Message);
        }

        [TestMethod]
        public void GetWithCorrectTypeShouldNotThrow()
        {
            var raw = new XElement("notification", new XElement("themessage", "Hej!"));
            DevChallenge.Connection.Tcp.Notification.Get(raw);
        }

        [TestMethod]
        [ExpectedException(typeof(DevChallenge.InvalidMessageTypeException))]
        public void GetWithInCorrectTypeShouldThrow()
        {
            var raw = new XElement("comment", new XElement("themessage", "Hej!"));
            DevChallenge.Connection.Tcp.Notification.Get(raw);
        }


    }
}
