/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: RCollectionUtil.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Collections;

namespace Poderosa.Toolkit
{
	/// <summary>
	/// コンストラクタに渡されたIEnumeratorがIEnumerableを列挙するときにそれらを連結した形に見せるEnumerator
	/// Treeの中身の列挙などに使える
	/// </summary>
	internal class NestedEnumerator : IEnumerator {
		private IEnumerator _parent;
		private IEnumerator _child;
		public NestedEnumerator(IEnumerator en) {
			_parent = en;
			_child = null;
		}
		public NestedEnumerator(IEnumerator en, IEnumerator ch) {
			_parent = en;
			_child = ch;
		}
		public void Reset() {
			throw new NotSupportedException("Reset() is unsupported");
		}
		public bool MoveNext() {
			if(_child!=null) {
				if(_child.MoveNext())
					return true;
			}

			if(_parent.MoveNext()) {
				_child = ((IEnumerable)_parent.Current).GetEnumerator();
				return _child.MoveNext();
			} else
				return false;
		}
		public object Current {
			get {
				if(_child==null)
					throw new InvalidOperationException("Current property is referenced before MoveNext() is called");
				else
					return _child.Current;
			}
		}
	}

	internal class ConcatEnumerator : IEnumerator {
		private IEnumerator _first;
		private IEnumerator _second;

		public ConcatEnumerator(IEnumerator first, IEnumerator second) {
			_first = first;
			_second = second;
		}

		public object Current {
			get {
				if(_first!=null)
					return _first.Current;
				else
					return _second.Current;
			}
		}
		public bool MoveNext() {
			if(_first!=null) {
				bool r = _first.MoveNext();
				if(r) return true;
			}
			_first = null;
			return _second.MoveNext();
		}
		public void Reset() {
			throw new NotSupportedException();
		}
	}


	
	internal class CollectionUtil {
		public static IEnumerator NextEnumerator(IEnumerator e) {
			if(!e.MoveNext()) throw new InvalidOperationException("MoveNext() failed");
			return e;
		}

		public static Array DeepClone(Array src, Type type) {
			Array n = Array.CreateInstance(type, src.Length);
			for(int i=0; i<n.Length; i++) {
				object t = src.GetValue(i);
				n.SetValue(t==null? null : ((ICloneable)t).Clone(), i);
			}
			return n;
		}

		//KeyとValueを入れ替えたマップを返す。Valueがかぶっていない前提
		public static Hashtable ReverseHashtable(Hashtable src) {
			Hashtable r = new Hashtable();
			IDictionaryEnumerator ie = src.GetEnumerator();
			while(ie.MoveNext()) {
				r.Add(ie.Value, ie.Key);
			}
			return r;
		}
	}
}
