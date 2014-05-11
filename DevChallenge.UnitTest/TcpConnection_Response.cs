using System;
using System.Xml.Linq;
using DevChallenge.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test_DevChallenge
{
    [TestClass]
    public class TcpConnection_Response
    {
        [TestMethod]
        public void MessageIntegrity()
        {
            var message = new XElement("hej", new XAttribute("id", "123"));
            var n = new TcpConnection.Response(message, "123");
            Assert.AreEqual("123", n.RequestId);
            Assert.AreEqual(message, n.Message);
        }

        [TestMethod]
        public void GetWithCorrectTypeShouldNotThrow()
        {
            var raw = new XElement("response", new XAttribute("id", "123"), new XElement("themessage", "Hej!"));
            TcpConnection.Response.Get(raw);
        }

        [TestMethod]
        [ExpectedException(typeof(DevChallenge.InvalidMessageTypeException))]
        public void GetWithInCorrectTypeShouldThrow()
        {
            var raw = new XElement("comment", new XElement("themessage", "Hej!"));
            TcpConnection.Response.Get(raw);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void MissingIdShouldThrow()
        {
            var raw = new XElement("response", new XElement("themessage", "Hej!"));
            TcpConnection.Response.Get(raw);
        }
    }
}
