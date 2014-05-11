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
        public class Response : IResponse
        {
            public XElement Message { get; set; }
            public string RequestId { get; set; }
            public static Response Get(XElement serialized, string requestid)
            {
                if (serialized == null || serialized.Name != "response" || serialized.Attribute("id").Value != requestid) throw new InvalidMessageTypeException();
                return new Response(serialized.Elements().FirstOrDefault(), serialized.Attribute("id").Value);
            }
            public static Response Get(XElement serialized)
            {
                if (serialized == null || serialized.Name != "response") throw new InvalidMessageTypeException();
                return new Response(serialized.Elements().FirstOrDefault(), serialized.Attribute("id").Value);
            }

            public Response(XElement message, string requestid)
            {
                Message = message;
                RequestId = requestid;
            }

            public XElement Serialized
            {
                get
                {
                    return new XElement("response", new XAttribute("id", RequestId), Message);
                }
            }

        }
    }
}
