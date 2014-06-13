using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DevChallenge.Connection
{
    public partial class Abstract
    {
        public interface IRunLoopSource
        {
            XElement GetItem(TimeSpan delay);
            XElement GetItem();
        }


        public class RunLoop
        {


            public RunLoop(IRunLoopSource source, Action<XElement> onnewitem)
            {
                processitem = onnewitem;
                this.source = source;
            }

            IRunLoopSource source;
            Action<XElement> processitem;

            public void Exec()
            {
                Exec(Timeout.InfiniteTimeSpan);
            }

            public void Exec(TimeSpan timeout)
            {
                ExecWhile(() => true, timeout);
            }

            public void ExecWhile(Func<bool> condition)
            {
                ExecWhile(condition, Timeout.InfiniteTimeSpan);
            }

            public Thread ExecThread
            {
                get
                {
                    lock (execthreadlocker)
                    {
                        return execthread;
                    }
                }
            }

            Thread execthread = null;
            object execthreadlocker = new object();
            public void ExecWhile(Func<bool> condition, TimeSpan timeout)
            {

                bool isrootexec = false;


                lock (execthreadlocker)
                {
                    if (execthread != null && execthread != Thread.CurrentThread) throw new InvalidOperationException("Exec is already running in another thread.");
                    if (execthread == null) isrootexec = true;
                    execthread = Thread.CurrentThread;
                }


                try
                {


                    DateTime endtime = DateTime.Now + timeout;
                    while (condition())
                    {

                        TimeSpan subdelay = endtime - DateTime.Now;

                        if (subdelay.TotalMilliseconds <= 0 && timeout != Timeout.InfiniteTimeSpan)
                        {
                            throw new TimeoutException();
                        }
                        XElement item = null;

                        if (timeout == Timeout.InfiniteTimeSpan)
                            item = source.GetItem();
                        else item = source.GetItem(subdelay);

                        processitem(item);

                    }


                }
                finally
                {
                    if (isrootexec)
                    {
                        lock (execthreadlocker)
                        {
                            execthread = null;
                        }
                    }
                }


            }

        }
    }
}
