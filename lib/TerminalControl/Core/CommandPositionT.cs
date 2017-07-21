/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: CommandPositionT.cs,v 1.2 2011/10/27 23:21:55 kzmi Exp $
 */
#if UNITTEST
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Diagnostics;

using NUnit.Framework;

namespace Poderosa.Commands {
    internal class PDBase : IAdaptable {
        private int _index;

        public PDBase(int index) {
            _index = index;
        }

        public int Index {
            get {
                return _index;
            }
        }
        public IAdaptable GetAdapter(Type adapter) {
            return adapter.IsInstanceOfType(this) ? this : null;
        }
    }

    internal class PD : PDBase, IPositionDesignation {
        private IAdaptable _target;
        private PositionType _position;

        public PD(int index, IAdaptable target, PositionType pos)
            : base(index) {
            _target = target;
            _position = pos;
        }

        public IAdaptable DesignationTarget {
            get {
                return _target;
            }
        }

        public PositionType DesignationPosition {
            get {
                return _position;
            }
        }

    }

    [TestFixture]
    public class CommandPositionTests {

        private static string ToResultString(ICollection r) {
            StringBuilder b = new StringBuilder();
            foreach (IAdaptable a in r)
                b.Append(((PDBase)a).Index.ToString());
            return b.ToString();
        }

        [Test]
        public void NotDesignate() {
            List<PDBase> src = new List<PDBase>();
            src.Add(new PDBase(0));
            src.Add(new PDBase(1));
            src.Add(new PDBase(2));
            Assert.AreEqual("012", ToResultString(PositionDesignationSorter.SortItems(src)));
        }

        [Test]
        public void NoDependencies1() {
            List<PD> src = new List<PD>();
            src.Add(new PD(0, null, PositionType.Last));
            src.Add(new PD(1, null, PositionType.DontCare));
            src.Add(new PD(2, null, PositionType.First));
            Assert.AreEqual("210", ToResultString(PositionDesignationSorter.SortItems(src)));
        }

        [Test]
        public void NoDependencies2() {
            List<PD> src = new List<PD>();
            src.Add(new PD(0, null, PositionType.Last));
            src.Add(new PD(1, null, PositionType.Last));
            src.Add(new PD(2, null, PositionType.DontCare));
            src.Add(new PD(3, null, PositionType.DontCare));
            src.Add(new PD(4, null, PositionType.First));
            src.Add(new PD(5, null, PositionType.First));
            Assert.AreEqual("452310", ToResultString(PositionDesignationSorter.SortItems(src)));
        }

        [Test]
        public void Dependency1() {
            List<PD> src = new List<PD>();
            src.Add(new PD(0, null, PositionType.Last));
            src.Add(new PD(1, null, PositionType.DontCare));
            src.Add(new PD(2, null, PositionType.First));
            src.Add(new PD(3, src[0], PositionType.NextTo));
            src.Add(new PD(4, src[0], PositionType.PreviousTo));
            Assert.AreEqual("21403", ToResultString(PositionDesignationSorter.SortItems(src)));
        }

        [Test]
        public void Dependency2() {
            List<PD> src = new List<PD>();
            src.Add(new PD(0, null, PositionType.Last));
            src.Add(new PD(1, null, PositionType.DontCare));
            src.Add(new PD(2, null, PositionType.First));
            src.Add(new PD(3, src[1], PositionType.PreviousTo));
            src.Add(new PD(4, src[1], PositionType.NextTo));
            Assert.AreEqual("23140", ToResultString(PositionDesignationSorter.SortItems(src)));
        }

        [Test]
        public void Dependency3() {
            List<PD> src = new List<PD>();
            src.Add(new PD(0, null, PositionType.Last));
            src.Add(new PD(1, null, PositionType.DontCare));
            src.Add(new PD(2, src[1], PositionType.PreviousTo));
            src.Add(new PD(3, src[2], PositionType.NextTo));
            Assert.AreEqual("2310", ToResultString(PositionDesignationSorter.SortItems(src)));
        }

    }


}
#endif
