/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: Util.cs,v 1.2 2011/10/27 23:21:59 kzmi Exp $
 */
using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace Poderosa.UI {
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class UIUtil {
        public static int AdjustRange(int value, int min, int max) {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;

            return value;
        }

        public static void ReplaceControl(Control parent, Control src, Control dest) {
            Debug.Assert(src.Parent == parent);
            Size size = src.Size;
            DockStyle dock = src.Dock;
            Point location = src.Location;

            Control[] t = new Control[parent.Controls.Count];
            for (int i = 0; i < t.Length; i++) {
                Control c = parent.Controls[i];
                t[i] = c == src ? dest : c;
            }
            dest.Dock = dock;
            dest.Size = size;
            dest.Location = location;
            parent.Controls.Clear();
            parent.Controls.AddRange(t);
            Debug.Assert(parent.Controls.Contains(dest));
        }

        public static void DumpControlTree(Control t) {
            DumpControlTree(t, 0);
        }

        private static void DumpControlTree(Control t, int indent) {
            StringBuilder bld = new StringBuilder();
            for (int i = 0; i < indent; i++)
                bld.Append(' ');
            bld.Append(t.GetType().Name);
            bld.Append(" Size=");
            bld.Append(t.Size.ToString());
            bld.Append(" Dock=");
            bld.Append(t.Dock.ToString());
            Debug.WriteLine(bld.ToString());
            foreach (Control c in t.Controls)
                DumpControlTree(c, indent + 1);
        }
    }

    /// <summary>
    /// Utility methods for creating icon.
    /// </summary>
    public static class IconUtil {

        /// <summary>
        /// Creates a monotone icon from alpha channel of the specified image and the specified color.
        /// </summary>
        /// <param name="mask">image which has alpha channel</param>
        /// <param name="color">icon color</param>
        /// <returns>new icon image</returns>
        public static Bitmap CreateColoredIcon(Image mask, Color color) {
            Bitmap bmp = new Bitmap(mask.Width, mask.Height, PixelFormat.Format32bppArgb);
            var rval = color.R / 255f;
            var gval = color.G / 255f;
            var bval = color.B / 255f;
            using (var attr = new ImageAttributes()) {
                attr.SetColorMatrix(new ColorMatrix(new float[][] {
                    new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f },
                    new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f },
                    new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f },
                    new float[] { 0.0f, 0.0f, 0.0f, 1.0f, 0.0f },
                    new float[] { rval, gval, bval, 0.0f, 1.0f },
                }));
                using (var g = Graphics.FromImage(bmp)) {
                    g.DrawImage(mask, new Rectangle(0, 0, mask.Width, mask.Height), 0, 0, mask.Width, mask.Height, GraphicsUnit.Pixel, attr);
                }
            }
            return bmp;
        }


    }

}
