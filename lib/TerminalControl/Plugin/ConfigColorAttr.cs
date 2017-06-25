/*
 Copyright (c) 2005 Poderosa Project, All Rights Reserved.

 $Id: ConfigColorAttr.cs,v 1.2 2011/10/27 23:21:56 kzmi Exp $
*/
using System;
using System.Diagnostics;
using System.Reflection;
using System.Drawing;

namespace Poderosa {
#if false
    //Colorを直接Attributeの定義に使うことはできない！
    public enum LateBindColors {
        Empty,
        Window,
        WindowText
    }
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ConfigColorElementAttribute : ConfigElementAttribute {
        private LateBindColors _initial;
        public LateBindColors Initial {
            get {
                return _initial;
            }
            set {
                _initial = value;
            }
        }
        private static Color ToColor(LateBindColors value) {
            switch (value) {
                case LateBindColors.Empty:
                    return Color.Empty;
                case LateBindColors.Window:
                    return SystemColors.Window;
                case LateBindColors.WindowText:
                    return SystemColors.WindowText;
            }
            Debug.Assert(false, "should not reach here");
            return Color.Empty;
        }
#if false
        //BACK-BURNER
        public override void ExportTo(object holder, ConfigNode node) {
            Color value = (Color)_fieldInfo.GetValue(holder);
            if (value != ToColor(_initial))
                node[_externalName] = value.Name;
        }
        public override void ImportFrom(object holder, ConfigNode node) {
            _fieldInfo.SetValue(holder, GUtil.ParseColor(node[_externalName], ToColor(_initial)));
        }
#endif
        public override void Reset(object holder) {
            _fieldInfo.SetValue(holder, ToColor(_initial));
        }
    }
#endif
}
