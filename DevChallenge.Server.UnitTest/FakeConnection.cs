using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DevChallenge.Fakes
{
    public class FakeConnection : DevChallenge.Fakes.StubIConnection
    {


        public void SimulateFilterProcess(XElement e)
        {
            foreach (var filter in filters)
            {
                try
                {
                    if (filter(e) == FilterResponse.Consume) break;
                }
                catch (DevChallenge.InvalidMessageTypeException)
                {
                }
            }
        }

        List<Func<XElement, FilterResponse>> filters = new List<Func<XElement, FilterResponse>>();
        public FakeConnection()
        {
            this.RegisterFilterFuncOfXElementFilterResponse = func =>
                {
                    filters.Insert(0, func);
                };

            this.UnregisterFilterFuncOfXElementFilterResponse = func =>
                {
                    filters.Remove(func);
                };


            this.CreateRequestXElement = msg =>
                {
                    return new DevChallenge.Connection.Tcp.Request(msg);
                };
            this.CreateNotificationXElement = msg =>
                {
                    return new DevChallenge.Connection.Tcp.Notification(msg);
                };
            this.CreateResponseXElementString = (msg,id) =>
                {
                    return new DevChallenge.Connection.Tcp.Response(msg,id);
                };
            this.GetNotificationXElement = raw =>
            {
                return DevChallenge.Connection.Tcp.Notification.Get(raw);
            };
            this.GetRequestXElement = raw =>
            {
                return DevChallenge.Connection.Tcp.Request.Get(raw);
            };
            this.GetResponseXElement = raw =>
            {
                return DevChallenge.Connection.Tcp.Response.Get(raw);
            };
        }
    }
}
