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


        public class Collector : IRunLoopSource
        {
            public bool HasAvailableItem
            {
                get
                {
                    lock (queue)
                    {
                        return queue.Count != 0;
                    }
                }
            }


            public XElement GetItem(TimeSpan timeout)
            {
                lock (queue)
                {
                    while (!HasAvailableItem)
                    {
                        if (!IsRunning) throw new ClosedException();
                        if (!Monitor.Wait(queue, timeout)) throw new TimeoutException();
                    }
                    var ret = queue.First();
                    queue.RemoveAt(0);
                    return ret;
                }
            }

            public XElement GetItem()
            {
                lock (queue)
                {
                    while (!HasAvailableItem)
                    {
                        if (!IsRunning) throw new ClosedException();
                        Monitor.Wait(queue);
                    }
                    var ret = queue.First();
                    queue.RemoveAt(0);
                    return ret;
                }
            }



            private List<XElement> queue = new List<XElement>();


            public Collector(Func<IEnumerable<XElement>> itempoolfunc)
            {
                readxmlitems = itempoolfunc;
            }

            Func<IEnumerable<XElement>> readxmlitems;

            public void Start()
            {
                lock (queue)
                {
                    if (isrunning) throw new InvalidOperationException("Collector is already started");
                    isrunning = true;
                }


                CollectorTask = Task.Run(() =>
                {
                    try
                    {
                        foreach (var i in readxmlitems())
                        {
                            lock (queue)
                            {
                                if (i == null)
                                {
                                    break;
                                }
                                else
                                {
                                    queue.Add(i);
                                    Monitor.Pulse(queue);
                                }
                            }
                        }
                    }
                    finally
                    {
                        lock (queue)
                        {
                            isrunning = false;
                            Monitor.PulseAll(queue);
                        }
                    }
                });

            }

            
            public Task CollectorTask { get; private set; }


            public bool IsRunning 
            {
                get
                {
                    lock (queue)
                    {
                        return isrunning;
                    }
                }
            }
            bool isrunning = false;
            
        }
    }
}
