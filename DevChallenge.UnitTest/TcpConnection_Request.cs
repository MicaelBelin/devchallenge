using System;
using System.Xml.Linq;
using DevChallenge;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test_DevChallenge
{
    [TestClass]
    public class TcpConnection_Request
    {
        [TestMethod]
        public void MessageIntegrity()
        {
            var message = new XElement("hej");
            var n = new DevChallenge.Connection.Tcp.Request(message, "123");
            Assert.AreEqual("123", n.RequestId);
            Assert.AreEqual(message, n.Message);
        }

        [TestMethod]
        public void GetWithCorrectTypeShouldNotThrow()
        {
            var raw = new XElement("request", new XAttribute("id","123"),new XElement("themessage", "Hej!"));
            DevChallenge.Connection.Tcp.Request.Get(raw);
        }

        [TestMethod]
        [ExpectedException(typeof(DevChallenge.InvalidMessageTypeException))]
        public void GetWithInCorrectTypeShouldThrow()
        {
            var raw = new XElement("comment", new XElement("themessage", "Hej!"));
            DevChallenge.Connection.Tcp.Request.Get(raw);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void MissingIdShouldThrow()
        {
            var raw = new XElement("request", new XElement("themessage", "Hej!"));
            DevChallenge.Connection.Tcp.Request.Get(raw);
            
        }
    }
}
