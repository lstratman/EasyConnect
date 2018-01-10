/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: Win32.cs,v 1.7 2012/01/28 09:36:14 kzmi Exp $
 */
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Poderosa {
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class Win32 {

        //関数
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindowEx(
            IntPtr hwndParent,      // handle to parent window
            IntPtr hwndChildAfter,  // handle to child window
            string lpszClass,    // class name
            string lpszWindow    // window name
            );
        [DllImport("user32.dll", ExactSpelling = false, CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hwnd, char[] buf, int size);
        [DllImport("user32.dll", ExactSpelling = false, CharSet = CharSet.Auto)]
        public static extern int GetWindowModuleFileName(IntPtr hwnd, char[] buf, int size);
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern int PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        public static extern int DestroyWindow(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern IntPtr LoadIcon(IntPtr hModule, IntPtr iconName);
        // use System.Windows.Forms.SystemInformation instead
        //[DllImport("user32.dll")]
        //public static extern int GetSystemMetrics(int index);
        [DllImport("user32.dll")]
        public static extern int GetCaretBlinkTime();
        [DllImport("user32.dll")]
        public static extern int MessageBeep(int type);
        [DllImport("user32.dll")]
        public static extern short GetKeyState(int vk);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern short VkKeyScan(char ch);
        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] data);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int RegisterWindowMessage(string lpString);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int MessageBox(IntPtr hwnd, string text, string caption, int flags);
        [DllImport("user32.dll")]
        public static extern bool InvalidateRect(IntPtr hwnd, ref RECT rect, int erase);
        [DllImport("user32.dll")]
        public static extern bool InvalidateRect(IntPtr hwnd, IntPtr rect, int erase); //for invalidating all


        [DllImport("kernel32.dll")]
        public static extern int GetLastError();
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr CreateEvent(IntPtr lpSecurityAttribute, int manualReset, int initialState, string name);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr CreateMutex(IntPtr lpSecurityAttribute, int initialOwner, string name);
        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr handle);
        [DllImport("kernel32.dll")]
        public static extern bool ReleaseMutex(IntPtr handle);
        [DllImport("kernel32.dll")]
        public static extern int WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int SetEnvironmentVariable(string name, string value);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetEnvironmentVariable(string name, char[] buf, int len);



        //描画をネイティブコードに
        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        public static extern unsafe int TextOut(IntPtr hdc, int x, int y, char* text, int length);
        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        public static extern unsafe int ExtTextOut(IntPtr hdc, int x, int y, int options, RECT* lprc, char* text, int length, int* lpdx);
        [DllImport("gdi32.dll")]
        public static extern int SetBkMode(IntPtr hDC, int mode);
        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetTextExtentPoint32(IntPtr hdc, string text, int length, out SIZE size);
        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        [DllImport("gdi32.dll")]
        public static extern int DeleteObject(IntPtr hObject);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreatePen(int style, int width, uint color);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateSolidBrush(uint color);
        [DllImport("gdi32.dll")]
        public static extern int SetTextColor(IntPtr hDC, uint color);
        [DllImport("gdi32.dll")]
        public static extern int SetBkColor(IntPtr hDC, uint color);
        [DllImport("user32.dll")]
        public static extern int FillRect(IntPtr hDC, ref RECT rect, IntPtr brush);
        [DllImport("gdi32.dll")]
        public static extern int MoveToEx(IntPtr hDC, int x, int y, IntPtr prev_point/*out POINT prev*/);
        [DllImport("gdi32.dll")]
        public static extern int LineTo(IntPtr hDC, int x, int y);
        [DllImport("gdi32.dll")]
        public static extern uint SetPixel(IntPtr hDC, int x, int y, uint colorref);
        public static int MoveToEx(IntPtr hDC, int x, int y) {
            return MoveToEx(hDC, x, y, IntPtr.Zero/*NULL*/);
        }

        [DllImport("kernel32.dll")]
        public static extern bool FlushFileBuffers(IntPtr handle);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        public static extern int EnumFontFamiliesEx(
            IntPtr hdc,                          // handle to DC
            ref tagLOGFONT lpLogfont,              // font information
            EnumFontFamExProc lpEnumFontFamExProc, // callback function
            IntPtr lParam,                    // additional data
            uint dwFlags                     // not used; must be 0
            );

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr CreateFontIndirect([In] LOGFONT lf);



        [DllImport("imm32.dll")]
        public static extern IntPtr ImmGetContext(IntPtr hWnd);
        [DllImport("imm32.dll")]
        public static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);
        [DllImport("imm32.dll", CharSet = CharSet.Auto)]
        public static extern bool ImmSetCompositionFont(IntPtr hIMC, LOGFONT lf);
        [DllImport("imm32.dll")]
        public static extern bool ImmSetCompositionWindow(IntPtr hIMC, ref COMPOSITIONFORM form);
        [DllImport("imm32.dll")]
        public static extern bool ImmNotifyIME(IntPtr hIMC, int dwAction, int dwIndex, int dwValue);

        [DllImport("msvcr71.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int _controlfp(int n, int mask);
        public static void ClearFPUOverflowFlag() {
            _controlfp(0x9001f, 0xfffff); //JSPager問題の対応。情報はhttp://support.microsoft.com/default.aspx?scid=kb;en-us;326219
        }

        //定数
        public const int WM_COPYDATA = 0x4A;
        public const int WM_NOTIFY = 0x4E;
        public const int WM_NCACTIVATE = 0x0086;
        public const int WM_CHAR = 0x0102;
        public const int WM_USER = 0x400;
        public const int WM_VSCROLL = 0x115;
        public const int WM_IME_STARTCOMPOSITION = 0x010D;
        public const int WM_IME_ENDCOMPOSITION = 0x010E;
        public const int WM_IME_COMPOSITION = 0x010F;
        public const int WM_IME_CHAR = 0x0286;

        public const int TCN_FIRST = -550;
        public const int TCN_SELCHANGING = (TCN_FIRST - 2);

        public const int VK_LSHIFT = 0xA0;
        public const int VK_RSHIFT = 0xA1;
        public const int VK_LCONTROL = 0xA2;
        public const int VK_RCONTROL = 0xA3;
        public const int VK_LMENU = 0xA4;
        public const int VK_RMENU = 0xA5;

        public const uint GENERIC_READ = (0x80000000);
        public const uint GENERIC_WRITE = (0x40000000);
        public const uint OPEN_EXISTING = 3;
        public const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
        public const uint FILE_FLAG_OVERLAPPED = 0x40000000;

        public const int IDI_APPLICATION = 32512;
        public const int IDI_HAND = 32513;
        public const int IDI_QUESTION = 32514;
        public const int IDI_EXCLAMATION = 32515;
        public const int IDI_ASTERISK = 32516;

        public static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        public const int CFS_DEFAULT = 0x0000;
        public const int CFS_RECT = 0x0001;
        public const int CFS_POINT = 0x0002;
        public const int CFS_FORCE_POSITION = 0x0020;
        public const int CFS_CANDIDATEPOS = 0x0040;
        public const int CFS_EXCLUDE = 0x0080;

        public const int NI_COMPOSITIONSTR = 0x0015;
        public const int CPS_CANCEL = 0x0004;

        public const int ERROR_INVALID_HANDLE = 6;
        public const int ERROR_ALREADY_EXISTS = 183;
        public const int ERROR_OPERATION_ABORTED = 995;
        public const int ERROR_IO_PENDING = 997;
        public const int WAIT_OBJECT_0 = 0;

        public const int TRANSPARENT = 1;
        public const int OPAQUE = 2;

        public const int ETO_OPAQUE = 2;

        public const byte CLEARTYPE_QUALITY = 5;            // (_WIN32_WINNT >= 0x0500)
        public const byte CLEARTYPE_NATURAL_QUALITY = 6;    // (_WIN32_WINNT >= 0x0501)


        /*
        #define SW_HIDE             0
        #define SW_SHOWNORMAL       1
        #define SW_NORMAL           1
        #define SW_SHOWMINIMIZED    2
        #define SW_SHOWMAXIMIZED    3
        #define SW_MAXIMIZE         3
        #define SW_SHOWNOACTIVATE   4
        #define SW_SHOW             5
        #define SW_MINIMIZE         6
        #define SW_SHOWMINNOACTIVE  7
        #define SW_SHOWNA           8
        #define SW_RESTORE          9
        #define SW_SHOWDEFAULT      10
        #define SW_FORCEMINIMIZE    11
        #define SW_MAX              11
        */

        //構造体
        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public unsafe struct COPYDATASTRUCT {
            public uint dwData;
            public uint cbData;
            public void* lpData;
        }

        //WM_COPYDATAでファイルを開くときのメッセージ 数値自体に意味はない。別のアプリケーションと偶然かぶらないようにすることだけが目的
        public const int PODEROSA_OPEN_FILE_REQUEST = 7964;
        public const int PODEROSA_OPEN_FILE_OK = 485;


        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        [StructLayout(LayoutKind.Sequential)]
        public struct NMHDR {
            public IntPtr hwndFrom;
            public uint idFrom;
            public int code;
        }


        //Font#ToLogFontに渡すためにはstructではだめでclassにしないといかん
        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class LOGFONT {
            public int lfHeight;
            public int lfWidth;
            public int lfEscapement;
            public int lfOrientation;
            public int lfWeight;
            public byte lfItalic;
            public byte lfUnderline;
            public byte lfStrikeOut;
            public byte lfCharSet;
            public byte lfOutPrecision;
            public byte lfClipPrecision;
            public byte lfQuality;
            public byte lfPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string lfFaceName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct tagLOGFONT {
            public int lfHeight;
            public int lfWidth;
            public int lfEscapement;
            public int lfOrientation;
            public int lfWeight;
            public byte lfItalic;
            public byte lfUnderline;
            public byte lfStrikeOut;
            public byte lfCharSet;
            public byte lfOutPrecision;
            public byte lfClipPrecision;
            public byte lfQuality;
            public byte lfPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string lfFaceName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct ENUMLOGFONTEX {
            //LOGFONT part
            public int lfHeight;
            public int lfWidth;
            public int lfEscapement;
            public int lfOrientation;
            public int lfWeight;
            public byte lfItalic;
            public byte lfUnderline;
            public byte lfStrikeOut;
            public byte lfCharSet;
            public byte lfOutPrecision;
            public byte lfClipPrecision;
            public byte lfQuality;
            public byte lfPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string lfFaceName;
            //ENUMLOGFONTEX part
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string elfFullName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string elfStyle;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string elfScript;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct NEWTEXTMETRIC {
            public int tmHeight;
            public int tmAscent;
            public int tmDescent;
            public int tmInternalLeading;
            public int tmExternalLeading;
            public int tmAveCharWidth;
            public int tmMaxCharWidth;
            public int tmWeight;
            public int tmOverhang;
            public int tmDigitizedAspectX;
            public int tmDigitizedAspectY;
            public char tmFirschar;
            public char tmLaschar;
            public char tmDefaulchar;
            public char tmBreakChar;
            public byte tmItalic;
            public byte tmUnderlined;
            public byte tmStruckOut;
            public byte tmPitchAndFamily;
            public byte tmCharSet;
            public uint ntmFlags;
            public uint ntmSizeEM;
            public uint ntmCellHeight;
            public uint ntmAvgWidth;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        [StructLayout(LayoutKind.Sequential)]
        public struct FONTSIGNATURE {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public uint[] fsUsb;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public uint[] fsCsb;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct NEWTEXTMETRICEX {
            public NEWTEXTMETRIC ntmTm;
            public FONTSIGNATURE ntmFontSig;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT {
            public int x;
            public int y;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        [StructLayout(LayoutKind.Sequential)]
        public struct SIZE {
            public int width;
            public int height;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public RECT(int l, int t, int r, int b) {
                left = l;
                top = t;
                right = r;
                bottom = b;
            }
        }
        public static RECT CreateRect(ref Rectangle rc) {
            RECT r = new RECT();
            r.left = rc.Left;
            r.right = rc.Right;
            r.top = rc.Top;
            r.bottom = rc.Bottom;
            return r;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        [StructLayout(LayoutKind.Sequential)]
        public struct COMPOSITIONFORM {
            public int dwStyle;
            public POINT ptCurrentPos;
            public RECT rcArea;
        }


        //callbacks
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lpelfe"></param>
        /// <param name="lpntme"></param>
        /// <param name="FontType"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        /// <exclude/>
        public delegate int EnumFontFamExProc(ref ENUMLOGFONTEX lpelfe,    // logical-font data
                         ref NEWTEXTMETRICEX lpntme,  // physical-font data
                         uint FontType,           // type of font
                         IntPtr lParam             // application-defined data
                         );


        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        public class SystemMetrics {
            private int _vScrollBarWidth;
            private int _controlBorderWidth;
            private int _controlBorderHeight;

            public SystemMetrics() {
                _vScrollBarWidth = System.Windows.Forms.SystemInformation.VerticalScrollBarWidth;
                Size bs = System.Windows.Forms.SystemInformation.Border3DSize;
                _controlBorderWidth = bs.Width;
                _controlBorderHeight = bs.Height;
            }
            public int ScrollBarWidth {
                get {
                    return _vScrollBarWidth;
                }
            }
            public int ControlBorderWidth {
                get {
                    return _controlBorderWidth;
                }
            }
            public int ControlBorderHeight {
                get {
                    return _controlBorderHeight;
                }
            }
        }
    }
}
