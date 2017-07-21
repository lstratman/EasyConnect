/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: Serialize.cs,v 1.2 2011/10/27 23:21:55 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Poderosa.Util.Collections;
using Poderosa.Plugins;
using Poderosa.View;

[assembly: PluginDeclaration(typeof(Poderosa.Serializing.SerializeServicePlugin))]

namespace Poderosa.Serializing {
    //StructuredTextを使ってオブジェクトをシリアライズするための仕組み
    [PluginInfo(ID = SerializeServicePlugin.PLUGIN_ID, Version = VersionInfo.PODEROSA_VERSION, Author = VersionInfo.PROJECT_NAME)]
    internal class SerializeServicePlugin : PluginBase, ISerializeService {
        public const string PLUGIN_ID = "org.poderosa.core.serializing";
        public const string EXTENSIONPOINT_NAME = "org.poderosa.core.serializeElement";
        private IExtensionPoint _serviceElements;
        private TypedHashtable<string, ISerializeServiceElement> _nameToSerializer;

        public override void InitializePlugin(IPoderosaWorld poderosa) {
            base.InitializePlugin(poderosa);
            _serviceElements = poderosa.PluginManager.CreateExtensionPoint(EXTENSIONPOINT_NAME, typeof(ISerializeServiceElement), this);
            //_typeToElement = new TypedHashtable<Type, ISerializeServiceElement>();
            //RenderProfileはこのアセンブリなので登録してしまう
            _serviceElements.RegisterExtension(new RenderProfileSerializer());
        }

        public StructuredText Serialize(object obj) {
            return Serialize(obj.GetType(), obj);
        }
        public StructuredText Serialize(Type type, object obj) {
            ISerializeServiceElement se = FindServiceElement(type.FullName
                );
            if (se == null)
                throw new ArgumentException("ISerializeServiceElement is not found for the class " + obj.GetType().FullName);

            StructuredText t = se.Serialize(obj);
            return t;
        }

        public object Deserialize(StructuredText node) {
            ISerializeServiceElement se = FindServiceElement(node.Name);
            if (se == null)
                throw new ArgumentException("ISerializeServiceElement is not found for the tag " + node.Name);

            object t = se.Deserialize(node);
            Debug.Assert(t.GetType() == se.ConcreteType);
            return t;
        }

        //型から検索する。まあ個数は多くないだろうからリニアサーチでいいだろう。どうしても、という場合にはこの中でキャッシュするくらいか
        private ISerializeServiceElement FindServiceElement(string tag) {
            if (_nameToSerializer == null) {
                _nameToSerializer = new TypedHashtable<string, ISerializeServiceElement>();
                ISerializeServiceElement[] t = (ISerializeServiceElement[])_serviceElements.GetExtensions();
                foreach (ISerializeServiceElement e in t) {
                    _nameToSerializer.Add(e.ConcreteType.FullName, e);
                }
            }
            return _nameToSerializer[tag];
        }
    }
}
