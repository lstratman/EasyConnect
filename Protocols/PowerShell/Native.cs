using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Win32.SafeHandles;

namespace EasyConnect.Protocols.PowerShell
{
	internal class Native
	{
		internal const string Win32ERR_GetInputMode = "The Win32 internal error \"{0}\" 0x{1:X} occurred when retrieving input console handle. ";
		internal const string Win32ERR_FillConsoleOutputAttribute = "The Win32 internal error \"{0}\" 0x{1:X} occurred when filling the console output buffer with attribute. ";
		internal const string Win32ERR_FillConsoleOutputCharacter = "The Win32 internal error \"{0}\" 0x{1:X} occurred when filling the console output buffer with character. ";
		internal const string Win32ERR_FlushConsoleInputBuffer = "The Win32 internal error \"{0}\" 0x{1:X} occurred when flushing the console input buffer. ";
		internal const string Win32ERR_GetActiveScreenBufferHandle = "The Win32 internal error \"{0}\" 0x{1:X} occurred when retrieving handle for active console output buffer. ";
		internal const string Win32ERR_GetConsoleCursorInfo = "The Win32 internal error \"{0}\" 0x{1:X} occurred when getting cursor information. ";
		internal const string Win32ERR_GetConsoleScreenBufferInfo = "The Win32 internal error \"{0}\" 0x{1:X} occurred when getting console output buffer information. ";
		internal const string Win32ERR_GetConsoleWindowTitle = "The Win32 internal error \"{0}\" 0x{1:X} occurred when getting console window title. ";
		internal const string Win32ERR_GetLargestConsoleWindowSize = "The Win32 internal error \"{0}\" 0x{1:X} occurred when getting largest console window size. ";
		internal const string Win32ERR_GetMode = "The Win32 internal error \"{0}\" 0x{1:X} occurred when getting console mode. ";
		internal const string Win32ERR_GetNumberOfConsoleInputEvents = "The Win32 internal error \"{0}\" 0x{1:X} occurred when getting number of events in console input buffer. ";
		internal const string Win32ERR_PeekConsoleInput = "The Win32 internal error \"{0}\" 0x{1:X} occurred when peeking console input buffer. ";
		internal const string Win32ERR_ReadConsole = "The Win32 internal error \"{0}\" 0x{1:X} occurred when reading characters from console input buffer. ";
		internal const string Win32ERR_ReadConsoleInput = "The Win32 internal error \"{0}\" 0x{1:X} occurred when reading input records from console input buffer. ";
		internal const string Win32ERR_ReadConsoleOutput = "The Win32 internal error \"{0}\" 0x{1:X} occurred when reading console output buffer. ";
		internal const string Win32ERR_RemoveBreakHandler = "The Win32 internal error \"{0}\" 0x{1:X} occurred when removing a break handler. ";
		internal const string Win32ERR_ScrollConsoleScreenBuffer = "The Win32 internal error \"{0}\" 0x{1:X} occurred when scrolling console output buffer. ";
		internal const string Win32ERR_SetConsoleCursorInfo = "The Win32 internal error \"{0}\" 0x{1:X} occurred when setting cursor information. ";
		internal const string Win32ERR_SetConsoleCursorPosition = "The Win32 internal error \"{0}\" 0x{1:X} occurred when setting cursor position. ";
		internal const string Win32ERR_SetConsoleScreenBufferSize = "The Win32 internal error \"{0}\" 0x{1:X} occurred when setting console output buffer size. ";
		internal const string Win32ERR_SetConsoleTextAttribute = "The Win32 internal error \"{0}\" 0x{1:X} occurred when setting attributes of characters for console output buffer. ";
		internal const string Win32ERR_SetConsoleWindowInfo = "The Win32 internal error \"{0}\" 0x{1:X} occurred when setting console window information. ";
		internal const string Win32ERR_SetConsoleWindowTitle = "The Win32 internal error \"{0}\" 0x{1:X} occurred when setting console window title. ";
		internal const string Win32ERR_SetMode = "The Win32 internal error \"{0}\" 0x{1:X} occurred when setting console mode. ";
		internal const string Win32ERR_WriteConsole = "The Win32 internal error \"{0}\" 0x{1:X} occurred when writing console output buffer at current cursor position. ";
		internal const string Win32ERR_WriteConsoleOutput = "The Win32 internal error \"{0}\" 0x{1:X} occurred when writing console output buffer. ";
		internal const string Win32ERR_AddBreakHandler = "The Win32 internal error \"{0}\" 0x{1:X} occurred when adding a break handler. ";
		internal const int WM_SETICON = 128;
		internal const int WM_GETICON = 127;
		internal const int WM_SETFOCUS = 7;
		internal const int WM_KILLFOCUS = 8;
		internal const int ICON_SMALL = 0;
		internal const int ICON_BIG = 1;
		internal const int SM_CXBORDER = 5;
		internal const int SM_CXSIZEFRAME = 32;
		internal const int SM_CYBORDER = 6;
		internal const int SM_CYCAPTION = 4;
		internal const int SM_CYSIZEFRAME = 33;
		internal const int GW_HWNDFIRST = 0;
		internal const int GW_HWNDLAST = 1;
		internal const int GW_HWNDNEXT = 2;
		internal const int GW_HWNDPREV = 3;
		internal const int GW_OWNER = 4;
		internal const int GW_CHILD = 5;
		internal const int SW_HIDE = 0;
		internal const int SW_MAXIMIZE = 3;
		internal const int SW_MINIMIZE = 6;
		internal const int SW_NORMAL = 1;
		internal const int SW_SHOW = 5;
		internal const int SW_SHOWDEFAULT = 10;
		internal const int SW_SHOWMAXIMIZED = 3;
		internal const int SW_SHOWMINIMIZED = 2;
		internal const int SW_SHOWMINNOACTIVE = 7;
		internal const int SW_SHOWNA = 8;
		internal const int SW_SHOWNOACTIVATE = 4;
		internal const int SW_SHOWNORMAL = 1;
		private const int HKEY_CLASSES_ROOT = -2147483648;
		private const int HKEY_CURRENT_USER = -2147483647;
		private const int HKEY_LOCAL_MACHINE = -2147483646;
		private const int REG_NOTIFY_CHANGE_NAME = 1;
		private const int REG_NOTIFY_CHANGE_ATTRIBUTES = 2;
		private const int REG_NOTIFY_CHANGE_LAST_SET = 4;
		private const int REG_NOTIFY_CHANGE_SECURITY = 8;
		private const int REG_NOTIFY_ALL = 15;

