using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace saturdayclub.Messages
{
    public abstract class XmlMessage : IMessage
    {
        private XDocument doc;
        protected XDocument Xml
        {
            get { return this.doc; }
        }

        private static Encoding contentEncoding = Encoding.UTF8;
        public static Encoding ContentEncoding
        {
            get { return contentEncoding; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                contentEncoding = value;
            }
        }

        public string To
        {
            get
            {
                var node = this.doc.Element("xml").Element("ToUserName").DescendantNodes().OfType<XCData>().First();
                return node.Value;
            }
            set
            {
                var node = this.doc.Element("xml").Element("ToUserName").DescendantNodes().OfType<XCData>().First();
                node.Value = value ?? string.Empty;
            }
        }

        public string From
        {
            get
            {
                var node = this.doc.Element("xml").Element("FromUserName").DescendantNodes().OfType<XCData>().First();
                return node.Value;
            }
            set
            {
                var node = this.doc.Element("xml").Element("FromUserName").DescendantNodes().OfType<XCData>().First();
                node.Value = value ?? string.Empty;
            }
        }

        public int CreateTime
        {
            get
            {
                var node = this.doc.Element("xml").Element("CreateTime");
                return int.Parse(node.Value);
            }
            set
            {
                var node = this.doc.Element("xml").Element("CreateTime");
                node.Value = value.ToString();
            }
        }

        public abstract MessageTypes MessageType
        {
            get;
        }

        protected XmlMessage()
        {
            this.doc = GetXmlSchema();
        }

        public string GetMessageContent()
        {
            int ndx = (int)this.MessageType;
            var names = Enum.GetNames(typeof(MessageTypes));
            if (ndx < 0 || ndx > names.Length)
            {
                throw new InvalidOperationException("Invalid message type.");
            }
            string name = names[ndx].ToLower();
            var node = this.doc.Element("xml").Element("MsgType").DescendantNodes().OfType<XCData>().First();
            node.Value = name;
            return this.doc.ToString();
        }

        protected virtual XDocument GetXmlSchema()
        {
            XDocument xdoc = new XDocument();
            var xml = new XElement("xml");
            //
            // ToUserName
            // 
            var innerData = new XCData(string.Empty);
            var outerData = new XElement("ToUserName");
            outerData.Add(innerData);
            xml.Add(outerData);
            //
            // FromUserName
            // 
            innerData = new XCData(string.Empty);
            outerData = new XElement("FromUserName");
            outerData.Add(innerData);
            xml.Add(outerData);
            //
            // CreateTime
            // 
            outerData = new XElement("CreateTime");
            xml.Add(outerData);
            //
            // MsgType
            // 
            innerData = new XCData(string.Empty);
            outerData = new XElement("MsgType");
            outerData.Add(innerData);
            xml.Add(outerData);
            //
            // Root
            //
            xdoc.Add(xml);
            this.doc = xdoc;
            return xdoc;
        }

        public virtual void Deserialize(Stream stream)
        {
            XDocument xdoc = XDocument.Load(stream);
            this.doc = xdoc;
        }

        public static int Time()
        {
            return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public override string ToString()
        {
            return GetMessageContent();
        }
    }

    public static class XmlMessageHelper
    {
        public static byte[] GetMessageBytes(this XmlMessage msg)
        {
            var content = msg.GetMessageContent();
            var bytes = XmlMessage.ContentEncoding.GetBytes(content);
            return bytes;
        }
    }
}
