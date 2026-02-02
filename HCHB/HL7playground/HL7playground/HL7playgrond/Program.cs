using NHapi.Base.Parser;
using System.Xml;

XmlDocument doc = new XmlDocument();
doc.Load(@".\Cerner Samples\Discharge_Summary_ED.xml");


var xmlParser = new DefaultXMLParser();
var m = xmlParser.ParseDocument(doc, "2.8.1");

Console.WriteLine(string.Join(' ', m.Message.Names));
