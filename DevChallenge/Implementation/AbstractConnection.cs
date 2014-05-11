using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DevChallenge.Implementation
{
    public abstract partial class AbstractConnection : IConnection
    {
        protected abstract IEnumerable<XElement> ReadXmlItems();
        protected abstract void SendXmlItem(XElement e);
        public abstract void Close();


        void Send(XElement e)
        {
            Action<XElement, MessageDirection>[] monitors;

            lock (monitor)
            {
                monitors = monitor.ToArray();
            }
            foreach (var p in monitors)
            {
                p(e, MessageDirection.Outgoing);
            }
            SendXmlItem(e);
        }

        public AbstractConnection()
        {
            collector = new Collector(ReadXmlItems);
            runloop = new RunLoop(collector, DispatchMessage);
        }

        public void PostRequest(System.Xml.Linq.XElement message)
        {
            var request = new Request(message);
            Send(request.Serialized);
        }
        public System.Xml.Linq.XElement SendRequest(System.Xml.Linq.XElement message)
        {
            return SendRequest(message, Timeout.InfiniteTimeSpan);
        }

        public System.Xml.Linq.XElement SendRequest(System.Xml.Linq.XElement message, TimeSpan timeout)
        {
            IResponse ret = null;
            var request = new Request(message);
            AutoResetEvent gotevent = new AutoResetEvent(false);
            Func<XElement, FilterResponse> filter = rawmsg =>
                {
                    var r = GetResponse(rawmsg);
                    if (r.RequestId == request.RequestId)
                    {
                        ret = r;
                        gotevent.Set();
                        return FilterResponse.Consume;
                    }
                    else return FilterResponse.PassToNext;
                };
            RegisterFilter(filter);
            try
            {
                Send(request.Serialized);
                WaitUntil(gotevent, timeout);
            }
            finally
            {
                UnregisterFilter(filter);
            }
            return ret.Message;
        }

        public void SendNotification(System.Xml.Linq.XElement message)
        {
            Send(new Notification(message).Serialized);
        }

        public void SendResponse(System.Xml.Linq.XElement response, string requestid)
        {
            Send(new Response(response, requestid).Serialized);
        }

        public IRequest WaitForRequest(Func<IRequest, FilterResponse> filter)
        {
            return WaitForRequest(filter, Timeout.InfiniteTimeSpan);
        }

        public IRequest WaitForRequest(Func<IRequest, FilterResponse> filter, TimeSpan timeout)
        {
            Request ret = null;
            WaitUntilFilterMatch(e =>
                {
                    ret = Request.Get(e);
                    return filter(ret);
                },timeout);
            return ret;
        }

        public INotification WaitForNotification(Func<INotification, FilterResponse> filter)
        {
            return WaitForNotification(filter, Timeout.InfiniteTimeSpan);
        }

        public INotification WaitForNotification(Func<INotification, FilterResponse> filter, TimeSpan timeout)
        {
            Notification ret = null;
            WaitUntilFilterMatch(e =>
            {
                ret = Notification.Get(e);
                return filter(ret);
            },timeout);
            return ret;
        }


        public void RegisterMonitor(Action<XElement,MessageDirection> me)
        {
            lock (monitor)
            {
                monitor.Insert(0,me);
            }
        }

        public void UnregisterMonitor(Action<XElement,MessageDirection> me)
        {
            lock (monitor)
            {
                monitor.Remove(me);
            }
        }

        public void RegisterFilter(Func<System.Xml.Linq.XElement, FilterResponse> me)
        {
            lock (msgfilter)
            {
                msgfilter.Insert(0, me);
            }
        }

        public void UnregisterFilter(Func<System.Xml.Linq.XElement, FilterResponse> me)
        {
            lock (msgfilter)
            {
                msgfilter.Remove(me);
            }
        }
        private List<Func<XElement, FilterResponse>> msgfilter = new List<Func<XElement, FilterResponse>>();
        private List<Action<XElement,MessageDirection>> monitor = new List<Action<XElement,MessageDirection>>();

        public void DispatchMessage(XElement msg)
        {
            Func<XElement, FilterResponse>[] filters;
            Action<XElement,MessageDirection>[] monitors;

            lock (msgfilter)
            {
                filters = msgfilter.ToArray();
            }
            lock (monitor)
            {
                monitors = monitor.ToArray();
            }
            foreach (var p in monitors)
            {
                p(msg,MessageDirection.Incoming);
            }
                foreach (var f in filters)
                {
                    try
                    {
                        if (f(msg) == FilterResponse.Consume) break;
                    }
                    catch (InvalidMessageTypeException) //in case this is thrown, it means this filter is to be ignored.
                    {
                    }
                }
        }


        public void Exec()
        {
            runloop.Exec();
        }

        public void Exec(TimeSpan timeout)
        {
            runloop.Exec(timeout);
        }

        public void ExecWhile(Func<bool> condition)
        {
            runloop.ExecWhile(condition);
        }

        public void ExecWhile(Func<bool> condition, TimeSpan timeout)
        {
            runloop.ExecWhile(condition, timeout);
        }

        public void WaitUntil(EventWaitHandle handle)
        {
            WaitUntil(handle, Timeout.InfiniteTimeSpan);
        }

        public void WaitUntil(EventWaitHandle handle, TimeSpan timeout)
        {
            bool runnative = runloop.ExecThread == null || runloop.ExecThread == Thread.CurrentThread;
            if (runnative)
            {
                ExecWhile(() => !handle.WaitOne(0), timeout);
            }
            else
            {
                if (timeout != Timeout.InfiniteTimeSpan)
                {
                    if (handle.WaitOne(timeout) == false) throw new TimeoutException();
                }
                else handle.WaitOne();
            }
        }

        public void WaitUntilFilterMatch(Func<XElement, FilterResponse> filter)
        {
            WaitUntilFilterMatch(filter, Timeout.InfiniteTimeSpan);
        }

        public void WaitUntilFilterMatch(Func<XElement, FilterResponse> filter, TimeSpan timeout)
        {

            AutoResetEvent GotFilterEvent = new AutoResetEvent(false);

            Func<XElement, FilterResponse> wrappedfilter = e =>
            {
                if (filter(e) == FilterResponse.Consume)
                {
                    GotFilterEvent.Set();
                    return FilterResponse.Consume;
                }
                else return FilterResponse.PassToNext;
            };
            RegisterFilter(wrappedfilter);

            try
            {
                WaitUntil(GotFilterEvent, timeout);
            }
            finally
            {
            }

        }




        Collector collector;
        RunLoop runloop;

        public void StartCollector()
        {
            collector.Start();
        }

        public void RunUntilClosed()
        {
            try
            {
                Exec();
            }
            catch (ClosedException e)
            {
                string msg = e.Message;
            }
        }

        public IRequest GetRequest(System.Xml.Linq.XElement rawmsg)
        {
            return Request.Get(rawmsg);
        }

        public INotification GetNotification(System.Xml.Linq.XElement rawmsg)
        {
            return Notification.Get(rawmsg);
        }

        public IResponse GetResponse(System.Xml.Linq.XElement rawmsg)
        {
            return Response.Get(rawmsg);
        }

        public IRequest CreateRequest(System.Xml.Linq.XElement message)
        {
            return new Request(message);
        }

        public INotification CreateNotification(System.Xml.Linq.XElement message)
        {
            return new Notification(message);
        }

        public IResponse CreateResponse(System.Xml.Linq.XElement message, string requestid)
        {
            return new Response(message, requestid);
        }

        public void Dispose()
        {
            Close();
        }







    }
}
