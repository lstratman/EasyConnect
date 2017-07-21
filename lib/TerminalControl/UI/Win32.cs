/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: Win32.cs,v 1.1 2010/11/19 15:41:20 kzmi Exp $
 */
using System;
using System.Runtime.InteropServices;

namespace Poderosa.UI {
    internal class Win32 {
        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        [DllImport("gdi32.dll")]
        public static extern int DeleteObject(IntPtr hObject);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreatePen(int style, int width, uint color);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateSolidBrush(uint color);
        [DllImport("gdi32.dll")]
        public static extern int MoveToEx(IntPtr hDC, int x, int y, IntPtr prev_point/*out POINT prev*/);
        public static int MoveToEx(IntPtr hDC, int x, int y) {
            return MoveToEx(hDC, x, y, IntPtr.Zero/*NULL*/);
        }
        [DllImport("gdi32.dll")]
        public static extern int LineTo(IntPtr hDC, int x, int y);
        [DllImport("gdi32.dll")]
        public static extern uint SetPixel(IntPtr hDC, int x, int y, uint colorref);
    }
}
