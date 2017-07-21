/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: SerializeEx.cs,v 1.2 2011/10/27 23:21:55 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace Poderosa.Serializing {
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ISerializeService {
        StructuredText Serialize(object obj);
        StructuredText Serialize(Type type, object obj); //型を明示
        object Deserialize(StructuredText node);
    }

    //ExtensionPointに接続するインタフェース。
    //ConcreteTypeに対応するオブジェクトに対して使用する。
    //扱うStructuredTextの形は
    // <ConcreteType.FullName> {
    //   ...
    // }
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ISerializeServiceElement {
        Type ConcreteType {
            get;
        }
        StructuredText Serialize(object obj);
        object Deserialize(StructuredText node);
    }
}
