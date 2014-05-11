using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DevChallenge.Implementation
{
    public partial class AbstractConnection
    {
        public class Request : IRequest
        {
            public XElement Message { get; set; }
            public string RequestId { get; set; }
            public static Request Get(XElement serialized)
            {
                if (serialized == null || serialized.Name != "request") throw new InvalidMessageTypeException();
                return new Request(serialized.Elements().FirstOrDefault(), serialized.Attribute("id").Value);
            }

            static Random rand = new Random();
            static string NextRequestId()
            {
                return rand.Next().ToString();
            }


            public Request(XElement message)
                : this(message, NextRequestId())
            {
            }

            public Request(XElement message, string requestid)
            {
                Message = message;
                RequestId = requestid;
            }

            public XElement Serialized
            {
                get
                {
                    return new XElement("request", new XAttribute("id", RequestId), Message);
                }
            }
        }
    }
}
