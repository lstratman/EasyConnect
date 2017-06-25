/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: GenericsUtil.cs,v 1.2 2011/10/27 23:21:55 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace Poderosa.Util.Generics {
    //ToString, Parse, Equalsの３つを備える。PreferenceItem用に導入された。
    //bool, int, stringについてはここに列挙だが、Enum用には各自で。
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <exclude/>
    public interface IPrimitiveAdapter<T> {
        string ToString(T value);
        T Parse(string value);
        bool Equals(T v1, T v2);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    /// 
    public class BoolPrimitiveAdapter : IPrimitiveAdapter<bool> {
        public string ToString(bool value) {
            return value.ToString();
        }

        public bool Parse(string value) {
            return Boolean.Parse(value);
        }

        public bool Equals(bool v1, bool v2) {
            return v1 == v2;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class IntPrimitiveAdapter : IPrimitiveAdapter<int> {
        public string ToString(int value) {
            return value.ToString();
        }

        public int Parse(string value) {
            return Int32.Parse(value);
        }

        public bool Equals(int v1, int v2) {
            return v1 == v2;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class StringPrimitiveAdapter : IPrimitiveAdapter<string> {
        public string ToString(string value) {
            return value;
        }

        public string Parse(string value) {
            return value;
        }

        public bool Equals(string v1, string v2) {
            return v1 == v2;
        }
    }
}
