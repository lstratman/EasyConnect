/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: StructuredTextXML.cs,v 1.3 2011/12/06 12:26:55 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Poderosa {
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class XmlStructuredTextReader : StructuredTextReader {
        private XmlElement _root;

        public XmlStructuredTextReader(XmlElement root) {
            _root = root;
        }
        public override StructuredText Read() {
            return Read(_root);
        }

        private StructuredText Read(XmlElement elem) {
            StructuredText node = new StructuredText(elem.LocalName);
            foreach (XmlAttribute attr in elem.Attributes)
                node.Set(attr.LocalName, attr.Value);
            foreach (XmlNode ch in elem.ChildNodes) {
                XmlElement ce = ch as XmlElement;
                if (ce != null)
                    node.AddChild(Read(ce));
            }
            return node;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class XmlStructuredTextWriter : StructuredTextWriter {
        private XmlWriter _writer;

        public XmlStructuredTextWriter(XmlWriter writer) {
            _writer = writer;
        }

        public override void Write(StructuredText node) {
            WriteNode(node);
        }

        private void WriteNode(StructuredText node) {
            _writer.WriteStartElement(node.Name);

            // first, output StructuredText.Entry as a XML attribute.
            List<StructuredText> childNodes = new List<StructuredText>();
            foreach (object ch in node.Children) {
                StructuredText.Entry e = ch as StructuredText.Entry;
                if (e != null) { //entry
                    _writer.WriteAttributeString(e.name, e.value);
                }
                else { //child node
                    childNodes.Add((StructuredText)ch);
                }
            }

            // second, output StructuredText as a XML tag.
            foreach (StructuredText ch in childNodes) {
                WriteNode(ch);
            }

            _writer.WriteEndElement();
        }
    }
}
