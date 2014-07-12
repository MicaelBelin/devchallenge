using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DevChallenge.Connection
{
    public class Tcp : Abstract, ITcpConnection
    {

        public Tcp(TcpClient c)
        {
            c.NoDelay = true;
            client = c;
            stream = client.GetStream();
            StartCollector();
        }

        private TcpClient client;
        System.IO.Stream stream;

        private object clientlocker = new object();


        protected override IEnumerable<System.Xml.Linq.XElement> ReadXmlItems()
        {

            while (true)
            {

                string ret = "";
                int level = 0;
                bool comment = false;
                bool endtag = false;
                int prevvalue = 0;

                while (true)
                {
                    int value = -1;
                    try
                    {
                        value = stream.ReadByte();
                    }
                    catch (Exception)
                    {
                        value = -1;
                    }
                    if (value < 0)
                    {
                        yield break;
                    }
                    ret += (char)value;
                    switch (value)
                    {
                        case '<':
                            comment = false;
                            endtag = false;
                            if (level == 0) ret = "<"; //remove unnecessary characters
                            break;
                        case '?':
                            if (prevvalue == '<') comment = true;
                            break;
                        case '/':
                            if (prevvalue == '<') endtag = true;
                            break;
                        case '>':
                            if (prevvalue == '/') //empty tag
                            {
                                if (level == 0) yield return XElement.Parse(ret);
                            }
                            else if (!comment)
                            {
                                if (endtag) level--; else level++;
                                if (level == 0)
                                {
                                    yield return XElement.Parse(ret);
                                }
                            }
                            break;
                    }

                    prevvalue = value;
                }

            }


        }

        protected override void SendXmlItem(System.Xml.Linq.XElement e)
        {
            lock (clientlocker)
            {
                if (client == null) throw new ClosedException();
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(e.ToString());
                stream.Write(msg, 0, msg.Count());
            }
        }

        public override void Close()
        {
            lock (clientlocker)
            {
                if (client == null) return;
                client.Close();
                client = null;
            }
        }

        public TcpClient TcpClient
        {
            get { return client; }
        }
    }
}
