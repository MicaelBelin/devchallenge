using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace DevChallenge.Server
{
    public static class Extensions
    {
        public static T Deserialized<T>(this XElement e)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            return (T)serializer.Deserialize(e.CreateReader());
        }
    }
}
