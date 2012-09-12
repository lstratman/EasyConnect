/*
 * Copyright (c) 2005 Poderosa Project, All Rights Reserved.
 * 
 * $Id: ConfigXML.cs,v 1.2 2005/04/20 09:06:03 okajima Exp $
 */
using System;
using System.Collections;
using System.Xml;

namespace Poderosa.Config {
	public class DOMNodeConverter {
		public static ConfigNode Read(XmlDocument doc) {
			return Read(doc.DocumentElement);
		}
		public static ConfigNode Read(XmlElement elem) {
			ConfigNode node = new ConfigNode(elem.LocalName);
			foreach(XmlAttribute attr in elem.Attributes)
				node[attr.LocalName] = attr.Value;
			foreach(XmlNode ch in elem.ChildNodes) {
				XmlElement ce = ch as XmlElement;
				if(ce!=null)
					node.AddChild(Read(ce));
			}
			return node;
		}

		public static XmlElement Write(XmlDocument doc, ConfigNode node) {
			XmlElement e = doc.CreateElement(node.Name);
			IDictionaryEnumerator i = node.GetPairEnumerator();
			while(i.MoveNext())
				e.SetAttribute((string)i.Key, (string)i.Value);
			foreach(ConfigNode ch in node.Children)
				e.AppendChild(Write(doc, ch));
			return e;
		}

		public static void Write(XmlWriter wr, ConfigNode node) {
			wr.WriteStartElement(node.Name);
			IDictionaryEnumerator i = node.GetPairEnumerator();
			while(i.MoveNext())
				wr.WriteAttributeString((string)i.Key, (string)i.Value);
			foreach(ConfigNode ch in node.Children)
				Write(wr, ch);
			wr.WriteEndElement();
		}
	}
}
