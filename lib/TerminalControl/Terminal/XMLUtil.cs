/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: XMLUtil.cs,v 1.2 2005/04/20 08:45:48 okajima Exp $
*/
using System;
using System.Collections;
using System.Xml;
using System.Text;

namespace Poderosa.Toolkit
{
	public class XMLUtil
	{
		public static XmlWriter CreateDefaultWriter(string filename) {
			XmlTextWriter wr = new XmlTextWriter(filename, Encoding.Default);
			wr.Formatting = Formatting.Indented;
			wr.Indentation = 2;
			wr.IndentChar = ' ';
			wr.Namespaces = true;
			wr.WriteStartDocument(); //XML PI
			return wr;
		}

		public static XmlReader CreateDefaultReader(string filename) {
			XmlTextReader re = new XmlTextReader(filename);
			re.WhitespaceHandling = WhitespaceHandling.Significant;
			return re;
		}

		public static XmlDocument FileToDOM(string filename) {
			XmlDocument doc = new XmlDocument();
			doc.Load(filename);
			return doc;
		}

		public static bool FindNode(XmlReader reader, XmlNodeType type) {
			do {
				if(reader.NodeType==type) return true;
			} while(reader.Read());

			return false;
		}

		//ふつう使わない要素をスキップしながら読むタイプのRead
		public static bool ReadSubstantialNode(XmlReader reader) {
			do {
				if(!reader.Read())
					return false;

				XmlNodeType nt = reader.NodeType;
				if(nt==XmlNodeType.Attribute || nt==XmlNodeType.CDATA || nt==XmlNodeType.Element || nt==XmlNodeType.EndElement
					|| nt==XmlNodeType.SignificantWhitespace || nt==XmlNodeType.Text || nt==XmlNodeType.Whitespace) return true;
			} while(true);
		}

		
		//<a>aaa</a><b>bbb</b>... という形式が続く限りresultに追加する
		public static void ReadElementsIntoStrMap(XmlReader reader, Hashtable result) {
			while(ReadSubstantialNode(reader) && reader.NodeType!=XmlNodeType.EndElement) {
				string name = null, value = null;
				do {
					if(reader.NodeType==XmlNodeType.Element) {
						name = reader.LocalName;
						if(reader.IsEmptyElement) break;
					}
					else if(reader.NodeType==XmlNodeType.Text || reader.NodeType==XmlNodeType.CDATA)
						value = reader.Value;
					else if(reader.NodeType==XmlNodeType.EndElement) {
						result.Add(name, value);
						break;
					}
				} while(ReadSubstantialNode(reader));
			}
		}
		//アトリビュートをHashtableに入れる
		public static void ReadAttributesIntoStrMap(XmlReader reader, Hashtable result) {
			if(!reader.MoveToFirstAttribute()) return;

			do {
				result.Add(reader.LocalName, reader.Value);
			} while(reader.MoveToNextAttribute());

			reader.MoveToElement();
		}

		public static bool MoveToStartElement(XmlReader reader, string name, string nsuri) {
			while(reader.NodeType!=XmlNodeType.Element || reader.LocalName!=name || reader.NamespaceURI!=nsuri) {
				if(!ReadSubstantialNode(reader)) return false;
			}
			return true;
		}
		public static bool MoveNextToStartElement(XmlReader reader, string name, string nsuri) {
			while(reader.NodeType!=XmlNodeType.Element || reader.LocalName!=name || reader.NamespaceURI!=nsuri) {
				if(!ReadSubstantialNode(reader)) return false;
			}
			return ReadSubstantialNode(reader); //見つかったStartElementの次に移動
		}
		public static bool MoveNextToStartElement(XmlReader reader, string name) {
			while(reader.NodeType!=XmlNodeType.Element || reader.LocalName!=name) {
				if(!ReadSubstantialNode(reader)) return false;
			}
			return ReadSubstantialNode(reader); //見つかったStartElementの次に移動
		}
		public static bool MoveNextToEndElement(XmlReader reader) {
			while(reader.NodeType!=XmlNodeType.EndElement) {
				if(reader.IsEmptyElement) break;
				if(!ReadSubstantialNode(reader)) return false;
			}
			return ReadSubstantialNode(reader); //見つかったEndElementの次に移動
		}
	}
}
