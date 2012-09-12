/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: EnumDescription.cs,v 1.2 2005/04/20 08:45:46 okajima Exp $
*/
using System;
using System.Collections;
using System.Reflection;

namespace Poderosa.Toolkit {

	//整数のenum値に表記をつけたり相互変換したりする　構造上
	[AttributeUsage(AttributeTargets.Enum)]
	public class EnumDescAttribute : Attribute {

#if MACRODOC
		public EnumDescAttribute(Type t) {
		}

#else
		private static Hashtable _assemblyToResource = new Hashtable(4); //小さいテーブルでよい
		public static void AddResourceTable(Assembly asm, StringResources res) {
			_assemblyToResource[asm] = res; //overwrite if the key is duplicated
		}

		private string[] _descriptions;
		private Hashtable _descToValue;
		private string[] _names;
		private Hashtable _nameToValue;
		private StringResources _strResource;

		public EnumDescAttribute(Type t) {
			_strResource = (StringResources)_assemblyToResource[t.Assembly];
			if(_strResource==null) throw new Exception("String resource is not found");
			Init(t);
		}

		public void Init(Type t) {
			MemberInfo[] ms = t.GetMembers();
			_descToValue = new Hashtable(ms.Length);
			_nameToValue = new Hashtable(ms.Length);

			ArrayList descriptions = new ArrayList(ms.Length);
			ArrayList names = new ArrayList(ms.Length);

			int expected = 0;
			foreach(MemberInfo mi in ms) {
				FieldInfo fi = mi as FieldInfo;
				if(fi!=null && fi.IsStatic && fi.IsPublic) {
					int intVal = (int)fi.GetValue(null); //int以外をベースにしているEnum値はサポート外
					if(intVal!=expected) throw new Exception("unexpected enum value order");
					EnumValueAttribute a = (EnumValueAttribute)(fi.GetCustomAttributes(typeof(EnumValueAttribute), false)[0]);
				
					string desc = a.Description;
					descriptions.Add(desc);
					_descToValue[desc] = intVal;

					string name = fi.Name;
					names.Add(name);
					_nameToValue[name] = intVal;

					expected++;
				}
			}

			_descriptions = (string[])descriptions.ToArray(typeof(string));
			_names        = (string[])names.ToArray(typeof(string));
		}

		public virtual string GetDescription(ValueType i) {
			return LoadString(_descriptions[(int)i]);
		}
		public virtual ValueType FromDescription(string v, ValueType d) {
			if(v==null) return d;
			IDictionaryEnumerator ie = _descToValue.GetEnumerator();
			while(ie.MoveNext()) {
				if(v==LoadString((string)ie.Key)) return (ValueType)ie.Value;
			}
			return d;
		}
		public virtual string GetName(ValueType i) {
			return _names[(int)i];
		}
		public virtual ValueType FromName(string v) {
			return (ValueType)_nameToValue[v];
		}
		public virtual ValueType FromName(string v, ValueType d) {
			if(v==null) return d;
			ValueType t = (ValueType)_nameToValue[v];
			return t==null? d : t;
		}

		public virtual string[] DescriptionCollection() {
			string[] r = new string[_descriptions.Length];
			for(int i=0; i<r.Length; i++)
				r[i] = LoadString(_descriptions[i]);
			return r;
		}
		private string LoadString(string id) {
			string t = _strResource.GetString(id);
			return t==null? id : t;
		}


		//アトリビュートを取得する
		private static Hashtable _typeToAttr = new Hashtable();
		public static EnumDescAttribute For(Type type) {
			EnumDescAttribute a = _typeToAttr[type] as EnumDescAttribute;
			if(a==null) {
				a = (EnumDescAttribute)(type.GetCustomAttributes(typeof(EnumDescAttribute), false)[0]);
				_typeToAttr.Add(type, a);
			}
			return a;
		}
#endif

	}

	[AttributeUsage(AttributeTargets.Field)]
	public class EnumValueAttribute : Attribute {
		private string _description;

		public string Description {
			get {
				return _description;
			}
			set {
				_description = value;
			}
		}
	}

}