		[DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern int ShowWindow(int hwnd, int nCmdShow);

		[DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern int GetWindowThreadProcessId(IntPtr hWnd, IntPtr lpdwProcessId);

		[DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern bool AttachThreadInput(int idAttach, int idAttachTo, bool fAttach);

		[DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern int GetDesktopWindow();

		[DllImport("user32", EntryPoint = "GetWindowTextA", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern int GetWindowText(int hwnd, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpString, int cch);

		[DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern int GetWindow(int hwnd, int wCmd);

		[DllImport("user32", EntryPoint = "GetClassNameA", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern int GetClassName(int hwnd, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpClassName, int nMaxCount);

		[DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern int SetWindowRgn(int hwnd, int hRgn, int bRedraw);

		[DllImport("user32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

		[DllImport("user32", EntryPoint = "SetWindowLongA", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern int SetWindowLong(int hwnd, int nIndex, int dwNewLong);

		[DllImport("user32", EntryPoint = "GetWindowLongA", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern int GetWindowLong(int hwnd, int nIndex);

		[DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

		[DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern int SetFocus(int hwnd);

		[DllImport("User32", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern void ReleaseCapture();

		[DllImport("user32", EntryPoint = "PostMessageA", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern int PostMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern int EnumChildWindows(int hWndParent, Native.EnumChildCallback lpEnumFunc, int lParam);

		[DllImport("user32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern int GetActiveWindow();

		[DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern int GetForegroundWindow();

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("User32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern IntPtr GetDC(IntPtr hwnd);

		[DllImport("User32.dll", CharSet = CharSet.Unicode)]
		internal static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

		[DllImport("user32.dll")]
		internal static extern bool GetWindowInfo(IntPtr hwnd, out Native.WINDOWINFO wi);

		[DllImport("user32.dll")]
		internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		internal static extern bool GetClientRect(IntPtr hWnd, out Native.RECTA lpRect);

		[DllImport("user32.dll")]
		internal static extern int GetSystemMetrics(int smIndex);

		[DllImport("user32.dll")]
		internal static extern bool GetWindowRect(IntPtr hWnd, out Native.RECTA lpRect);

		[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

		[DllImport("user32.dll")]
		public static extern int GetWindowLong(IntPtr hWnd, Native.GWL nIndex);

		[DllImport("user32.dll")]
		public static extern int SetWindowLong(IntPtr hWnd, Native.GWL nIndex, Native.WS_EX dwNewLong);

		[DllImport("user32.dll")]
		public static extern bool SetLayeredWindowAttributes(IntPtr hWnd, int crKey, byte alpha, Native.LWA dwFlags);

		[DllImport("gdi32", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern int CreateRectRgn(int X1, int Y1, int X2, int Y2);

		[DllImport("gdi32", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern int DeleteObject(int hObject);

		[DllImport("GDI32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool GetCharWidth32(IntPtr hdc, uint first, uint last, out int width);

		[DllImport("GDI32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool GetTextMetrics(IntPtr hdc, out Native.TEXTMETRIC tm);

		[DllImport("GDI32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool TranslateCharsetInfo(IntPtr src, out Native.CHARSETINFO Cs, uint options);

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern bool AttachConsole(int dwProcessId);

		[DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern bool GetConsoleSelectionInfo(ref Native.CONSOLE_SELECTION_INFO lpConsoleSelectionInfo);

		[DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern int SetConsoleCP(int wCodePageID);

		[DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern int SetConsoleOutputCP(int wCodePageID);

		[DllImport("Kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern int GetConsoleScreenBufferInfoEx(uint hConsole, ref Native.CONSOLE_SCREEN_BUFFER_INFOEX csbi);

		[DllImport("Kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern int SetConsoleScreenBufferInfoEx(uint hConsole, ref Native.CONSOLE_SCREEN_BUFFER_INFOEX csbi);

		[DllImport("Kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern int SetCurrentConsoleFontEx(uint hConsole, uint bMaximumWindow, ref Native.CONSOLE_FONT_INFOEX cfi);

		[DllImport("Kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern int GetCurrentConsoleFontEx(uint hConsole, uint bMaximumWindow, ref Native.CONSOLE_FONT_INFOEX cfi);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		internal static extern bool WriteConsoleInput(IntPtr hIn, [MarshalAs(UnmanagedType.LPStruct)] Native.KEY_INPUT_RECORD r, int count, out int countOut);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		internal static extern int GenerateConsoleCtrlEvent(int ConsoleCtrlEvent, int dwProcessGroupId);

		[DllImport("kernel32")]
		internal static extern bool AllocConsole();

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern IntPtr CreateFile(string fileName, uint desiredAccess, uint ShareModes, IntPtr securityAttributes, uint creationDisposition, uint flagsAndAttributes, IntPtr templateFileWin32Handle);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool FillConsoleOutputAttribute(IntPtr consoleOutput, ushort attribute, uint length, Native.Coord writeCoord, out uint numberOfAttrsWritten);

		[DllImport("KERNEL32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool FillConsoleOutputCharacter(IntPtr consoleOutput, char character, uint length, Native.Coord writeCoord, out uint numberOfCharsWritten);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool FlushConsoleInputBuffer(IntPtr consoleInput);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		internal static extern uint GetConsoleCP();

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool GetConsoleCursorInfo(IntPtr consoleOutput, out Native.CONSOLE_CURSOR_INFO consoleCursorInfo);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool GetConsoleMode(IntPtr consoleHandle, out uint mode);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		internal static extern uint GetConsoleOutputCP();

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool GetConsoleScreenBufferInfo(IntPtr consoleHandle, out Native.CONSOLE_SCREEN_BUFFER_INFO consoleScreenBufferInfo);

		[DllImport("KERNEL32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern uint GetConsoleTitle(StringBuilder consoleTitle, uint size);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern IntPtr GetConsoleWindow();

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern Native.Coord GetLargestConsoleWindowSize(IntPtr consoleOutput);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool GetNumberOfConsoleInputEvents(IntPtr consoleInput, out uint numberOfEvents);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern IntPtr GetStdHandle(uint handleId);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		internal static extern uint GetUserDefaultLCID();

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		internal static extern ushort GetUserDefaultUILanguage();

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool PeekConsoleInput(IntPtr consoleInput, [Out] Native.INPUT_RECORD[] buffer, uint length, out uint numberOfEventsRead);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool ReadConsole(IntPtr consoleInput, StringBuilder buffer, uint numberOfCharsToRead, out uint numberOfCharsRead, ref Native.CONSOLE_READCONSOLE_CONTROL controlData);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool ReadConsoleInput(IntPtr consoleInput, [Out] Native.INPUT_RECORD[] buffer, uint length, out uint numberOfEventsRead);

		[DllImport("KERNEL32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool ReadConsoleOutput(IntPtr consoleOutput, [Out] Native.CHAR_INFO[] buffer, Native.Coord bufferSize, Native.Coord bufferCoord, ref Native.SMALL_RECT readRegion);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool ScrollConsoleScreenBuffer(IntPtr consoleOutput, ref Native.SMALL_RECT scrollRectangle, ref Native.SMALL_RECT clipRectangle, Native.Coord destinationOrigin, ref Native.CHAR_INFO fill);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool SetConsoleCtrlHandler(Native.BreakHandler handlerRoutine, bool add);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool SetConsoleCursorInfo(IntPtr consoleOutput, ref Native.CONSOLE_CURSOR_INFO consoleCursorInfo);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool SetConsoleCursorPosition(IntPtr consoleOutput, Native.Coord cursorPosition);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool SetConsoleMode(IntPtr consoleHandle, uint mode);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool SetConsoleScreenBufferSize(IntPtr consoleOutput, Native.Coord size);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool SetConsoleTextAttribute(IntPtr consoleOutput, ushort attributes);

		[DllImport("KERNEL32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool SetConsoleTitle(string consoleTitle);

		[DllImport("KERNEL32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool SetConsoleWindowInfo(IntPtr consoleHandle, bool absolute, ref Native.SMALL_RECT windowInfo);

		[DllImport("KERNEL32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool WriteConsole(IntPtr consoleOutput, string buffer, uint numberOfCharsToWrite, out uint numberOfCharsWritten, IntPtr reserved);

		[DllImport("KERNEL32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool WriteConsoleOutput(IntPtr consoleOutput, Native.CHAR_INFO[] buffer, Native.Coord bufferSize, Native.Coord bufferCoord, ref Native.SMALL_RECT writeRegion);

		[DllImport("kernel32.dll")]
		internal static extern Native.Coord GetConsoleFontSize(IntPtr hConsoleOutput, int nFont);

		[DllImport("kernel32.dll")]
		internal static extern bool GetCurrentConsoleFont(IntPtr hConsoleOutput, bool bMaximumWindow, out Native.CONSOLE_FONT_INFO lpConsoleCurrentFont);

		[DllImport("advapi32", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern long RegNotifyChangeKeyValue(int hKey, bool bWatchSubtree, int dwNotifyFilter, int hEvent, bool fAsynchronous);

		[DllImport("advapi32.dll", EntryPoint = "RegOpenKeyA", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern int RegOpenKey(int hKey, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpSubKey, ref int phkResult);

		[DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern int RegCloseKey(int hKey);

		[DllImport("imm32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern int ImmAssociateContext(int hwnd, int himc);

		[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
		internal static extern uint SHGetFolderPath(IntPtr unused, uint nFolder, IntPtr unused1, uint dwFlags, StringBuilder pszPath);

		internal static System.Drawing.Size GetCurrentFontSize()
		{
			IntPtr hConsoleOutput = (IntPtr)typeof(Console).InvokeMember("ConsoleOutputHandle", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetProperty, (Binder)null, (object)null, (object[])null);
			Native.CONSOLE_FONT_INFO lpConsoleCurrentFont;
			Native.GetCurrentConsoleFont(hConsoleOutput, false, out lpConsoleCurrentFont);
			Native.Coord consoleFontSize = Native.GetConsoleFontSize(hConsoleOutput, lpConsoleCurrentFont.nFont);
			return new System.Drawing.Size((int)consoleFontSize.X, (int)consoleFontSize.Y);
		}

		internal static int GetWindowHorizontalOffset()
		{
			return checked(Native.GetSystemMetrics(6) + Native.GetSystemMetrics(33));
		}

		internal static Point GetWindowLocation(IntPtr windowHandle)
		{
			Native.RECTA lpRect;
			Native.GetWindowRect(windowHandle, out lpRect);
			return lpRect.Location;
		}

		internal static int GetWindowVerticalOffset()
		{
			return checked(Native.GetSystemMetrics(6) + Native.GetSystemMetrics(4) + Native.GetSystemMetrics(33));
		}

		public static void SendVKCode(int code, int repeatCount)
		{
			IntPtr stdHandle = Native.GetStdHandle(4294967286U);
			Native.KEY_INPUT_RECORD r = new Native.KEY_INPUT_RECORD();
			int num1 = 1;
			int num2 = repeatCount;
			int num3 = num1;
			while (num3 <= num2)
			{
				r.EventType = (short)1;
				r.bKeyDown = true;
				r.wRepeatCount = (short)1;
				r.wVirtualKeyCode = checked((short)code);
				r.wVirtualScanCode = (short)0;
				r.dwControlKeyState = 0;
				int countOut;
				Native.WriteConsoleInput(stdHandle, r, 1, out countOut);
				r.bKeyDown = false;
				Native.WriteConsoleInput(stdHandle, r, 1, out countOut);
				checked { ++num3; }
			}
		}

		public static void SendVKCodeUp(int code, int repeatCount)
		{
			IntPtr stdHandle = Native.GetStdHandle(4294967286U);
			Native.KEY_INPUT_RECORD r = new Native.KEY_INPUT_RECORD();
			int num1 = 1;
			int num2 = repeatCount;
			int num3 = num1;
			while (num3 <= num2)
			{
				r.EventType = (short)1;
				r.bKeyDown = false;
				r.wRepeatCount = (short)1;
				r.wVirtualKeyCode = checked((short)code);
				r.wVirtualScanCode = (short)0;
				r.dwControlKeyState = 0;
				int countOut;
				Native.WriteConsoleInput(stdHandle, r, 1, out countOut);
				checked { ++num3; }
			}
		}

		public static void SendVKCodeDown(int code, int repeatCount)
		{
			IntPtr stdHandle = Native.GetStdHandle(4294967286U);
			Native.KEY_INPUT_RECORD r = new Native.KEY_INPUT_RECORD();
			int num1 = 1;
			int num2 = repeatCount;
			int num3 = num1;
			while (num3 <= num2)
			{
				r.EventType = (short)1;
				r.bKeyDown = true;
				r.wRepeatCount = (short)1;
				r.wVirtualKeyCode = checked((short)code);
				r.wVirtualScanCode = (short)0;
				r.dwControlKeyState = 0;
				int countOut;
				Native.WriteConsoleInput(stdHandle, r, 1, out countOut);
				checked { ++num3; }
			}
		}

		internal static SafeFileHandle GetInputHandle()
		{
			SafeFileHandle safeFileHandle = new SafeFileHandle(Native.CreateFile("CONIN$", 3221225472U, 1U, IntPtr.Zero, 3U, 0U, IntPtr.Zero), true);
			if (safeFileHandle.IsInvalid)
				throw Native.CreateHostException(Marshal.GetLastWin32Error(), "RetreiveInputConsoleHandle", ErrorCategory.ResourceUnavailable, "The Win32 internal error \"{0}\" 0x{1:X} occurred when retrieving input console handle. ");
			else
				return safeFileHandle;
		}

		public static void SetFocusThread(IntPtr hwnd)
		{
			if (hwnd.Equals((object)Native.GetForegroundWindow()))
				return;
			IntPtr hWnd;
			int windowThreadProcessId = Native.GetWindowThreadProcessId(hwnd, IntPtr.Zero);
			Native.AttachThreadInput(windowThreadProcessId, 0, true);
			Native.SetForegroundWindow(hwnd);
			Native.AttachThreadInput(windowThreadProcessId, 0, false);
		}

		internal static HostException CreateHostException(int win32Error, string errorId, ErrorCategory category, string messageText)
		{
			Win32Exception win32Exception = new Win32Exception(win32Error);
			return new HostException(string.Format((IFormatProvider)Thread.CurrentThread.CurrentCulture, messageText, new object[2]
      {
        (object) win32Exception.Message,
        (object) win32Error
      }), (Exception)win32Exception, errorId, category);
		}

		public static void SendText(string command)
		{
			string str = command;
			int index = 0;
			int length = str.Length;
			while (index < length)
			{
				Native.SendChar(str[index]);
				checked { ++index; }
			}
		}

		public static void SendChar(char c)
		{
			IntPtr stdHandle = Native.GetStdHandle(4294967286U);
			Native.KEY_INPUT_RECORD r = new Native.KEY_INPUT_RECORD();
			r.EventType = (short)1;
			r.bKeyDown = true;
			r.wRepeatCount = (short)1;
			r.wVirtualKeyCode = (short)0;
			r.wVirtualScanCode = (short)0;
			r.UnicodeChar = c;
			r.dwControlKeyState = 0;
			int countOut;
			Native.WriteConsoleInput(stdHandle, r, 1, out countOut);
			r.bKeyDown = false;
			Native.WriteConsoleInput(stdHandle, r, 1, out countOut);
		}

		public static void SendCtrlC()
		{
			IntPtr stdHandle = Native.GetStdHandle(4294967286U);
			Native.KEY_INPUT_RECORD r = new Native.KEY_INPUT_RECORD();
			r.EventType = (short)1;
			r.bKeyDown = true;
			r.wRepeatCount = (short)1;
			r.wVirtualKeyCode = (short)67;
			r.wVirtualScanCode = (short)0;
			r.dwControlKeyState = 8;
			int countOut;
			Native.WriteConsoleInput(stdHandle, r, 1, out countOut);
			r.bKeyDown = false;
			Native.WriteConsoleInput(stdHandle, r, 1, out countOut);
		}

		public static void SendTab()
		{
			Native.SendChar('\t');
		}

		public static void SendNativeKey(int vk, int scancocde)
		{
		}

		public static void RunCommand(string command)
		{
			Native.SendText(command);
			Native.SendChar('\r');
		}

		public static void SendCharCode(int code)
		{
			IntPtr stdHandle = Native.GetStdHandle(4294967286U);
			Native.KEY_INPUT_RECORD r = new Native.KEY_INPUT_RECORD();
			r.EventType = (short)1;
			r.bKeyDown = true;
			r.wRepeatCount = (short)1;
			r.wVirtualKeyCode = checked((short)code);
			r.wVirtualScanCode = (short)0;
			r.UnicodeChar = char.MinValue;
			r.dwControlKeyState = 0;
			int countOut;
			Native.WriteConsoleInput(stdHandle, r, 1, out countOut);
			r.bKeyDown = false;
			Native.WriteConsoleInput(stdHandle, r, 1, out countOut);
		}

		public static void GenerateCTRL()
		{
			Native.GenerateConsoleCtrlEvent(0, 0);
		}

		[System.Flags]
		public enum SelectionFlags
		{
			NoSelection = 0,
			SelectionInProgress = 1,
			SelectionNotEmpty = 2,
			MouseSelection = 4,
			MouseDown = 8,
		}

		[System.Flags]
		internal enum AccessQualifiers : uint
		{
			GenericRead = 2147483648U,
			GenericWrite = 1073741824U,
		}

		internal enum CHAR_INFO_Attributes : uint
		{
			COMMON_LVB_LEADING_BYTE = 256U,
			COMMON_LVB_TRAILING_BYTE = 512U,
		}

		internal enum CreationDisposition : uint
		{
			CreateNew = 1U,
			CreateAlways = 2U,
			OpenExisting = 3U,
			OpenAlways = 4U,
			TruncateExisting = 5U,
		}

		[System.Flags]
		internal enum ShareModes : uint
		{
			ShareRead = 1U,
			ShareWrite = 2U,
		}

		internal enum StandardHandleId : uint
		{
			Error = 4294967284U,
			Output = 4294967285U,
			Input = 4294967286U,
		}

		internal enum ConsoleBreakSignal : uint
		{
			CtrlC = 0U,
			CtrlBreak = 1U,
			Close = 2U,
			is4 = 4U,
			Logoff = 5U,
			Shutdown = 6U,
			None = 255U,
		}

		[System.Flags]
		internal enum ConsoleModes : uint
		{
			AutoPosition = 256U,
			EchoInput = 4U,
			Extended = 128U,
			Insert = 32U,
			LineInput = 2U,
			MouseInput = 16U,
			ProcessedInput = 1U,
			ProcessedOutput = ProcessedInput,
			QuickEdit = 64U,
			WindowInput = 8U,
			WrapEndOfLine = LineInput,
			isNull = 0U,
		}

		[System.Flags]
		public enum ControlKeyStates
		{
			CapsLockOn = 128,
			EnhancedKey = 256,
			LeftAltPressed = 2,
			LeftCtrlPressed = 8,
			NumLockOn = 32,
			RightAltPressed = 1,
			RightCtrlPressed = 4,
			ScrollLockOn = 64,
			ShiftPressed = 16,
		}

		public enum ROOT_KEYS
		{
			HKEY_CLASSES_ROOT = -2147483648,
			HKEY_CURRENT_USER = -2147483647,
			HKEY_LOCAL_MACHINE = -2147483646,
			HKEY_USERS = -2147483645,
			HKEY_PERFORMANCE_DATA = -2147483644,
			HKEY_CURRENT_CONFIG = -2147483643,
			HKEY_DYN_DATA = -2147483642,
		}

		public enum NOTIFY_EVENTS
		{
			REG_NOTIFY_CHANGE_NAME = 1,
			REG_NOTIFY_CHANGE_ATTRIBUTES = 2,
			REG_NOTIFY_CHANGE_LAST_SET = 4,
			REG_NOTIFY_CHANGE_SECURITY = 8,
		}

		public enum CursorType
		{
			Off,
			SingleLine,
			Block,
		}

		public enum GWL
		{
			ExStyle = -20,
		}

		public enum WS_EX
		{
			Transparent = 32,
			Layered = 524288,
		}

		public enum LWA
		{
			ColorKey = 1,
			Alpha = 2,
		}

		public struct CONSOLE_SELECTION_INFO
		{
			public Native.SelectionFlags dwFlags;
			public Native.Coord dwSelectionAnchor;
			public Native.SMALL_RECT srSelection;
		}

		internal struct SMALL_RECT
		{
			internal short Left;
			internal short Top;
			internal short Right;
			internal short Bottom;

			public override string ToString()
			{
				return string.Format((IFormatProvider)CultureInfo.InvariantCulture, "{0},{1},{2},{3}", (object)this.Left, (object)this.Top, (object)this.Right, (object)this.Bottom);
			}
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct TEXTMETRIC
		{
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
			public char tmFirstChar;
			public char tmLastChar;
			public char tmDefaultChar;
			public char tmBreakChar;
			public byte tmItalic;
			public byte tmUnderlined;
			public byte tmStruckOut;
			public byte tmPitchAndFamily;
			public byte tmCharSet;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal class KEY_INPUT_RECORD
		{
			public short EventType;
			public bool bKeyDown;
			public short wRepeatCount;
			public short wVirtualKeyCode;
			public short wVirtualScanCode;
			public char UnicodeChar;
			public int dwControlKeyState;
		}

		public struct INPUT_RECORD
		{
			internal ushort EventType;
			internal Native.KEY_EVENT_RECORD KeyEvent;
		}

		public struct KEY_EVENT_RECORD
		{
			internal bool KeyDown;
			internal ushort RepeatCount;
			internal ushort VirtualKeyCode;
			internal ushort VirtualScanCode;
			internal char UnicodeChar;
			internal uint ControlKeyState;
		}

		public struct RECT
		{
			public int left;
			public int top;
			public int right;
			public int bottom;
		}

		public struct WINDOWINFO
		{
			public int cbSize;
			public Native.RECT rcWindow;
			public Native.RECT rcClient;
			public int dwStyle;
			public int dwExStyle;
			public int dwWindowStatus;
			public int cxWindowBorders;
			public int cyWindowBorders;
			public short atomWindowType;
			public short wCreatorVersion;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct CONSOLE_FONT_INFO_EX
		{
			public int cbSize;
			public int nFont;
			public Native.Coord dwFontSize;
			public short FontFamily;
			public short FontWeight;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string FaceName;
		}

		public struct CONSOLE_FONT_INFO
		{
			public int nFont;
			public Native.Coord dwFontSize;
		}

		public struct Coord
		{
			public short X;
			public short Y;
		}

		public struct RECTA
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;

			public int Height
			{
				get
				{
					return checked(this.Bottom - this.Top);
				}
			}

			public Point Location
			{
				get
				{
					return new Point(this.Left, this.Top);
				}
			}

			public System.Drawing.Size Size
			{
				get
				{
					return new System.Drawing.Size(this.Width, this.Height);
				}
			}

			public int Width
			{
				get
				{
					return checked(this.Right - this.Left);
				}
			}

			public RECTA(int left_, int top_, int right_, int bottom_)
			{
				this = new Native.RECTA();
				this.Left = left_;
				this.Top = top_;
				this.Right = right_;
				this.Bottom = bottom_;
			}

			public static implicit operator System.Drawing.Rectangle(Native.RECTA rect)
			{
				return rect.ToRectangle();
			}

			public static implicit operator Native.RECTA(System.Drawing.Rectangle rect)
			{
				return Native.RECTA.FromRectangle(rect);
			}

			public static Native.RECTA FromRectangle(System.Drawing.Rectangle rectangle)
			{
				return new Native.RECTA(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
			}

			public override int GetHashCode()
			{
				return this.Left ^ (this.Top << 13 | this.Top >> 19) ^ (this.Width << 26 | this.Width >> 6) ^ (this.Height << 7 | this.Height >> 25);
			}

			public System.Drawing.Rectangle ToRectangle()
			{
				return System.Drawing.Rectangle.FromLTRB(this.Left, this.Top, this.Right, this.Bottom);
			}
		}

		public struct CONSOLE_CURSOR_INFO
		{
			internal uint Size;
			internal bool Visible;

			public override string ToString()
			{
				return string.Format((IFormatProvider)CultureInfo.InvariantCulture, "Size: {0}, Visible: {1}", new object[2]
        {
          (object) this.Size,
          (object) (bool) (this.Visible)
        });
			}
		}

		public struct CONSOLE_SCREEN_BUFFER_INFO
		{
			internal Native.Coord BufferSize;
			internal Native.Coord CursorPosition;
			internal ushort Attributes;
			internal Native.SMALL_RECT WindowRect;
			internal Native.Coord MaxWindowSize;
			internal uint Padding;
		}

		public struct CHAR_INFO
		{
			internal ushort UnicodeChar;
			internal ushort Attributes;
		}

		public struct CONSOLE_READCONSOLE_CONTROL
		{
			internal uint nLength;
			internal uint nInitialChars;
			internal uint dwCtrlWakeupMask;
			internal uint dwControlKeyState;
		}

		public struct CHARSETINFO
		{
			internal uint ciCharset;
			internal uint ciACP;
			internal Native.FONTSIGNATURE fs;
		}

		public struct FONTSIGNATURE
		{
			internal uint fsUsb0;
			internal uint fsUsb1;
			internal uint fsUsb2;
			internal uint fsUsb3;
			internal uint fsCsb0;
			internal uint fsCsb1;
		}

		public struct KeyInfo
		{
			private int _virtualKeyCode;
			private char _character;
			private Native.ControlKeyStates _controlKeyState;
			private bool _keyDown;

			public int VirtualKeyCode
			{
				get
				{
					return this._virtualKeyCode;
				}
				set
				{
					this._virtualKeyCode = value;
				}
			}

			public char Character
			{
				get
				{
					return this._character;
				}
				set
				{
					this._character = value;
				}
			}

			public Native.ControlKeyStates ControlKeyState
			{
				get
				{
					return this._controlKeyState;
				}
				set
				{
					this._controlKeyState = value;
				}
			}

			public bool KeyDown
			{
				get
				{
					return this._keyDown;
				}
				set
				{
					this._keyDown = value;
				}
			}

			public KeyInfo(int virtualKeyCode, char ch, Native.ControlKeyStates controlKeyState, bool keyDown)
			{
				this = new Native.KeyInfo();
				this.VirtualKeyCode = virtualKeyCode;
				this.Character = ch;
				this.ControlKeyState = controlKeyState;
				this.KeyDown = keyDown;
			}

			public static bool operator ==(Native.KeyInfo first, Native.KeyInfo second)
			{
				return (int)first.Character == (int)second.Character && first.ControlKeyState == second.ControlKeyState && (first.KeyDown == second.KeyDown && first.VirtualKeyCode == second.VirtualKeyCode);
			}

			public static bool operator !=(Native.KeyInfo first, Native.KeyInfo second)
			{
				return !(first == second);
			}

			public override string ToString()
			{
				return string.Format((IFormatProvider)CultureInfo.InvariantCulture, "{0},{1},{2},{3}", (object)this.VirtualKeyCode, (object)this.Character, (object)this.ControlKeyState, (object)(bool)(this.KeyDown));
			}

			public override bool Equals(object obj)
			{
				bool flag = false;
				if (obj is Native.KeyInfo)
					flag = this == (Native.KeyInfo)obj;
				return flag;
			}

			public override int GetHashCode()
			{
				return (checked((uint)((long)(uint)((long)Conversions.ToUInteger(Interaction.IIf(this.KeyDown, (object)268435456, (object)0)) | (long)(unchecked((int)this._controlKeyState) << 16)) | (long)this._virtualKeyCode))).GetHashCode();
			}
		}

		internal struct CURSOR_INFO
		{
			internal int Size;
			internal bool Visible;
		}

		public struct CONSOLE_SCREEN_BUFFER_INFOEX
		{
			public uint cbSize;
			public Native.Coord dwSize;
			public Native.Coord dwCursorPosition;
			public ushort wAttributes;
			public Native.SMALL_RECT srWindow;
			public Native.Coord dwMaximumWindowSize;
			public ushort wPopupAttributes;
			public uint bFullscreenSupported;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
			public uint[] ColorTable;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct CONSOLE_FONT_INFOEX
		{
			public uint cbSize;
			public uint nFont;
			public Native.Coord dwFontSize;
			public uint FontFamily;
			public uint FontWeight;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string FaceName;
		}

		internal struct SHELLEXECUTEINFO
		{
			public int cbSize;
			public int fMask;
			public int hwnd;
			public string lpVerb;
			public string lpFile;
			public string lpParameters;
			public string lpDirectory;
			public int nShow;
			public int hInstApp;
			public int lpIDList;
			public string lpClass;
			public int hkeyClass;
			public int dwHotKey;
			public int hIcon;
			public int hProcess;
		}

		internal delegate bool BreakHandler(Native.ConsoleBreakSignal ConsoleBreakSignal);

		internal delegate bool EnumChildCallback(int hWnd, int lParam);
	}
}
