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
                    return new DevChallenge.Implementation.TcpConnection.Request(msg);
                };
            this.CreateNotificationXElement = msg =>
                {
                    return new DevChallenge.Implementation.TcpConnection.Notification(msg);
                };
            this.CreateResponseXElementString = (msg,id) =>
                {
                    return new DevChallenge.Implementation.TcpConnection.Response(msg,id);
                };
            this.GetNotificationXElement = raw =>
            {
                return DevChallenge.Implementation.TcpConnection.Notification.Get(raw);
            };
            this.GetRequestXElement = raw =>
            {
                return DevChallenge.Implementation.TcpConnection.Request.Get(raw);
            };
            this.GetResponseXElement = raw =>
            {
                return DevChallenge.Implementation.TcpConnection.Response.Get(raw);
            };
        }
    }
}
