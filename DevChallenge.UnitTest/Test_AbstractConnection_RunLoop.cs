using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test_DevChallenge
{
    [TestClass]
    public class Test_AbstractConnection_RunLoop
    {

        class Source : DevChallenge.Connection.Abstract.IRunLoopSource
        {

            public Source(bool doclose)
            {
                queue.Add(new XElement("element1"));
                queue.Add(new XElement("element2"));
                queue.Add(new XElement("element3"));
                if (doclose) queue.Close();
            }

            DevChallenge.Util.AsyncQueue<XElement> queue = new DevChallenge.Util.AsyncQueue<XElement>();

            public System.Xml.Linq.XElement GetItem(TimeSpan delay)
            {
                try
                {
                    return queue.Pop(delay);
                }
                catch (InvalidOperationException)
                {
                    throw new DevChallenge.ClosedException();
                }
            }

            public System.Xml.Linq.XElement GetItem()
            {
                try
                {
                    return queue.Pop();
                }
                catch (InvalidOperationException)
                {
                    throw new DevChallenge.ClosedException();
                }
            }
        }


        [TestMethod]
        public void ReadItemsThenFinish()
        {
            int readitems = 0;
            var runloop = new DevChallenge.Connection.Abstract.RunLoop(new Source(true), e => { readitems++; });
            try
            {
                runloop.Exec();
            }
            catch (DevChallenge.ClosedException)
            {
                Assert.AreEqual(3, readitems);
            }
            catch (Exception)
            {
                Assert.Fail("unexpected exception");
            }
        }

        [TestMethod]
        public void ExecWhile()
        {
            int readitems = 0;
            var runloop = new DevChallenge.Connection.Abstract.RunLoop(new Source(true), e => { readitems++; });
            runloop.ExecWhile(() => readitems != 2);
            Assert.AreEqual(2, readitems);
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public void Timeout()
        {
            var runloop = new DevChallenge.Connection.Abstract.RunLoop(new Source(false), e => { });
            runloop.Exec(TimeSpan.FromMilliseconds(1));
        }

    }
}
