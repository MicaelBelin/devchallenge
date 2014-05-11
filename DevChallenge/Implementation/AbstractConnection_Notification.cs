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
        public class Notification : INotification
        {
            public XElement Message { get; set; }
            public static Notification Get(XElement message)
            {
                if (message == null || message.Name != "notification") throw new InvalidMessageTypeException();
                return new Notification(message.Elements().FirstOrDefault());
            }

            public Notification(XElement message)
            {
                Message = message;
            }

            public XElement Serialized
            {
                get
                {
                    return new XElement("notification", Message);
                }
            }
        }
    }
}
