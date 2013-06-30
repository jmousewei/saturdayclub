using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace saturdayclub.Messages
{
    public class EventMessage : XmlMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.Event; }
        }

        public MessageEvents Event
        {
            get
            {
                var eventName = this.Xml.Element("xml").Element("Event").DescendantNodes().OfType<XCData>().First().Value;
                MessageEvents type = eventName.ToEnum<MessageEvents>();
                return type;
            }
            set
            {
                var node = this.Xml.Element("xml").Element("Event").DescendantNodes().OfType<XCData>().First();
                node.Value = value.ToString().ToLower();
            }
        }

        public string EventKey
        {
            get
            {
                var node = this.Xml.Element("xml").Element("EventKey").DescendantNodes().OfType<XCData>().First();
                return node.Value;
            }
            set
            {
                var node = this.Xml.Element("xml").Element("EventKey").DescendantNodes().OfType<XCData>().First();
                node.Value = value ?? string.Empty;
            }
        }

        protected override XDocument GetXmlSchema()
        {
            var xdoc = base.GetXmlSchema();
            var xml = xdoc.Element("xml");
            //
            // Event
            //
            var innerData = new XCData(string.Empty);
            var outerData = new XElement("Event");
            outerData.Add(innerData);
            xml.Add(outerData);
            //
            // EventKey
            //
            innerData = new XCData(string.Empty);
            outerData = new XElement("EventKey");
            outerData.Add(innerData);
            xml.Add(outerData);
            return xdoc;
        }
    }
}
