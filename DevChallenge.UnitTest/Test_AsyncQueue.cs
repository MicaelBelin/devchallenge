using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Test_DevChallenge
{
    [TestClass]
    public class Test_AsyncQueue
    {
        [TestMethod]
        public void TestAsyncQueue()
        {
            var queue = new DevChallenge.Util.AsyncQueue<int>();



            var task = Task<List<int>>.Run(() =>
                {
                    var ret = queue.AsEnumerable().ToList();
                    return ret;
                });


            queue.Add(5);
            queue.Add(6);
            Assert.IsFalse(task.IsCompleted);
            queue.Close();

            Assert.IsTrue(task.Wait(500));
            Assert.IsTrue(task.IsCompleted);

            var res = task.Result;

            Assert.AreEqual(5,task.Result[0]);
            Assert.AreEqual(6,task.Result[1]);

        }
    }
}
