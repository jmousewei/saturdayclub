using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace saturdayclub.Messages
{
    public class ReplyTextMessage : XmlMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.Text; }
        }

        public string Content
        {
            get
            {
                var node = this.Xml.Element("xml").Element("Content").DescendantNodes().OfType<XCData>().First();
                return node.Value;
            }
            set
            {
                if (XmlMessage.ContentEncoding.GetByteCount(value ?? string.Empty) > 2048)
                {
                    throw new ArgumentOutOfRangeException();
                }
                var node = this.Xml.Element("xml").Element("Content").DescendantNodes().OfType<XCData>().First();
                node.Value = value ?? string.Empty;
            }
        }

        public FunctionFlags Functions
        {
            get
            {
                var node = this.Xml.Element("xml").Element("FuncFlag");
                return (FunctionFlags)int.Parse(node.Value);
            }
            set
            {
                var node = this.Xml.Element("xml").Element("FuncFlag");
                node.Value = ((int)value).ToString();
            }
        }

        protected override XDocument GetXmlSchema()
        {
            var xdoc = base.GetXmlSchema();
            var xml = xdoc.Element("xml");
            //
            // Content
            //
            var innerData = new XCData(string.Empty);
            var outerData = new XElement("Content");
            outerData.Add(innerData);
            xml.Add(outerData);
            //
            // FuncFlag
            //
            outerData = new XElement("FuncFlag");
            xml.Add(outerData);
            return xdoc;
        }
    }
}
