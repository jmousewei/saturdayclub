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
            string name = this.MessageType.ToString().ToLower();
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

        private static MessageTypes NativeGetMessageType(XDocument xdoc)
        {
            var typeName = xdoc.Element("xml").Element("MsgType").DescendantNodes().OfType<XCData>().First().Value;
            MessageTypes type = typeName.ToEnum<MessageTypes>();
            return type;
        }

        public static MessageTypes GetMessageType(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            var xdoc = XDocument.Load(stream);
            return NativeGetMessageType(xdoc);
        }

        public static T Deserialize<T>(Stream stream) where T : XmlMessage, new()
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            var xdoc = XDocument.Load(stream);
            T newObj = new T();
            newObj.doc = xdoc;
            return null;
        }

        public static XmlMessage CreateClient(MessageTypes type)
        {
            switch (type)
            {
                case MessageTypes.Text:
                    return new ClientTextMessage();
                case MessageTypes.Event:
                    return new EventMessage();
                default:
                    return null;
            }
        }

        public static XmlMessage CreateClient(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            XDocument xdoc = XDocument.Load(stream);
            var type = NativeGetMessageType(xdoc);
            var newObj = CreateClient(type);
            if (newObj == null)
                return null;
            newObj.doc = xdoc;
            return newObj;
        }

        public static XmlMessage CreateReply(MessageTypes type)
        {
            switch (type)
            {
                case MessageTypes.Text:
                    return new ReplyTextMessage();
                default:
                    return null;
            }
        }

        public static XmlMessage CreateReply(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            XDocument xdoc = XDocument.Load(stream);
            var type = NativeGetMessageType(xdoc);
            var newObj = CreateReply(type);
            if (newObj == null)
                return null;
            newObj.doc = xdoc;
            return newObj;
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

        public static TEnum ToEnum<TEnum>(this string reference) where TEnum : struct
        {
            TEnum value;
            if (Enum.TryParse<TEnum>(reference, true, out value))
            {
                return value;
            }
            return default(TEnum);
        }
    }
}
