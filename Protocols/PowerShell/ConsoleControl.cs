using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using Rectangle = System.Drawing.Rectangle;
using Size = System.Drawing.Size;
using Timer = System.Windows.Forms.Timer;

namespace EasyConnect.Protocols.PowerShell
{
	public class ConsoleControl : UserControl
	{
		#region Delegates
		public delegate void QuickEditModeChangedEventHandler(bool active);

		public delegate void OnAltEnterEventHandler();

		public delegate void OnAltCaptureEventHandler(ref bool handled);

		public delegate void OnAltEventHandler();

		public delegate void OnClickEventHandler();

		public delegate void OnClipCopyEventHandler();

		public delegate void OnCtrlAltEventHandler();

		public delegate void OnCtrlCEventHandler(ref bool cancel);

		public delegate void OnCursorKeyEventHandler(Keys keycode, ref bool cancel);

		public delegate void OnDoubleClickEventHandler();

		public delegate void OnEscEventHandler();

		public delegate void OnEnterEventHandler();

		public delegate void OnExitEventHandler();

		public delegate void OnFunctionKeyEventHandler(Keys keycode, ref bool cancel);

		public delegate void OnKeepFocusEventHandler(bool mode);

		public delegate void OnLicenseAddedEventHandler(string path);

		public delegate void OnMenuRequestEventHandler(Request request, ref int data);

		public delegate void OnMiniModeAltEventHandler();

		public delegate void OnMinimizeEventHandler();

		public delegate void OnMinimodeEndEventHandler();

		public delegate void OnOneQuickEditDoneEventHandler();

		public delegate void OnPrintScreenEventHandler();

		public delegate void OnPropertyPageExitEventHandler();

		public delegate void OnPropertyPageShowEventHandler(ref bool cancel);

		public delegate void OnScrollBarEventHandler(bool barX, bool barY);

		public delegate void OnShiftAltEventHandler();

		public delegate void OnSpaceAltEventHandler();

		public delegate void OnTitleChangeEventHandler(string text);

		public delegate void OnToggleWindowModeEventHandler();

		public delegate void OnTriggerEventHandler(string key, ref bool cancel);
		#endregion

		#region Request enum
		public enum Request
		{
			ExitMiniMode = 1,
			GetHwnd,
			SetOpacity,
			GetOpacity,
			SetAlywaysOnTop,
			GetAlwaysOnTop,
			ResetVariableToolWindows,
			GetVariableState
		}
		#endregion

		internal const short WINEVENT_SKIPOWNPROCESS = 2;

		private static int m_ConsoleWidth = 20;
		private QuickEditModeChangedEventHandler QuickEditModeChangedEvent;

		[AccessedThroughProperty("AdjustWidthToolStripMenuItem")]
		private ToolStripMenuItem _AdjustWidthToolStripMenuItem;

		[AccessedThroughProperty("ContextMenuStrip1")]
		private ContextMenuStrip _contextMenu;

		[AccessedThroughProperty("HScrollBar1")]
		private HScrollBar _horizontalScrollBar;

		[AccessedThroughProperty("ImageList1")]
		private ImageList _ImageList1;

		private string _KeyToRegProfile;

		[AccessedThroughProperty("MinimodeToolStripMenuItem1")]
		private ToolStripMenuItem _MinimodeToolStripMenuItem1;

		[AccessedThroughProperty("Panel1")]
		private Panel _Panel1;

		[AccessedThroughProperty("PasteToolStripMenuItem")]
		private ToolStripMenuItem _pasteMenuItem;

		[AccessedThroughProperty("PropertiesToolStripMenuItem")]
		private ToolStripMenuItem _propertiesMenuItem;

		private string _PropertyClass;
		private bool _PropertyFound;
		private string _PropertyName;

		[AccessedThroughProperty("PropertyWatcher")]
		private Timer _PropertyWatcher;

		[AccessedThroughProperty("SelectToolStripMenuItem")]
		private ToolStripMenuItem _SelectToolStripMenuItem;

		[AccessedThroughProperty("Timer1")]
		private Timer _Timer1;

		[AccessedThroughProperty("Timer2")]
		private Timer _Timer2;

		[AccessedThroughProperty("ToolStripSeparator2")]
		private ToolStripSeparator _ToolStripSeparator2;

		[AccessedThroughProperty("ToolStripSeparator3")]
		private ToolStripSeparator _ToolStripSeparator3;

		[AccessedThroughProperty("VScrollBar1")]
		private VScrollBar _verticalScrollBar;

		private int _consoleBack;
		private Color[] _consoleColor;
		private int _consoleFront;
		private uint[] _consoleTable;

		private string _currentFontFace;
		private Native.Coord _currentFontSize;
		private bool _isDragging;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		private bool _isEmbedConsole;

		private bool _isOneClickQuickEdit;
		private bool _isTemporaryProfile;
		private bool _layoutsuspended;

		[AccessedThroughProperty("lblConsoleSize")]
		private Label _lblConsoleSize;

		private bool _moveform;
		private int _offsetFrame;
		private int _offsetX;
		private int _offsetY;

		private int _oldBufferX;
		private int _oldBuffery;
		private string _oldTitle;
		private int _oldWindowBottom;
		private int _oldWindowLeft;
		private int _oldWindowPosition;
		private int _oldWindowRight;
		private int _oldWindowTop;

		[AccessedThroughProperty("EventsPanel")]
		private SpecComponent _eventsPanel;

		[AccessedThroughProperty("ContainerPanel")]
		private Panel _containerPanel;

		[AccessedThroughProperty("ConsolePanel")]
		private Panel _consolePanel;

		[AccessedThroughProperty("p6")]
		private Panel _p6;

		private RegistryKey _regKeyProfile;

		private object _sl_adjustblank;
		private object _updateSizer;
		private int _windowHeight;
		private int _windowWidth;
		private IContainer components;
		private EnumWindows ew;
		private int hwnd;

		[MarshalAs(UnmanagedType.FunctionPtr)]
		internal EventFunc mEventFunc;

		private int oldX;
		private int oldY;
		private OnAltEnterEventHandler onALTENTEREvent;
		private OnAltCaptureEventHandler _onAltCaptureEvent;
		private OnAltEventHandler _onAltEvent;
		private OnClickEventHandler _onClickEvent;
		private OnClipCopyEventHandler _onClipCopyEvent;
		private OnCtrlAltEventHandler _onCtrlAltEvent;
		private OnCtrlCEventHandler _onCtrlCEvent;
		private OnCursorKeyEventHandler _onCursorKeyEvent;
		private OnDoubleClickEventHandler _onDoubleClickEvent;
		private OnEscEventHandler _onEscEvent;
		private OnEnterEventHandler _onEnterEvent;
		private OnExitEventHandler _onExitEvent;
		private OnFunctionKeyEventHandler _onFunctionKeyEvent;
		private OnKeepFocusEventHandler _onKeepFocusEvent;
		private OnLicenseAddedEventHandler _onLicenseAddedEvent;
		private OnMenuRequestEventHandler onMenuRequestEvent;
		private OnMiniModeAltEventHandler onMiniModeAltEvent;
		private OnMinimizeEventHandler onMinimizeEvent;
		private OnMinimodeEndEventHandler onMinimodeEndEvent;
		private OnOneQuickEditDoneEventHandler onOneQuickEditDoneEvent;
		private OnPrintScreenEventHandler onPrintScreenEvent;
		private OnPropertyPageExitEventHandler onPropertyPageExitEvent;
		private OnPropertyPageShowEventHandler onPropertyPageShowEvent;
		private OnScrollBarEventHandler onScrollBarEvent;
		private OnShiftAltEventHandler onShiftAltEvent;
		private OnSpaceAltEventHandler _onSpaceAltEvent;
		private OnTitleChangeEventHandler _onTitleChangeEvent;
		private OnToggleWindowModeEventHandler _onToggleWindowModeEvent;
		private OnTriggerEventHandler _onTriggerEvent;
		private Size s;

		private int safetycount;
		private Dictionary<int, string> _windowMap;
		private bool _shiftToSelect;
		private bool _shiftSelectActive = false;

		public ConsoleControl()
		{
			Load += PSControl_Load;
			OnCtrlC += ConsoleControl_onCtrlC;
			Resize += PSControl_Resize;
			_windowWidth = 80;
			_windowHeight = 25;
			_isDragging = false;
			_currentFontFace = "";
			_oldBufferX = -1;
			_oldBuffery = -1;
			_oldWindowLeft = -1;
			_oldWindowRight = -1;
			_oldWindowTop = -1;
			_oldWindowBottom = -1;
			_layoutsuspended = false;
			_updateSizer = RuntimeHelpers.GetObjectValue(new object());
			_sl_adjustblank = RuntimeHelpers.GetObjectValue(new object());
			HasFocus = false;
			_isOneClickQuickEdit = false;
			hwnd = 0;
			checked
			{
				_offsetX = Native.GetWindowHorizontalOffset();
				_offsetY = Native.GetWindowVerticalOffset();
				_offsetFrame = 0;
				_moveform = false;
				_PropertyFound = false;
				_PropertyClass = "#32770";
				_PropertyName = "";
				MinSize = new Size(20, 1);
				CanAutoAdjust = false;
				_KeyToRegProfile = "";
				_isTemporaryProfile = false;
				_isEmbedConsole = false;
				ew = new EnumWindows();
				oldX = 0;
				oldY = 0;
				safetycount = 0;
				PreStart();
				LockWindowUpdate(GetDesktopWindow());
				Native.AllocConsole();
				IntPtr intPtr = IntPtr.Zero;
				int num = 1;
				do
				{
					intPtr = Native.GetConsoleWindow();
					if (!(intPtr == IntPtr.Zero))
						break;
					num++;
				} while (num <= 50);
				LockWindowUpdate(0);
				Native.ShowWindow((int)intPtr, 0);
				Native.ImmAssociateContext((int)Native.GetConsoleWindow(), 0);
				InitializeComponent();
				PostStart();
			}
		}

		internal static Native.ConsoleModes ConsoleMode
		{
			get
			{
				return GetMode(GetInputHandle());
			}
			set
			{
				SetMode(GetInputHandle(), value);
			}
		}

		internal static int NewConsoleWidth
		{
			get
			{
				return m_ConsoleWidth;
			}
			set
			{
				if (value >= 1)
					m_ConsoleWidth = value;
				if (m_ConsoleWidth < 20)
					m_ConsoleWidth = 20;
			}
		}

		internal virtual Panel ContainerPanel
		{
			get
			{
				return _containerPanel;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				_containerPanel = value;
			}
		}

		internal virtual Panel ConsolePanel
		{
			get
			{
				return _consolePanel;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				_consolePanel = value;
			}
		}

		internal virtual Timer PropertyWatcher
		{
			get
			{
				return _PropertyWatcher;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler value2 = PropertyWatcher_Tick;
				if (_PropertyWatcher != null)
					_PropertyWatcher.Tick -= value2;
				_PropertyWatcher = value;
				if (_PropertyWatcher != null)
					_PropertyWatcher.Tick += value2;
			}
		}

		internal virtual SpecComponent EventsPanel
		{
			get
			{
				return _eventsPanel;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				SpecComponent.PageDownEventHandler obj = p1_PageDown;
				SpecComponent.PageUpEventHandler obj2 = p1_PageUp;
				SpecComponent.onClickEventHandler obj3 = p1_onClick;
				SpecComponent.onClipInsertEventHandler obj4 = TransparentPanel1_onClipInsert;
				SpecComponent.onEnterEventHandler obj5 = TransparentPanel1_onEnter;
				SpecComponent.onALTENTEREventHandler obj6 = TransparentPanel1_onALTENTER;
				SpecComponent.onCursorKeyEventHandler obj7 = TransparentPanel1_onCursorKey;
				SpecComponent.onFunctionKeyEventHandler obj8 = TransparentPanel1_onFunctionKey;
				SpecComponent.onTriggerEventHandler obj9 = TransparentPanel1_onTrigger;
				DragEventHandler value2 = TransparentPanel1_DragEnter;
				DragEventHandler value3 = TransparentPanel1_DragDrop;
				SpecComponent.onAltUpEventHandler obj10 = TransparentPanel1_onAlt;
				MouseEventHandler value4 = TransparentPanel1_MouseDown;
				EventHandler value5 = TransparentPanel1_GotFocus;
				EventHandler value6 = TransparentPanel1_DoubleClick;
				EventHandler value7 = TransparentPanel1_Click;
				EventHandler value8 = p1_LostFocus;
				SpecComponent.onSpaceAltEventHandler obj11 = p1_onSpaceAlt;
				SpecComponent.onShiftAltEventHandler obj12 = p1_onShiftAlt;
				SpecComponent.onCtrlAltEventHandler obj13 = p1_onCtrlAlt;
				SpecComponent.onAltCaptureEventHandler obj14 = p1_onAltCapture;
				SpecComponent.onCtrlCEventHandler obj15 = p1_onCtrlC;
				SpecComponent.onPrintScreenEventHandler obj16 = p1_onPrintScreen;
				SpecComponent.onSendCharEventHandler obj17 = p1_onSendChar;
				SpecComponent.onESCEventHandler obj18 = p1_onESC;
				SpecComponent.onClipCopyEventHandler obj19 = p1_onClipCopy;
				if (_eventsPanel != null)
				{
					_eventsPanel.PageDown -= obj;
					_eventsPanel.PageUp -= obj2;
					_eventsPanel.onClick -= obj3;
					_eventsPanel.onClipInsert -= obj4;
					_eventsPanel.onEnter -= obj5;
					_eventsPanel.onALTENTER -= obj6;
					_eventsPanel.onCursorKey -= obj7;
					_eventsPanel.onFunctionKey -= obj8;
					_eventsPanel.onTrigger -= obj9;
					_eventsPanel.DragEnter -= value2;
					_eventsPanel.DragDrop -= value3;
					_eventsPanel.onAltUp -= obj10;
					_eventsPanel.MouseDown -= value4;
					_eventsPanel.GotFocus -= value5;
					_eventsPanel.DoubleClick -= value6;
					_eventsPanel.Click -= value7;
					_eventsPanel.LostFocus -= value8;
					_eventsPanel.onSpaceAlt -= obj11;
					_eventsPanel.onShiftAlt -= obj12;
					_eventsPanel.onCtrlAlt -= obj13;
					_eventsPanel.onAltCapture -= obj14;
					_eventsPanel.onCtrlC -= obj15;
					_eventsPanel.onPrintScreen -= obj16;
					_eventsPanel.onSendChar -= obj17;
					_eventsPanel.onESC -= obj18;
					_eventsPanel.onClipCopy -= obj19;
					_eventsPanel.onShiftDown -= EventsPanelOnShiftDown;
					_eventsPanel.onShiftUp -= _eventsPanel_onShiftUp;
				}
				_eventsPanel = value;
				if (_eventsPanel != null)
				{
					_eventsPanel.PageDown += obj;
					_eventsPanel.PageUp += obj2;
					_eventsPanel.onClick += obj3;
					_eventsPanel.onClipInsert += obj4;
					_eventsPanel.onEnter += obj5;
					_eventsPanel.onALTENTER += obj6;
					_eventsPanel.onCursorKey += obj7;
					_eventsPanel.onFunctionKey += obj8;
					_eventsPanel.onTrigger += obj9;
					_eventsPanel.DragEnter += value2;
					_eventsPanel.DragDrop += value3;
					_eventsPanel.onAltUp += obj10;
					_eventsPanel.MouseDown += value4;
					_eventsPanel.GotFocus += value5;
					_eventsPanel.DoubleClick += value6;
					_eventsPanel.Click += value7;
					_eventsPanel.LostFocus += value8;
					_eventsPanel.onSpaceAlt += obj11;
					_eventsPanel.onShiftAlt += obj12;
					_eventsPanel.onCtrlAlt += obj13;
					_eventsPanel.onAltCapture += obj14;
					_eventsPanel.onCtrlC += obj15;
					_eventsPanel.onPrintScreen += obj16;
					_eventsPanel.onSendChar += obj17;
					_eventsPanel.onESC += obj18;
					_eventsPanel.onClipCopy += obj19;
					_eventsPanel.onShiftDown += EventsPanelOnShiftDown;
					_eventsPanel.onShiftUp += _eventsPanel_onShiftUp;
				}
			}
		}

		void _eventsPanel_onShiftUp()
		{
			if (ShiftToSelect && _shiftSelectActive)
			{
				//QuickEditMode = false;
				//_isOneClickQuickEdit = false;

				_shiftSelectActive = false;
				Debug.WriteLine("quick edit off");
			}
		}

		void EventsPanelOnShiftDown()
		{
			if (ShiftToSelect && !_shiftSelectActive)
			{
				OneQuickEdit();

				_shiftSelectActive = true;
				Debug.WriteLine("quick edit on");
			}
		}

		internal virtual ContextMenuStrip ContextMenuStrip1
		{
			get
			{
				return _contextMenu;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				CancelEventHandler value2 = ContextMenuStrip1_Opening;
				if (_contextMenu != null)
					_contextMenu.Opening -= value2;
				_contextMenu = value;
				if (_contextMenu != null)
					_contextMenu.Opening += value2;
			}
		}

		internal virtual ToolStripMenuItem SelectToolStripMenuItem
		{
			get
			{
				return _SelectToolStripMenuItem;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler value2 = SelectToolStripMenuItem_Click;
				if (_SelectToolStripMenuItem != null)
					_SelectToolStripMenuItem.Click -= value2;
				_SelectToolStripMenuItem = value;
				if (_SelectToolStripMenuItem != null)
					_SelectToolStripMenuItem.Click += value2;
			}
		}

		internal virtual ToolStripMenuItem PropertiesToolStripMenuItem
		{
			get
			{
				return _propertiesMenuItem;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler value2 = PropertiesToolStripMenuItem_Click;
				if (_propertiesMenuItem != null)
					_propertiesMenuItem.Click -= value2;
				_propertiesMenuItem = value;
				if (_propertiesMenuItem != null)
					_propertiesMenuItem.Click += value2;
			}
		}

		internal virtual ToolStripMenuItem PasteToolStripMenuItem
		{
			get
			{
				return _pasteMenuItem;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler value2 = PasteToolStripMenuItem_Click;
				if (_pasteMenuItem != null)
					_pasteMenuItem.Click -= value2;
				_pasteMenuItem = value;
				if (_pasteMenuItem != null)
					_pasteMenuItem.Click += value2;
			}
		}

		internal virtual ToolStripSeparator ToolStripSeparator2
		{
			get
			{
				return _ToolStripSeparator2;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				_ToolStripSeparator2 = value;
			}
		}

		internal virtual Timer Timer1
		{
			get
			{
				return _Timer1;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler value2 = Timer1_Tick;
				if (_Timer1 != null)
					_Timer1.Tick -= value2;
				_Timer1 = value;
				if (_Timer1 != null)
					_Timer1.Tick += value2;
			}
		}

		internal virtual ImageList ImageList1
		{
			get
			{
				return _ImageList1;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				_ImageList1 = value;
			}
		}

		internal virtual Panel p6
		{
			get
			{
				return _p6;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				_p6 = value;
			}
		}

		internal virtual ToolStripSeparator ToolStripSeparator3
		{
			get
			{
				return _ToolStripSeparator3;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				_ToolStripSeparator3 = value;
			}
		}

		internal virtual Panel Panel1
		{
			get
			{
				return _Panel1;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				_Panel1 = value;
			}
		}

		internal virtual HScrollBar HScrollBar1
		{
			get
			{
				return _horizontalScrollBar;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				_horizontalScrollBar = value;
			}
		}

		internal virtual VScrollBar VScrollBar1
		{
			get
			{
				return _verticalScrollBar;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				_verticalScrollBar = value;
			}
		}

		internal virtual ToolStripMenuItem AdjustWidthToolStripMenuItem
		{
			get
			{
				return _AdjustWidthToolStripMenuItem;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler value2 = AdjustWidthToolStripMenuItem_Click;
				if (_AdjustWidthToolStripMenuItem != null)
					_AdjustWidthToolStripMenuItem.Click -= value2;
				_AdjustWidthToolStripMenuItem = value;
				if (_AdjustWidthToolStripMenuItem != null)
					_AdjustWidthToolStripMenuItem.Click += value2;
			}
		}

		internal virtual ToolStripMenuItem MinimodeToolStripMenuItem1
		{
			get
			{
				return _MinimodeToolStripMenuItem1;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler value2 = MinimodeToolStripMenuItem1_Click;
				if (_MinimodeToolStripMenuItem1 != null)
					_MinimodeToolStripMenuItem1.Click -= value2;
				_MinimodeToolStripMenuItem1 = value;
				if (_MinimodeToolStripMenuItem1 != null)
					_MinimodeToolStripMenuItem1.Click += value2;
			}
		}

		internal virtual Label lblConsoleSize
		{
			get
			{
				return _lblConsoleSize;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				_lblConsoleSize = value;
			}
		}

		internal virtual Timer Timer2
		{
			get
			{
				return _Timer2;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler value2 = Timer2_Tick;
				if (_Timer2 != null)
					_Timer2.Tick -= value2;
				_Timer2 = value;
				if (_Timer2 != null)
					_Timer2.Tick += value2;
			}
		}

		public bool IsEmbedConsole
		{
			get
			{
				return _isEmbedConsole;
			}
			set
			{
				_isEmbedConsole = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string CurrentInput
		{
			get
			{
				return EventsPanel.CurrentInput;
			}
			set
			{
				EventsPanel.CurrentInput = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool Listen
		{
			get
			{
				return EventsPanel.Listen;
			}
			set
			{
				EventsPanel.Listen = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsNativeApp
		{
			get
			{
				return EventsPanel.isNativeApp;
			}
			set
			{
				EventsPanel.isNativeApp = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ListenTrigger
		{
			get
			{
				return EventsPanel.ListenTrigger;
			}
			set
			{
				EventsPanel.ListenTrigger = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Coordinates CursorPosition
		{
			get
			{
				Native.CONSOLE_SCREEN_BUFFER_INFO screenBufferInfo;
				Coordinates result;
				using (GetBufferInfo(out screenBufferInfo))
				{
					Coordinates coordinates = new Coordinates(screenBufferInfo.CursorPosition.X, screenBufferInfo.CursorPosition.Y);
					result = coordinates;
				}
				return result;
			}
			set
			{
				Native.CONSOLE_SCREEN_BUFFER_INFO screenBufferInfo;
				using (SafeFileHandle bufferInfo = GetBufferInfo(out screenBufferInfo))
				{
					Coordinates coordinates = value;
					Native.CONSOLE_SCREEN_BUFFER_INFO screenBufferInfo2 = screenBufferInfo;
					CheckCoordinateWithinBuffer(ref coordinates, ref screenBufferInfo2, "value");
					SetConsoleCursorPosition(bufferInfo, value);
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int CodePage
		{
			get
			{
				return checked((int)Native.GetConsoleOutputCP());
			}
			set
			{
				Native.SetConsoleOutputCP(value);
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int CursorSize
		{
			get
			{
				return checked((int)CursorInfo.Size);
			}
			set
			{
				if (value < 1)
					value = 1;
				if (value > 100)
					value = 100;
				Native.CONSOLE_CURSOR_INFO cursorInfo;
				cursorInfo.Size = checked((uint)value);
				cursorInfo.Visible = true;
				CursorInfo = cursorInfo;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Size BufferSize
		{
			get
			{
				Native.CONSOLE_SCREEN_BUFFER_INFO screenBufferInfo;
				Size result;
				using (GetBufferInfo(out screenBufferInfo))
				{
					Size size = new Size(screenBufferInfo.BufferSize.X, screenBufferInfo.BufferSize.Y);
					result = size;
				}
				return result;
			}
			set
			{
				try
				{
					using (SafeFileHandle obj = new SafeFileHandle(Native.CreateFile("CONOUT$", 3221225472u, 2u, IntPtr.Zero, 3u, 0u, IntPtr.Zero), true))
					{
						SetConsoleScreenBufferSize(obj, value);
					}
				}
				catch (Exception expr_43)
				{
					ProjectData.SetProjectError(expr_43);
					ProjectData.ClearProjectError();
				}
				finally
				{
				}
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Size WindowSize
		{
			get
			{
				Size result = new Size();
				if (hwnd != 0)
				{
					Native.CONSOLE_SCREEN_BUFFER_INFO screenBufferInfo;
					using (GetBufferInfo(out screenBufferInfo))
					{
						Size size =
							checked(
								new Size(
									(unchecked(screenBufferInfo.WindowRect.Right - screenBufferInfo.WindowRect.Left) + 1),
									(unchecked(screenBufferInfo.WindowRect.Bottom - screenBufferInfo.WindowRect.Top) + 1)));
						result = size;
					}
				}
				return result;
			}
			set
			{
				if (hwnd == 0)
					return;
				Native.CONSOLE_SCREEN_BUFFER_INFO screenBufferInfo;
				using (SafeFileHandle bufferInfo = GetBufferInfo(out screenBufferInfo))
				{
					if (value.Width < 10)
						value.Width = 10;
					if (value.Height < 3)
						value.Height = 3;
					if (value.Width > screenBufferInfo.BufferSize.X)
						value.Width = screenBufferInfo.BufferSize.X;
					if (value.Height > screenBufferInfo.BufferSize.Y)
						value.Height = screenBufferInfo.BufferSize.Y;
					Native.SMALL_RECT windowRect = screenBufferInfo.WindowRect;
					short num;
					short num2;
					checked
					{
						windowRect.Right = (short)(windowRect.Left + value.Width - 1);
						windowRect.Bottom = (short)(windowRect.Top + value.Height - 1);
						num = (short)(windowRect.Right - (screenBufferInfo.BufferSize.X - 1));
						num2 = (short)(windowRect.Bottom - (screenBufferInfo.BufferSize.Y - 1));
					}
					if (num > 0)
					{
						windowRect.Left -= num;
						windowRect.Right -= num;
					}
					if (num2 > 0)
					{
						windowRect.Top -= num2;
						windowRect.Bottom -= num2;
					}
					if (windowRect.Right >= windowRect.Left)
					{
						if (windowRect.Bottom >= windowRect.Top)
							SetConsoleWindowInfo(bufferInfo, true, windowRect);
					}
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Size FontSize
		{
			get
			{
				int num = (int)Native.GetConsoleWindow();
				IntPtr stdHandle = Native.GetStdHandle(4294967285u);
				Native.CONSOLE_FONT_INFO consoleFontInfo;
				Native.GetCurrentConsoleFont(stdHandle, false, out consoleFontInfo);
				Native.Coord consoleFontSize = Native.GetConsoleFontSize(stdHandle, consoleFontInfo.nFont);
				Size result = new Size(consoleFontSize.X, consoleFontSize.Y);
				return result;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int ConsoleHandle
		{
			get
			{
				return hwnd;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool CanAutoAdjust
		{
			get;
			set;
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Size MinSize
		{
			get;
			set;
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		private Native.CONSOLE_CURSOR_INFO CursorInfo
		{
			get
			{
				IntPtr consoleOutput = Native.GetStdHandle(4294967285u);
				Native.CONSOLE_CURSOR_INFO result;
				if (Native.GetConsoleCursorInfo(consoleOutput, out result))
					return result;

				return new Native.CONSOLE_CURSOR_INFO();
			}
			set
			{
				IntPtr consoleOutput = Native.GetStdHandle(4294967285u);
				Native.SetConsoleCursorInfo(consoleOutput, ref value);
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		private uint Mode
		{
			get
			{
				IntPtr consoleHandle = Native.GetStdHandle(4294967286u);
				uint result;
				if (Native.GetConsoleMode(consoleHandle, out result))
					return result;
				return 0;
			}
			set
			{
				IntPtr consoleHandle = Native.GetStdHandle(4294967286u);
				Native.SetConsoleMode(consoleHandle, value);
			}
		}

		public bool ShiftToSelect
		{

			get
			{
				return _shiftToSelect;
			}

			set
			{
				_shiftToSelect = value;

				//if (value)
				//{
				//	MouseInputEnabled = false;
				//	QuickEditMode = true;
				//}
			}
		}

		public bool MouseInputEnabled
		{
			get
			{
				return (ConsoleMode & Native.ConsoleModes.MouseInput) == Native.ConsoleModes.MouseInput;
			}

			set
			{
				if (value)
					ConsoleMode = ConsoleMode | Native.ConsoleModes.MouseInput;

				else
					ConsoleMode &= ~Native.ConsoleModes.MouseInput;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool QuickEditMode
		{
			get
			{
				return (Mode & 64uL) == 64uL;
			}
			set
			{
				checked
				{
					if (!value)
					{
						Mode = (uint)(unchecked(Mode) & 18446744073709551551uL);
						_isOneClickQuickEdit = false;
						QuickEditModeChangedEventHandler quickEditModeChangedEvent = QuickEditModeChangedEvent;
						if (quickEditModeChangedEvent != null)
							quickEditModeChangedEvent(false);
					}
					else
					{
						Mode = (uint)(unchecked(Mode) | 64uL);
						EventsPanel._newfocus = false;
						QuickEditModeChangedEventHandler quickEditModeChangedEvent = QuickEditModeChangedEvent;
						if (quickEditModeChangedEvent != null)
							quickEditModeChangedEvent(true);
					}
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool PgUpDownScrollsConsole
		{
			get
			{
				return EventsPanel.PGUPDOWNScrollsConsole;
			}
			set
			{
				EventsPanel.PGUPDOWNScrollsConsole = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool PanelVisible
		{
			get
			{
				return EventsPanel.Visible;
			}
			set
			{
				EventsPanel.Visible = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool HasFocus
		{
			get;
			private set;
		}

		public event OnMinimodeEndEventHandler OnMinimodeEnd
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				onMinimodeEndEvent = (OnMinimodeEndEventHandler)Delegate.Combine(onMinimodeEndEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				onMinimodeEndEvent = (OnMinimodeEndEventHandler)Delegate.Remove(onMinimodeEndEvent, value);
			}
		}

		public event OnToggleWindowModeEventHandler OnToggleWindowMode
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				_onToggleWindowModeEvent = (OnToggleWindowModeEventHandler)Delegate.Combine(_onToggleWindowModeEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				_onToggleWindowModeEvent = (OnToggleWindowModeEventHandler)Delegate.Remove(_onToggleWindowModeEvent, value);
			}
		}

		public event OnMinimizeEventHandler OnMinimize
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				onMinimizeEvent = (OnMinimizeEventHandler)Delegate.Combine(onMinimizeEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				onMinimizeEvent = (OnMinimizeEventHandler)Delegate.Remove(onMinimizeEvent, value);
			}
		}

		public event OnLicenseAddedEventHandler OnLicenseAdded
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				_onLicenseAddedEvent = (OnLicenseAddedEventHandler)Delegate.Combine(_onLicenseAddedEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				_onLicenseAddedEvent = (OnLicenseAddedEventHandler)Delegate.Remove(_onLicenseAddedEvent, value);
			}
		}

		public event OnMiniModeAltEventHandler OnMiniModeAlt
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				onMiniModeAltEvent = (OnMiniModeAltEventHandler)Delegate.Combine(onMiniModeAltEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				onMiniModeAltEvent = (OnMiniModeAltEventHandler)Delegate.Remove(onMiniModeAltEvent, value);
			}
		}

		public event OnCtrlAltEventHandler OnCtrlAlt
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				_onCtrlAltEvent = (OnCtrlAltEventHandler)Delegate.Combine(_onCtrlAltEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				_onCtrlAltEvent = (OnCtrlAltEventHandler)Delegate.Remove(_onCtrlAltEvent, value);
			}
		}

		public event OnShiftAltEventHandler OnShiftAlt
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				onShiftAltEvent = (OnShiftAltEventHandler)Delegate.Combine(onShiftAltEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				onShiftAltEvent = (OnShiftAltEventHandler)Delegate.Remove(onShiftAltEvent, value);
			}
		}

		public event OnSpaceAltEventHandler OnSpaceAlt
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				_onSpaceAltEvent = (OnSpaceAltEventHandler)Delegate.Combine(_onSpaceAltEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				_onSpaceAltEvent = (OnSpaceAltEventHandler)Delegate.Remove(_onSpaceAltEvent, value);
			}
		}

		public event OnAltEventHandler OnAlt
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				_onAltEvent = (OnAltEventHandler)Delegate.Combine(_onAltEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				_onAltEvent = (OnAltEventHandler)Delegate.Remove(_onAltEvent, value);
			}
		}

		public event OnAltCaptureEventHandler OnAltCapture
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				_onAltCaptureEvent = (OnAltCaptureEventHandler)Delegate.Combine(_onAltCaptureEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				_onAltCaptureEvent = (OnAltCaptureEventHandler)Delegate.Remove(_onAltCaptureEvent, value);
			}
		}

		public event OnCtrlCEventHandler OnCtrlC
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				_onCtrlCEvent = (OnCtrlCEventHandler)Delegate.Combine(_onCtrlCEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				_onCtrlCEvent = (OnCtrlCEventHandler)Delegate.Remove(_onCtrlCEvent, value);
			}
		}

		public event OnPrintScreenEventHandler OnPrintScreen
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				onPrintScreenEvent = (OnPrintScreenEventHandler)Delegate.Combine(onPrintScreenEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				onPrintScreenEvent = (OnPrintScreenEventHandler)Delegate.Remove(onPrintScreenEvent, value);
			}
		}

		public event OnKeepFocusEventHandler OnKeepFocus
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				_onKeepFocusEvent = (OnKeepFocusEventHandler)Delegate.Combine(_onKeepFocusEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				_onKeepFocusEvent = (OnKeepFocusEventHandler)Delegate.Remove(_onKeepFocusEvent, value);
			}
		}

		public event OnEscEventHandler OnEsc
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				_onEscEvent = (OnEscEventHandler)Delegate.Combine(_onEscEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				_onEscEvent = (OnEscEventHandler)Delegate.Remove(_onEscEvent, value);
			}
		}

		public event OnClipCopyEventHandler OnClipCopy
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				_onClipCopyEvent = (OnClipCopyEventHandler)Delegate.Combine(_onClipCopyEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				_onClipCopyEvent = (OnClipCopyEventHandler)Delegate.Remove(_onClipCopyEvent, value);
			}
		}

		public event OnTriggerEventHandler OnTrigger
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				_onTriggerEvent = (OnTriggerEventHandler)Delegate.Combine(_onTriggerEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				_onTriggerEvent = (OnTriggerEventHandler)Delegate.Remove(_onTriggerEvent, value);
			}
		}

		public event OnDoubleClickEventHandler onDoubleClick
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				_onDoubleClickEvent = (OnDoubleClickEventHandler)Delegate.Combine(_onDoubleClickEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				_onDoubleClickEvent = (OnDoubleClickEventHandler)Delegate.Remove(_onDoubleClickEvent, value);
			}
		}

		public event QuickEditModeChangedEventHandler QuickEditModeChanged
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				QuickEditModeChangedEvent = (QuickEditModeChangedEventHandler)Delegate.Combine(QuickEditModeChangedEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				QuickEditModeChangedEvent = (QuickEditModeChangedEventHandler)Delegate.Remove(QuickEditModeChangedEvent, value);
			}
		}

		public event OnPropertyPageShowEventHandler OnPropertyPageShow
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				onPropertyPageShowEvent = (OnPropertyPageShowEventHandler)Delegate.Combine(onPropertyPageShowEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				onPropertyPageShowEvent = (OnPropertyPageShowEventHandler)Delegate.Remove(onPropertyPageShowEvent, value);
			}
		}

		public event OnPropertyPageExitEventHandler OnPropertyPageExit
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				onPropertyPageExitEvent = (OnPropertyPageExitEventHandler)Delegate.Combine(onPropertyPageExitEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				onPropertyPageExitEvent = (OnPropertyPageExitEventHandler)Delegate.Remove(onPropertyPageExitEvent, value);
			}
		}

		public event OnFunctionKeyEventHandler OnFunctionKey
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				_onFunctionKeyEvent = (OnFunctionKeyEventHandler)Delegate.Combine(_onFunctionKeyEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				_onFunctionKeyEvent = (OnFunctionKeyEventHandler)Delegate.Remove(_onFunctionKeyEvent, value);
			}
		}

		public event OnCursorKeyEventHandler OnCursorKey
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				_onCursorKeyEvent = (OnCursorKeyEventHandler)Delegate.Combine(_onCursorKeyEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				_onCursorKeyEvent = (OnCursorKeyEventHandler)Delegate.Remove(_onCursorKeyEvent, value);
			}
		}

		public event OnExitEventHandler OnExit
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				_onExitEvent = (OnExitEventHandler)Delegate.Combine(_onExitEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				_onExitEvent = (OnExitEventHandler)Delegate.Remove(_onExitEvent, value);
			}
		}

		public event OnMenuRequestEventHandler OnMenuRequest
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				onMenuRequestEvent = (OnMenuRequestEventHandler)Delegate.Combine(onMenuRequestEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				onMenuRequestEvent = (OnMenuRequestEventHandler)Delegate.Remove(onMenuRequestEvent, value);
			}
		}

		public event OnEnterEventHandler onEnter
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				_onEnterEvent = (OnEnterEventHandler)Delegate.Combine(_onEnterEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				_onEnterEvent = (OnEnterEventHandler)Delegate.Remove(_onEnterEvent, value);
			}
		}

		public event OnScrollBarEventHandler OnScrollBar
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				onScrollBarEvent = (OnScrollBarEventHandler)Delegate.Combine(onScrollBarEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				onScrollBarEvent = (OnScrollBarEventHandler)Delegate.Remove(onScrollBarEvent, value);
			}
		}

		public event OnAltEnterEventHandler OnAltEnter
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				onALTENTEREvent = (OnAltEnterEventHandler)Delegate.Combine(onALTENTEREvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				onALTENTEREvent = (OnAltEnterEventHandler)Delegate.Remove(onALTENTEREvent, value);
			}
		}

		public event OnOneQuickEditDoneEventHandler OnOneQuickEditDone
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				onOneQuickEditDoneEvent = (OnOneQuickEditDoneEventHandler)Delegate.Combine(onOneQuickEditDoneEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				onOneQuickEditDoneEvent = (OnOneQuickEditDoneEventHandler)Delegate.Remove(onOneQuickEditDoneEvent, value);
			}
		}

		public event OnClickEventHandler onClick
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				_onClickEvent = (OnClickEventHandler)Delegate.Combine(_onClickEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				_onClickEvent = (OnClickEventHandler)Delegate.Remove(_onClickEvent, value);
			}
		}

		public event OnTitleChangeEventHandler OnTitleChange
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				_onTitleChangeEvent = (OnTitleChangeEventHandler)Delegate.Combine(_onTitleChangeEvent, value);
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				_onTitleChangeEvent = (OnTitleChangeEventHandler)Delegate.Remove(_onTitleChangeEvent, value);
			}
		}

		[DebuggerNonUserCode]
		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && components != null)
					components.Dispose();
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		private void InitializeComponent()
		{
			components = new Container();
			ContainerPanel = new Panel();
			EventsPanel = new SpecComponent();
			ConsolePanel = new Panel();
			p6 = new Panel();
			Panel1 = new Panel();
			HScrollBar1 = new HScrollBar();
			VScrollBar1 = new VScrollBar();
			PropertyWatcher = new Timer(components);
			ContextMenuStrip1 = new ContextMenuStrip(components);
			SelectToolStripMenuItem = new ToolStripMenuItem();
			PasteToolStripMenuItem = new ToolStripMenuItem();
			ToolStripSeparator2 = new ToolStripSeparator();
			PropertiesToolStripMenuItem = new ToolStripMenuItem();
			ToolStripSeparator3 = new ToolStripSeparator();
			AdjustWidthToolStripMenuItem = new ToolStripMenuItem();
			MinimodeToolStripMenuItem1 = new ToolStripMenuItem();
			Timer1 = new Timer(components);
			ImageList1 = new ImageList(components);
			lblConsoleSize = new Label();
			Timer2 = new Timer(components);
			ContainerPanel.SuspendLayout();
			Panel1.SuspendLayout();
			ContextMenuStrip1.SuspendLayout();
			SuspendLayout();
			ContainerPanel.BackColor = SystemColors.Control;
			ContainerPanel.Controls.Add(EventsPanel);
			ContainerPanel.Controls.Add(ConsolePanel);
			ContainerPanel.Controls.Add(p6);
			ContainerPanel.Controls.Add(Panel1);
			ContainerPanel.Dock = DockStyle.Fill;
			Control arg_202_0 = ContainerPanel;
			Point location = new Point(0, 0);
			arg_202_0.Location = location;
			ContainerPanel.Name = "p2";
			Control arg_230_0 = ContainerPanel;
			Size size = new Size(787, 435);
			arg_230_0.Size = size;
			ContainerPanel.TabIndex = 1;
			EventsPanel.AllowDrop = true;
			EventsPanel.CurrentInput = "";
			EventsPanel.Dock = DockStyle.Fill;
			EventsPanel.isNativeApp = false;
			EventsPanel.Listen = true;
			EventsPanel.ListenTrigger = true;
			Control arg_29D_0 = EventsPanel;
			location = new Point(0, 0);
			arg_29D_0.Location = location;
			EventsPanel.Name = "p1";
			EventsPanel.NewFocus = false;
			EventsPanel.PGUPDOWNScrollsConsole = true;
			EventsPanel.scrollH = true;
			EventsPanel.scrollV = true;
			Control arg_2FB_0 = EventsPanel;
			size = new Size(787, 435);
			arg_2FB_0.Size = size;
			EventsPanel.TabIndex = 25;
			ConsolePanel.AllowDrop = true;
			Control arg_329_0 = ConsolePanel;
			location = new Point(0, 0);
			arg_329_0.Location = location;
			ConsolePanel.Name = "p3";
			Control arg_357_0 = ConsolePanel;
			size = new Size(491, 228);
			arg_357_0.Size = size;
			ConsolePanel.TabIndex = 20;
			Control arg_379_0 = p6;
			location = new Point(8, 8);
			arg_379_0.Location = location;
			p6.Name = "p6";
			Control arg_3A4_0 = p6;
			size = new Size(200, 100);
			arg_3A4_0.Size = size;
			p6.TabIndex = 26;
			Panel1.Controls.Add(HScrollBar1);
			Panel1.Controls.Add(VScrollBar1);
			Panel1.Dock = DockStyle.Fill;
			Control arg_3FE_0 = Panel1;
			location = new Point(0, 0);
			arg_3FE_0.Location = location;
			Panel1.Name = "Panel1";
			Control arg_42C_0 = Panel1;
			size = new Size(787, 435);
			arg_42C_0.Size = size;
			Panel1.TabIndex = 28;
			Control arg_452_0 = HScrollBar1;
			location = new Point(0, 418);
			arg_452_0.Location = location;
			HScrollBar1.Name = "HScrollBar1";
			Control arg_47D_0 = HScrollBar1;
			size = new Size(770, 17);
			arg_47D_0.Size = size;
			HScrollBar1.TabIndex = 1;
			Control arg_4A2_0 = VScrollBar1;
			location = new Point(770, 0);
			arg_4A2_0.Location = location;
			VScrollBar1.Name = "VScrollBar1";
			Control arg_4CD_0 = VScrollBar1;
			size = new Size(17, 435);
			arg_4CD_0.Size = size;
			VScrollBar1.TabIndex = 0;
			ContextMenuStrip1.Items.AddRange(
				new ToolStripItem[]
					{
						SelectToolStripMenuItem,
						PasteToolStripMenuItem,
						ToolStripSeparator2,
						AdjustWidthToolStripMenuItem,
						MinimodeToolStripMenuItem1,
						ToolStripSeparator3,
						PropertiesToolStripMenuItem
					});
			ContextMenuStrip1.Name = "ContextMenuStrip1";
			Control arg_8AC_0 = ContextMenuStrip1;
			size = new Size(153, 148);
			arg_8AC_0.Size = size;
			SelectToolStripMenuItem.Name = "SelectToolStripMenuItem";
			ToolStripItem arg_8F2_0 = SelectToolStripMenuItem;
			size = new Size(152, 22);
			arg_8F2_0.Size = size;
			SelectToolStripMenuItem.Text = "Select";
			PasteToolStripMenuItem.Name = "PasteToolStripMenuItem";
			ToolStripItem arg_948_0 = PasteToolStripMenuItem;
			size = new Size(152, 22);
			arg_948_0.Size = size;
			PasteToolStripMenuItem.Text = "Paste";
			ToolStripSeparator2.Name = "ToolStripSeparator2";
			ToolStripItem arg_982_0 = ToolStripSeparator2;
			size = new Size(149, 6);
			arg_982_0.Size = size;
			PropertiesToolStripMenuItem.Name = "PropertiesToolStripMenuItem";
			ToolStripItem arg_9C8_0 = PropertiesToolStripMenuItem;
			size = new Size(152, 22);
			arg_9C8_0.Size = size;
			PropertiesToolStripMenuItem.Text = "Properties";
			ToolStripSeparator3.Name = "ToolStripSeparator3";
			ToolStripItem arg_A02_0 = ToolStripSeparator3;
			size = new Size(149, 6);
			arg_A02_0.Size = size;
			AdjustWidthToolStripMenuItem.Name = "AdjustWidthToolStripMenuItem";
			ToolStripItem arg_A3D_0 = AdjustWidthToolStripMenuItem;
			size = new Size(152, 22);
			arg_A3D_0.Size = size;
			AdjustWidthToolStripMenuItem.Text = "Buffer Resize";
			MinimodeToolStripMenuItem1.Name = "MinimodeToolStripMenuItem1";
			ToolStripItem arg_A88_0 = MinimodeToolStripMenuItem1;
			size = new Size(152, 22);
			arg_A88_0.Size = size;
			MinimodeToolStripMenuItem1.Text = "Minimode";
			Timer1.Interval = 10;
			ImageList1.TransparentColor = Color.Transparent;
			lblConsoleSize.AutoSize = true;
			lblConsoleSize.BackColor = Color.Transparent;
			lblConsoleSize.Font = new Font("Microsoft Sans Serif", 26.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblConsoleSize.ForeColor = Color.Green;
			Control arg_B88_0 = lblConsoleSize;
			location = new Point(50, 50);
			arg_B88_0.Location = location;
			lblConsoleSize.Name = "lblConsoleSize";
			Control arg_BB0_0 = lblConsoleSize;
			size = new Size(116, 39);
			arg_BB0_0.Size = size;
			lblConsoleSize.TabIndex = 0;
			lblConsoleSize.Text = "20x80";
			lblConsoleSize.Visible = false;
			SizeF autoScaleDimensions = new SizeF(6f, 13f);
			AutoScaleDimensions = autoScaleDimensions;
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(lblConsoleSize);
			Controls.Add(ContainerPanel);
			Name = "ConsoleControl";
			size = new Size(787, 435);
			Size = size;
			ContainerPanel.ResumeLayout(false);
			Panel1.ResumeLayout(false);
			ContextMenuStrip1.ResumeLayout(false);
			ResumeLayout(false);
			PerformLayout();
		}

		public void OneQuickEdit()
		{
			if (!QuickEditMode)
				QuickEditMode = true;
			_isOneClickQuickEdit = true;
		}

		[DllImport("kernel32.dll")]
		internal static extern bool GetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool bMaximumWindow, out Native.CONSOLE_FONT_INFO_EX lpConsoleCurrentFont);

		[DllImport("kernel32")]
		private static extern int GetLastError();

		[DllImport("user32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		private static extern int SetWinEventHook(
			int eventMin, int eventMax, int hmodWinEventProc, EventFunc pfnWinEventProc, int idProcess, int idThread, int dwFlags);

		[DllImport("user32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		private static extern int UnhookWinEvent(int lHandle);

		[DllImport("user32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		private static extern int LockWindowUpdate(int hWnd);

		[DllImport("user32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		private static extern int GetDesktopWindow();

		internal void StartHook(IntPtr hwnd)
		{
		}

		internal void EndHook()
		{
		}

		internal int WinEventFunc(int HookHandle, int LEvent, int hwnd, int idObject, int idChild, int idEventThread, int dwmsEventTime)
		{
			if (hwnd == this.hwnd && LEvent != 16388 && LEvent == 16386)
			{
				xyCOORD start = new xyCOORD();
				start.all = idObject;
				xyCOORD ende = new xyCOORD();
				ende.all = idChild;
				string text = ReadConsole(start, ende);
			}
			return 0;
		}

		private string ReadConsole(xyCOORD start, xyCOORD ende)
		{
			string result = "";
			IntPtr stdHandle = Native.GetStdHandle(4294967285u);
			Native.CONSOLE_SCREEN_BUFFER_INFO cONSOLE_SCREEN_BUFFER_INFO = new Native.CONSOLE_SCREEN_BUFFER_INFO();
			int y = cONSOLE_SCREEN_BUFFER_INFO.BufferSize.Y;
			int x = cONSOLE_SCREEN_BUFFER_INFO.BufferSize.X;
			checked
			{
				int num = unchecked(ende.x - start.x) + unchecked(ende.y - start.y) * x;
				Native.CHAR_INFO[] array = new Native.CHAR_INFO[num + 1];
				Native.Coord bufferSize = new Native.Coord();
				Native.Coord bufferCoord = new Native.Coord();
				Native.SMALL_RECT sMALL_RECT = new Native.SMALL_RECT();
				if (Native.GetConsoleScreenBufferInfo(stdHandle, out cONSOLE_SCREEN_BUFFER_INFO))
				{
					bufferSize.X = (short)unchecked(ende.x - start.x);
					bufferSize.Y = (short)(unchecked(ende.y - start.y) + 1);
					bufferCoord.X = 0;
					bufferCoord.Y = 0;
					sMALL_RECT.Top = start.y;
					sMALL_RECT.Left = start.x;
					sMALL_RECT.Right = ende.x;
					sMALL_RECT.Bottom = ende.y;
				}
				if (Native.ReadConsoleOutput(stdHandle, array, bufferSize, bufferCoord, ref sMALL_RECT))
				{
					object obj2 = 1;
					StringBuilder stringBuilder = new StringBuilder();
					int num3 = 0;
					int arg_123_0 = array.GetLowerBound(0);
					int upperBound = array.GetUpperBound(0);
					for (int i = arg_123_0; i <= upperBound; i++)
					{
						stringBuilder.Append(Strings.ChrW(array[i].UnicodeChar));
						num3++;
						if (num3 > bufferSize.X)
						{
							num3 = 0;
							stringBuilder.Append("\r\n");
						}
					}
					result = stringBuilder.ToString();
				}
				return result;
			}
		}

		private void PreStart()
		{
			_KeyToRegProfile = Application.ExecutablePath.Replace("\\", "_");
			try
			{
				RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Console", false);
				if (registryKey == null)
					Registry.CurrentUser.CreateSubKey("Console").Close();
				else
					registryKey.Close();
				_regKeyProfile = Registry.CurrentUser.OpenSubKey("Console\\" + _KeyToRegProfile, true);
				if (_regKeyProfile == null)
				{
					_isTemporaryProfile = true;
					_regKeyProfile = Registry.CurrentUser.OpenSubKey("Console", true);
					RegistryKey registryKey2 = _regKeyProfile.CreateSubKey(_KeyToRegProfile);
					registryKey2.SetValue("ColorTable05", 5645313);
					registryKey2.SetValue("ColorTable06", 15789550);
					registryKey2.SetValue("ScreenColors", 86);
					registryKey2.SetValue("PopupColors", 243);
					registryKey2.SetValue("InsertMode", 1);
					registryKey2.SetValue("WindowPosition", 1073758208);
					registryKey2.SetValue("InsertMode", 1);
					registryKey2.SetValue("ScreenBufferSize", 19661100);
					registryKey2.Close();
				}
				else
				{
					_isTemporaryProfile = false;
					try
					{
						_oldWindowPosition = -1;
						_oldWindowPosition = Conversions.ToInteger(_regKeyProfile.GetValue("WindowPosition"));
					}
					catch (Exception arg_178_0)
					{
						ProjectData.SetProjectError(arg_178_0);
						ProjectData.ClearProjectError();
					}
					finally
					{
						_regKeyProfile.SetValue("WindowPosition", 1073758208);
						_regKeyProfile.SetValue("InsertMode", 1);
					}
				}
			}
			catch (Exception arg_1B7_0)
			{
				ProjectData.SetProjectError(arg_1B7_0);
				Console.WriteLine("ERROR in prestart");
				ProjectData.ClearProjectError();
			}
		}

		private void PostStart()
		{
			try
			{
				if (_isTemporaryProfile)
					_regKeyProfile.DeleteSubKey(_KeyToRegProfile);
				else
				{
					if (_oldWindowPosition == -1)
						_regKeyProfile.DeleteValue("WindowPosition");
					else
						_regKeyProfile.SetValue("WindowPosition", _oldWindowPosition);
				}
				_regKeyProfile.Close();
			}
			catch (Exception arg_5E_0)
			{
				ProjectData.SetProjectError(arg_5E_0);
				Console.WriteLine("Error in PostStart");
				ProjectData.ClearProjectError();
			}
		}

		internal static void FlushConsoleInputBuffer(SafeFileHandle consoleHandle)
		{
			Native.FlushConsoleInputBuffer(consoleHandle.DangerousGetHandle());
		}

		private void SendToHistory(string text)
		{
			using (SafeFileHandle inputHandle = GetInputHandle())
			{
				Console.SetCursorPosition(0, 0);
				FlushConsoleInputBuffer(inputHandle);
				Native.SendText(text + "\r");
				string text2 = Console.ReadLine();
				Console.SetCursorPosition(0, 0);
				Console.Write(Strings.Space(text.Length));
				Console.SetCursorPosition(0, 0);
			}
		}

		private void PSControl_Load(object sender, EventArgs e)
		{
			s = FontSize;
			int height = s.Height;
			int width = s.Width;
			hwnd = (int)Native.GetConsoleWindow();
			Native.SetParent((IntPtr)hwnd, ConsolePanel.Handle);
			Native.ShowWindow(hwnd, 5);
			checked
			{
				Native.SetWindowPos((IntPtr)hwnd, (IntPtr)0, 0 - _offsetX + _offsetFrame, 0 - _offsetY + _offsetFrame, 0, 0, 1);
				UpdateSize(false);
				try
				{
					if (Parent != null)
					{
						Parent.Width = Parent.Width - (ContainerPanel.Width - ConsolePanel.Width);
						Parent.Height = Parent.Height - (ContainerPanel.Height - ConsolePanel.Height);
					}
				}
				catch (Exception expr_10D)
				{
					ProjectData.SetProjectError(expr_10D);
					Exception ex = expr_10D;

					ProjectData.ClearProjectError();
				}
				EventsPanel.hwnd = (IntPtr)hwnd;
				try
				{
					StartHook((IntPtr)hwnd);
				}
				catch (Exception expr_19C)
				{
					ProjectData.SetProjectError(expr_19C);
					Exception ex2 = expr_19C;

					ProjectData.ClearProjectError();
				}
			}
		}

		public void SuspendUpdate()
		{
			_layoutsuspended = true;
		}

		public void ResumeUpdate()
		{
			_layoutsuspended = false;
		}

		public void CloseConsole()
		{
			if (hwnd != 0)
				Native.PostMessage((IntPtr)hwnd, 16, (IntPtr)0L, (IntPtr)0L);
		}

		public void HideCursor()
		{
			Native.SendMessage((IntPtr)hwnd, 8u, (IntPtr)(-1L), (IntPtr)0);
			EventsPanel.NewFocus = true;
			HasFocus = false;
		}

		public void ShowCursor()
		{
			Native.SendMessage((IntPtr)hwnd, 7u, (IntPtr)(-1L), (IntPtr)0);
			EventsPanel.NewFocus = false;
			HasFocus = true;
		}

		private bool isVista()
		{
			return Environment.OSVersion.Version.Major >= 6;
		}

		public void SetTextColor(uint color)
		{
			if (!isVista())
			{
				throw new InvalidOperationException(
					"Setting console text color requires Windows Vista or better. You can set colors using the console properties window: Settings/Properties");
			}
			int num = 0;
			int num2 = 0;
			GetConsoleColorIndex(ref num, ref num2);
			checked
			{
				uint hConsole = (uint)((int)Native.GetStdHandle(4294967285u));
				Native.CONSOLE_SCREEN_BUFFER_INFOEX screenBufferInfo = default(Native.CONSOLE_SCREEN_BUFFER_INFOEX);
				screenBufferInfo.cbSize = 96u;
				Native.GetConsoleScreenBufferInfoEx(hConsole, ref screenBufferInfo);
				screenBufferInfo.ColorTable[num] = color;
				screenBufferInfo.srWindow.Right = (short)(screenBufferInfo.srWindow.Right + 1);
				screenBufferInfo.srWindow.Bottom = (short)(screenBufferInfo.srWindow.Bottom + 1);
				Native.SetConsoleScreenBufferInfoEx(hConsole, ref screenBufferInfo);
			}
		}

		public void SetBackColor(uint color)
		{
			if (!isVista())
			{
				throw new InvalidOperationException(
					"Setting console background color requires Windows Vista or better. You can set colors using the console properties window: Settings/Properties");
			}
			int num = 0;
			int num2 = 0;
			GetConsoleColorIndex(ref num, ref num2);
			ContainerPanel.BackColor = ColorTranslator.FromWin32((int)color);
			checked
			{
				uint hConsole = (uint)((int)Native.GetStdHandle(4294967285u));
				Native.CONSOLE_SCREEN_BUFFER_INFOEX screenBufferInfo = default(Native.CONSOLE_SCREEN_BUFFER_INFOEX);
				screenBufferInfo.cbSize = 96u;
				Native.GetConsoleScreenBufferInfoEx(hConsole, ref screenBufferInfo);
				screenBufferInfo.ColorTable[num2] = color;
				screenBufferInfo.srWindow.Right = (short)(screenBufferInfo.srWindow.Right + 1);
				screenBufferInfo.srWindow.Bottom = (short)(screenBufferInfo.srWindow.Bottom + 1);
				Native.SetConsoleScreenBufferInfoEx(hConsole, ref screenBufferInfo);
			}
		}

		public uint GetBackColor()
		{
			if (!isVista())
			{
				throw new InvalidOperationException(
					"Setting console background color requires Windows Vista or better. You can set colors using the console properties window: Settings/Properties");
			}
			int num = 0;
			int num2 = 0;
			GetConsoleColorIndex(ref num, ref num2);
			uint hConsole = checked((uint)((int)Native.GetStdHandle(4294967285u)));
			Native.CONSOLE_SCREEN_BUFFER_INFOEX screenBufferInfo = default(Native.CONSOLE_SCREEN_BUFFER_INFOEX);
			screenBufferInfo.cbSize = 96u;
			Native.GetConsoleScreenBufferInfoEx(hConsole, ref screenBufferInfo);
			return screenBufferInfo.ColorTable[num2];
		}

		public void GetConsoleColorIndex(ref int foreground, ref int background)
		{
			Native.CONSOLE_SCREEN_BUFFER_INFO screenBufferInfo;
			using (GetBufferInfo(out screenBufferInfo))
			{
				ushort attributes = screenBufferInfo.Attributes;
				ConsoleColor consoleColor;
				ConsoleColor consoleColor2;
				WordToColor(attributes, out consoleColor, out consoleColor2);
				background = (int)consoleColor2;
				foreground = (int)consoleColor;
			}
		}

		internal static void WordToColor(ushort attribute, out ConsoleColor foreground, out ConsoleColor background)
		{
			foreground = (ConsoleColor)(attribute & 15);
			background = (ConsoleColor)((attribute & 240) >> 4);
		}

		public void SetFontFamily(string name)
		{
			if (!isVista())
			{
				throw new InvalidOperationException(
					"Setting console font requires Windows Vista or better. You can set colors using the console properties window: Settings/Properties");
			}
			uint num = checked((uint)((int)Native.GetStdHandle(4294967285u)));
			Native.CONSOLE_FONT_INFOEX consoleFontInfo = default(Native.CONSOLE_FONT_INFOEX);
			consoleFontInfo.cbSize = 84u;
			consoleFontInfo.FaceName = name;
			Native.GetConsoleFontSize((IntPtr)((long)(num)), 4);
			Native.SetCurrentConsoleFontEx(num, 0u, ref consoleFontInfo);
		}

		public void SetFontSize(int width)
		{
			if (!isVista())
			{
				throw new InvalidOperationException(
					"Setting console font size requires Windows Vista or better. You can set colors using the console properties window: Settings/Properties");
			}
			uint num;
			Native.CONSOLE_FONT_INFOEX consoleFontInfo;
			checked
			{
				num = (uint)((int)Native.GetStdHandle(4294967285u));
				consoleFontInfo = default(Native.CONSOLE_FONT_INFOEX);
				consoleFontInfo.cbSize = 84u;
				consoleFontInfo.dwFontSize.X = (short)width;
				consoleFontInfo.dwFontSize.Y = (short)width;
				consoleFontInfo.FaceName = GetFontFamily();
				consoleFontInfo.FontWeight = 400u;
			}
			Native.GetConsoleFontSize((IntPtr)((long)(num)), 4);
			Native.SetCurrentConsoleFontEx(num, 0u, ref consoleFontInfo);
		}

		private void FindMatch(ref short x, ref short y)
		{
			if (x >= 16)
			{
				if (y >= 12)
				{
					x = 16;
					y = 12;
				}
				else
				{
					x = 16;
					y = 8;
				}
			}
			else
			{
				if (x >= 12 & y < 18)
				{
					x = 12;
					y = 16;
				}
				else
				{
					if (x >= 10)
					{
						x = 10;
						y = 18;
					}
					else
					{
						if (x >= 8 & y >= 12)
						{
							x = 8;
							y = 12;
						}
						else
						{
							if (x >= 8)
							{
								x = 8;
								y = 8;
							}
							else
							{
								if (x >= 7)
								{
									x = 7;
									y = 12;
								}
								else
								{
									if (x >= 6)
									{
										x = 6;
										y = 8;
									}
									else
									{
										if (x >= 5)
										{
											x = 5;
											y = 12;
										}
										else
										{
											x = 4;
											y = 6;
										}
									}
								}
							}
						}
					}
				}
			}
		}

		public void SetFontSize(int width, int height)
		{
			if (!isVista())
			{
				throw new InvalidOperationException(
					"Setting console font size requires Windows Vista or better. You can set colors using the console properties window: Settings/Properties");
			}
			uint num;
			Native.CONSOLE_FONT_INFOEX consoleFontInfo;
			checked
			{
				num = (uint)((int)Native.GetStdHandle(4294967285u));
				consoleFontInfo = default(Native.CONSOLE_FONT_INFOEX);
				consoleFontInfo.cbSize = 84u;
				consoleFontInfo.dwFontSize.X = (short)width;
				consoleFontInfo.dwFontSize.Y = (short)height;
				consoleFontInfo.FaceName = GetFontFamily();
				if (Operators.CompareString(consoleFontInfo.FaceName.ToLower(), "terminal", false) == 0)
					FindMatch(ref consoleFontInfo.dwFontSize.X, ref consoleFontInfo.dwFontSize.Y);
			}
			Native.GetConsoleFontSize((IntPtr)((long)(num)), 4);
			Native.SetCurrentConsoleFontEx(num, 0u, ref consoleFontInfo);
		}

		public int GetFontSize()
		{
			uint num = checked((uint)((int)Native.GetStdHandle(4294967285u)));
			if (isVista())
			{
				Native.CONSOLE_FONT_INFOEX consoleFontInfo = default(Native.CONSOLE_FONT_INFOEX);
				consoleFontInfo.cbSize = 84u;
				Native.GetCurrentConsoleFontEx(num, 0u, ref consoleFontInfo);
				return consoleFontInfo.dwFontSize.Y;
			}
			return Native.GetConsoleFontSize((IntPtr)((long)(num)), 4).Y;
		}

		public string GetFontFamily()
		{
			if (!isVista())
				return "";
			uint hConsole = checked((uint)((int)Native.GetStdHandle(4294967285u)));
			Native.CONSOLE_FONT_INFOEX consoleFontInfo = default(Native.CONSOLE_FONT_INFOEX);
			consoleFontInfo.cbSize = 84u;
			Native.GetCurrentConsoleFontEx(hConsole, 0u, ref consoleFontInfo);
			return consoleFontInfo.FaceName;
		}

		public void ShowSmall()
		{
			uint num = checked((uint)((int)Native.GetStdHandle(4294967285u)));
			Native.CONSOLE_FONT_INFOEX consoleFontInfo = default(Native.CONSOLE_FONT_INFOEX);
			consoleFontInfo.cbSize = 84u;
			Native.GetCurrentConsoleFontEx(num, 0u, ref consoleFontInfo);
			_currentFontFace = consoleFontInfo.FaceName;
			_currentFontSize = consoleFontInfo.dwFontSize;
			consoleFontInfo = default(Native.CONSOLE_FONT_INFOEX);
			consoleFontInfo.cbSize = 84u;
			consoleFontInfo.dwFontSize.X = 10;
			consoleFontInfo.dwFontSize.Y = 10;
			consoleFontInfo.FaceName = "Lucida Console";
			Native.GetConsoleFontSize((IntPtr)((long)(num)), 4);
			Native.SetCurrentConsoleFontEx(num, 0u, ref consoleFontInfo);
		}

		public void ShowNormal()
		{
			uint hConsole = checked((uint)((int)Native.GetStdHandle(4294967285u)));
			Native.CONSOLE_FONT_INFOEX consoleFontInfo = default(Native.CONSOLE_FONT_INFOEX);
			consoleFontInfo.cbSize = 84u;
			consoleFontInfo.dwFontSize = _currentFontSize;
			consoleFontInfo.FaceName = _currentFontFace;
			Native.SetCurrentConsoleFontEx(hConsole, 0u, ref consoleFontInfo);
		}

		public void FontSizeEx(int value)
		{
			uint num;
			Native.CONSOLE_FONT_INFOEX consoleFontInfo;
			checked
			{
				num = (uint)((int)Native.GetStdHandle(4294967285u));
				consoleFontInfo = default(Native.CONSOLE_FONT_INFOEX);
				consoleFontInfo.cbSize = 84u;
				consoleFontInfo.dwFontSize.X = (short)value;
				consoleFontInfo.dwFontSize.Y = (short)value;
				consoleFontInfo.FaceName = "Consolas";
			}
			Native.GetConsoleFontSize((IntPtr)((long)(num)), 4);
			Native.SetCurrentConsoleFontEx(num, 0u, ref consoleFontInfo);
		}

		public void CancelContext()
		{
			Native.PostMessage((IntPtr)hwnd, 31, (IntPtr)0, (IntPtr)0);
		}

		public void SendTextToConsole(string text, bool insertnewline)
		{
			text = text.Trim().Replace("\t", "    ");
			if (insertnewline)
				text += "\r\n";
			Native.SendText(text);
		}

		public void ShowProperties()
		{
			bool flag = false;
			OnPropertyPageShowEventHandler onPropertyPageShowEventHandler = onPropertyPageShowEvent;
			if (onPropertyPageShowEventHandler != null)
				onPropertyPageShowEventHandler(ref flag);
			if (flag)
				return;
			ew.GetWindows();
			_windowMap = new Dictionary<int, string>();
			IEnumerator enumerator = ew.Items.GetEnumerator();
			while (enumerator.MoveNext())
			{
				EnumWindowsItem enumWindowsItem = (EnumWindowsItem)enumerator.Current;
				if (Operators.CompareString(enumWindowsItem.ClassName, _PropertyClass, false) == 0)
					_windowMap.Add((int)enumWindowsItem.Handle, enumWindowsItem.Text);
			}
			Native.SendMessage((IntPtr)hwnd, 274u, (IntPtr)65527, (IntPtr)1);
			_PropertyFound = false;
			safetycount = 5;
			PropertyWatcher.Interval = 300;
			PropertyWatcher.Enabled = true;
		}

		public Message ProcessSizeMsg(Message m)
		{
			if (_isDragging)
			{
				bool flag = ModifierKeys == Keys.Shift;
				if (flag & isVista())
					return m;
			}
			Native.RECT rECT = (Native.RECT)Marshal.PtrToStructure(m.LParam, typeof(Native.RECT));
			checked
			{
				int num = rECT.bottom - rECT.top;
				int num2 = rECT.right - rECT.left;
				int num3 = num2 - Parent.Width;
				int num4 = num - Parent.Height;
				if (!(num3 == 0 & num4 == 0))
				{
					Size desiredSize = new Size(ContainerPanel.Width + num3, ContainerPanel.Height + num4);
					Size size = CalcSize(desiredSize);
					num2 = Parent.Width + (size.Width - ContainerPanel.Width);
					num = Parent.Height + (size.Height - ContainerPanel.Height) + 1;
					if (m.WParam.ToInt32() == 3 | m.WParam.ToInt32() == 4 | m.WParam.ToInt32() == 5)
						rECT.top = rECT.bottom - num;
					else
						rECT.bottom = rECT.top + num;
					if (m.WParam.ToInt32() == 1 | m.WParam.ToInt32() == 4 | m.WParam.ToInt32() == 7)
						rECT.left = rECT.right - num2;
					else
						rECT.right = rECT.left + num2;
				}
				Marshal.StructureToPtr(rECT, m.LParam, true);
				return m;
			}
		}

		public void AdjustBlank()
		{
			object sl_adjustblank = _sl_adjustblank;
			ObjectFlowControl.CheckForSyncLockOnValueType(sl_adjustblank);
			Monitor.Enter(sl_adjustblank);
			checked
			{
				try
				{
					if (Parent != null && Parent.Visible)
					{
						Control parent = Parent;
						parent.Width -= ContainerPanel.Width - EventsPanel.Width;
						parent = Parent;
						parent.Height -= ContainerPanel.Height - EventsPanel.Height;
					}
					Refresh();
				}
				catch (Exception expr_9A)
				{
					ProjectData.SetProjectError(expr_9A);
					Exception ex = expr_9A;

					ProjectData.ClearProjectError();
				}
				finally
				{
					Monitor.Exit(sl_adjustblank);
				}
			}
		}

		public Size GetExcessSpace()
		{
			return checked(new Size
			{
				Width = ContainerPanel.Width - EventsPanel.Width,
				Height = ContainerPanel.Height - EventsPanel.Height
			});
		}

		private void AdjustScrollbars(Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo)
		{
			UpdateSize(false);
		}

		internal static string GetConsoleWindowTitle()
		{
			StringBuilder stringBuilder = new StringBuilder(1024);
			if (Native.GetConsoleTitle(stringBuilder, 1024) == 0uL)
				return "";
			return stringBuilder.ToString();
		}

		public void CheckForUpdate()
		{
			try
			{
				string consoleWindowTitle = GetConsoleWindowTitle();
				if (Operators.CompareString(consoleWindowTitle, _oldTitle, false) != 0)
				{
					_oldTitle = consoleWindowTitle;
					OnTitleChangeEventHandler onTitleChangeEventHandler = _onTitleChangeEvent;
					if (onTitleChangeEventHandler != null)
						onTitleChangeEventHandler(_oldTitle);
				}
				Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo2;
				using (GetBufferInfo(out bufferInfo2))
				{
				}
				if (bufferInfo2.BufferSize.X != _oldBufferX | bufferInfo2.BufferSize.Y != _oldBuffery | bufferInfo2.WindowRect.Left != _oldWindowLeft |
					bufferInfo2.WindowRect.Right != _oldWindowRight | bufferInfo2.WindowRect.Top != _oldWindowTop | bufferInfo2.WindowRect.Bottom != _oldWindowBottom)
				{
					_oldBufferX = bufferInfo2.BufferSize.X;
					_oldBuffery = bufferInfo2.BufferSize.Y;
					_oldWindowLeft = bufferInfo2.WindowRect.Left;
					_oldWindowRight = bufferInfo2.WindowRect.Right;
					_oldWindowTop = bufferInfo2.WindowRect.Top;
					_oldWindowBottom = bufferInfo2.WindowRect.Bottom;
					AdjustScrollbars(bufferInfo2);
				}
			}
			catch (Exception expr_150)
			{
				ProjectData.SetProjectError(expr_150);
				Exception ex = expr_150;

				ProjectData.ClearProjectError();
			}
		}

		public void SetWindowSize(Size size)
		{
			SetWindowSize(size.Width, size.Height);
		}

		public void SetWindowSize(int x, int y)
		{
			if (hwnd == 0)
				return;
			s = FontSize;
		}

		public void SetFixedSize()
		{
			checked
			{
				Native.CONSOLE_SCREEN_BUFFER_INFO screenBufferInfo;
				using (GetBufferInfo(out screenBufferInfo))
				{
					SetFixedSize(
						(unchecked(screenBufferInfo.WindowRect.Right - screenBufferInfo.WindowRect.Left) + 1),
						(unchecked(screenBufferInfo.WindowRect.Bottom - screenBufferInfo.WindowRect.Top) + 1));
				}
			}
		}

		public void SetFixedSize(int width, int height)
		{
			s = FontSize;
			checked
			{
				int num = ContainerPanel.Width - 2 * _offsetFrame;
				int num2 = ContainerPanel.Height - 2 * _offsetFrame;
				int num3 = (int)Math.Round(Conversion.Fix(num / (double)s.Width));
				int num4 = (int)Math.Round(Conversion.Fix(num2 / (double)s.Height));
				int num5 = width - num3;
				int num6 = height - num4;
				Form parentForm = ParentForm;
				parentForm.Width += num5 * s.Width;
				parentForm = ParentForm;
				parentForm.Height += num6 * s.Height;
			}
		}

		public Size UpdateSize(bool keep = false)
		{
			checked
			{
				Size size = new Size();
				if (!_layoutsuspended)
				{
					if (hwnd != 0)
					{
						object updateSizer = _updateSizer;
						ObjectFlowControl.CheckForSyncLockOnValueType(updateSizer);
						Monitor.Enter(updateSizer);
						try
						{
							s = FontSize;
							int overallWidth = ContainerPanel.Width - 2 * _offsetFrame;
							int overallHeight = ContainerPanel.Height - 2 * _offsetFrame;
							int columns = (int)Math.Round(Conversion.Fix(overallWidth / (double)s.Width));
							int rows = (int)Math.Round(Conversion.Fix(overallHeight / (double)s.Height));
							Native.CONSOLE_SCREEN_BUFFER_INFO consoleFontInfo;
							using (GetBufferInfo(out consoleFontInfo))
							{
							}
							Size bufferSize = new Size(consoleFontInfo.BufferSize.X, consoleFontInfo.BufferSize.Y);
							bool horizontalScroll = bufferSize.Width > columns;
							bool verticalScroll = bufferSize.Height > rows;
							int horizontalScrollbarHeight = Native.GetSystemMetrics(3);
							int verticalScrollbarWidth = Native.GetSystemMetrics(2);
							if (horizontalScroll)
							{
								rows = (int)Math.Round(Conversion.Fix((overallHeight - horizontalScrollbarHeight) / (double)s.Height));
								verticalScroll = (bufferSize.Height > rows);
							}
							if (verticalScroll)
							{
								columns = (int)Math.Round(Conversion.Fix((overallWidth - verticalScrollbarWidth) / (double)s.Width));
								if (!horizontalScroll && bufferSize.Width > columns)
								{
									horizontalScroll = true;
									rows = (int)Math.Round(Conversion.Fix((overallHeight - horizontalScrollbarHeight) / (double)s.Height));
								}
							}
							NewConsoleWidth = columns;
							if (CanAutoAdjust)
							{
								bool flag3 = false;
								if (bufferSize.Width < columns)
								{
									bufferSize.Width = columns;
									flag3 = true;
								}
								if (bufferSize.Height < rows)
								{
									bufferSize.Height = rows;
									flag3 = true;
								}
								if (flag3)
									BufferSize = bufferSize;
							}
							else
							{
								if (bufferSize.Width < columns)
									columns = bufferSize.Width;
								if (bufferSize.Height < rows)
									rows = bufferSize.Height;
							}
							SuspendLayout();
							size = new Size(columns, rows);
							WindowSize = size;
							bool flag4 = false;
							if (oldX > 0 && oldY > 0 && (Math.Abs(oldX - columns) > 2 || Math.Abs(oldY - rows) > 2))
								flag4 = true;
							oldX = columns;
							oldY = rows;
							if (flag4 || MouseButtons == MouseButtons.Left)
							{
								Timer2.Enabled = false;
								lblConsoleSize.Text = Conversions.ToString(columns) + " x " + Conversions.ToString(rows);
								lblConsoleSize.Refresh();
								lblConsoleSize.Left = (int)Math.Round(unchecked(Width / 2.0 - lblConsoleSize.Width / 2.0));
								lblConsoleSize.Top = (int)Math.Round(unchecked(Height / 2.0 - lblConsoleSize.Height / 2.0));
								if (isVista())
									lblConsoleSize.BackColor = Helper.NativeColor2Color(GetBackColor());
								else
									lblConsoleSize.BackColor = Helper.NativeColor2Color(Helper.GetConsoleBackgroundColorFromReg(Application.ExecutablePath));
								lblConsoleSize.Visible = true;
								Timer2.Interval = 600;
								Timer2.Enabled = true;
							}
							int newWidth = columns * s.Width;
							int newHeight = rows * s.Height;
							int num7 = Native.CreateRectRgn(_offsetX - 1, _offsetY, newWidth + _offsetX, newHeight + _offsetY);
							try
							{
								Native.SetWindowRgn(hwnd, num7, 1);
							}
							catch (Exception expr_3F1)
							{
								ProjectData.SetProjectError(expr_3F1);
								Exception ex = expr_3F1;

								ProjectData.ClearProjectError();
							}
							Native.DeleteObject(num7);
							ConsolePanel.Width = newWidth - 2 * _offsetFrame - 1;
							ConsolePanel.Height = newHeight - 2 * _offsetFrame;
							SuspendUpdate();
							if (horizontalScroll)
							{
								EventsPanel.Height = newHeight + _offsetFrame + verticalScrollbarWidth;
								EventsPanel.SetHScrollbar(
									unchecked(consoleFontInfo.BufferSize.X - (consoleFontInfo.WindowRect.Right - consoleFontInfo.WindowRect.Left)),
									consoleFontInfo.WindowRect.Left);
								HScrollBar1.Maximum = EventsPanel.HScrollBar1.Maximum;
								HScrollBar1.Value = EventsPanel.HScrollBar1.Value;
								EventsPanel.scrollH = true;
								HScrollBar1.Visible = true;
							}
							else
							{
								EventsPanel.scrollH = false;
								HScrollBar1.Visible = false;
								EventsPanel.Height = newHeight + _offsetFrame;
							}
							if (verticalScroll)
							{
								EventsPanel.SetVScrollbar(
									unchecked(consoleFontInfo.BufferSize.Y - (consoleFontInfo.WindowRect.Bottom - consoleFontInfo.WindowRect.Top)),
									consoleFontInfo.WindowRect.Top);
								VScrollBar1.Maximum = EventsPanel.VScrollBar1.Maximum;
								VScrollBar1.Value = EventsPanel.VScrollBar1.Value;
								EventsPanel.scrollV = true;
								VScrollBar1.Visible = true;
								EventsPanel.Width = newWidth + _offsetFrame + horizontalScrollbarHeight;
							}
							else
							{
								EventsPanel.scrollV = false;
								VScrollBar1.Visible = false;
								EventsPanel.Width = newWidth + _offsetFrame;
							}
							ResumeUpdate();
							Adjustbars();
							OnScrollBarEventHandler onScrollBarEventHandler = onScrollBarEvent;
							if (onScrollBarEventHandler != null)
								onScrollBarEventHandler(horizontalScroll, verticalScroll);
							ResumeLayout();
							consoleFontInfo = default(Native.CONSOLE_SCREEN_BUFFER_INFO);
							bufferSize = default(Size);
						}
						finally
						{
							Monitor.Exit(updateSizer);
						}
					}
				}
				return size;
			}
		}

		private void Adjustbars()
		{
			HScrollBar hScrollBar = HScrollBar1;
			if (hScrollBar.Left != Panel1.Left)
				hScrollBar.Left = Panel1.Left;
			checked
			{
				if (hScrollBar.Top != Panel1.Bottom - hScrollBar.Height)
					hScrollBar.Top = Panel1.Bottom - hScrollBar.Height;
				if (VScrollBar1.Visible & hScrollBar.Visible)
				{
					if (hScrollBar.Width != Panel1.Width - VScrollBar1.Width + 1)
						hScrollBar.Width = Panel1.Width - VScrollBar1.Width + 1;
				}
				else
				{
					if (hScrollBar.Width != Panel1.Width)
						hScrollBar.Width = Panel1.Width;
				}
				VScrollBar vScrollBar = VScrollBar1;
				if (vScrollBar.Left != Panel1.Right - vScrollBar.Width)
					vScrollBar.Left = Panel1.Right - vScrollBar.Width;
				if (vScrollBar.Top != Panel1.Top)
					vScrollBar.Top = Panel1.Top;
				if (HScrollBar1.Visible & vScrollBar.Visible)
				{
					if (vScrollBar.Height != Panel1.Height - HScrollBar1.Height + 1)
						vScrollBar.Height = Panel1.Height - HScrollBar1.Height + 1;
				}
				else
				{
					if (vScrollBar.Height != Panel1.Height)
						vScrollBar.Height = Panel1.Height;
				}
			}
		}

		public Size UpdateFontSize(bool keep = false)
		{
			checked
			{
				if (hwnd != 0)
				{
					int num = ContainerPanel.Width - 2 * _offsetFrame;
					int num2 = ContainerPanel.Height - 2 * _offsetFrame;
					Native.CONSOLE_SCREEN_BUFFER_INFO screenBufferInfo;

					using (GetBufferInfo(out screenBufferInfo))
					{
					}

					int width = (int)Math.Round(unchecked(Conversion.Fix(num / (double)_windowWidth) + 1.0));
					int height = (int)Math.Round(unchecked(Conversion.Fix(num2 / (double)_windowHeight) + 1.0));
					SetFontSize(width, height);
					UpdateSize(false);
				}
				return new Size();
			}
		}

		public void StartDrag()
		{
			_isDragging = true;
			checked
			{
				Native.CONSOLE_SCREEN_BUFFER_INFO screenBufferInfo;
				using (GetBufferInfo(out screenBufferInfo))
				{
					_windowWidth = (unchecked(screenBufferInfo.WindowRect.Right - screenBufferInfo.WindowRect.Left) + 1);
					_windowHeight = (unchecked(screenBufferInfo.WindowRect.Bottom - screenBufferInfo.WindowRect.Top) + 1);
				}
			}
		}

		public void EndDrag()
		{
			_isDragging = false;
			AdjustBlank();
		}

		public void FlushInput()
		{
			IntPtr stdHandle = Native.GetStdHandle(4294967286u);
			Native.FlushConsoleInputBuffer(stdHandle);
		}

		public string GetHardcopy(ref string plaintext)
		{
			IntPtr stdHandle = Native.GetStdHandle(4294967285u);
			int num = 0;
			int num2 = 0;
			GetConsoleColorIndex(ref num, ref num2);
			checked
			{
				Native.CONSOLE_SCREEN_BUFFER_INFO screenBufferInfo;
				if (Native.GetConsoleScreenBufferInfo(stdHandle, out screenBufferInfo))
				{
					int x = screenBufferInfo.BufferSize.X;
					int y = screenBufferInfo.BufferSize.Y;
					Native.CHAR_INFO[] array = new Native.CHAR_INFO[x + 1];
					Color[] array2 = new Color[17];
					uint[] consoleColorTable = Helper.GetConsoleColorTable();
					int num3 = 0;
					do
					{
						array2[num3] = ColorTranslator.FromWin32((int)consoleColorTable[num3]);
						num3++;
					} while (num3 <= 15);
					Native.Coord bufferSize;
					bufferSize.X = (short)x;
					bufferSize.Y = 1;
					Native.Coord bufferCoord;
					bufferCoord.X = 0;
					bufferCoord.Y = 0;
					string text = "Courier New";
					int value = 8;
					try
					{
						text = GetFontFamily();
					}
					catch (Exception expr_B6)
					{
						ProjectData.SetProjectError(expr_B6);
						Exception ex = expr_B6;

						ProjectData.ClearProjectError();
					}
					try
					{
						value = FontSize.Height;
					}
					catch (Exception expr_128)
					{
						ProjectData.SetProjectError(expr_128);
						Exception ex2 = expr_128;

						ProjectData.ClearProjectError();
					}
					if (Operators.CompareString(text, "Terminal", false) == 0)
						text = "Courier New";
					if (Operators.CompareString(text, "", false) == 0)
						text = "Courier New";
					StringBuilder stringBuilder = new StringBuilder();
					StringBuilder stringBuilder2 = new StringBuilder();
					stringBuilder.AppendLine("<html><body>");
					stringBuilder.AppendLine("<!-- created by PowerShell Plus: http://www.powershell.com -->");
					stringBuilder.AppendLine(
						string.Concat(
							new string[]
								{
									"<div style=\"font-family:'",
									text,
									"';font-size:",
									Conversions.ToString(value),
									";background:",
									ColorTranslator.ToHtml(array2[(int) Console.BackgroundColor]),
									"\">"
								}));
					stringBuilder.Append("<span style=\"COLOR: white;background:black\">");
					string right = "";
					char[] array3 = Conversions.ToCharArrayRankOne("&nbsp;");
					int num4 = 0;
					int arg_26D_0 = 0;
					int num5 = y - 1;
					for (int i = arg_26D_0; i <= num5; i++)
					{
						Native.SMALL_RECT sMALL_RECT;
						sMALL_RECT.Top = (short)i;
						sMALL_RECT.Left = 0;
						sMALL_RECT.Bottom = (short)(i + 1);
						sMALL_RECT.Right = (short)(x - 1);
						if (!Native.ReadConsoleOutput(stdHandle, array, bufferSize, bufferCoord, ref sMALL_RECT))
						{
							MessageBox.Show("Unable to read console content. Please contact Support.");
							return "";
						}
						StringBuilder stringBuilder3 = new StringBuilder();
						StringBuilder stringBuilder4 = new StringBuilder();
						StringBuilder stringBuilder5 = new StringBuilder();
						int arg_2F0_0 = array.GetLowerBound(0);
						int upperBound = array.GetUpperBound(0);
						int num9 = 0;
						for (int j = arg_2F0_0; j <= upperBound; j++)
						{
							int num7 = num;
							int num8 = num2;
							if (array[j].UnicodeChar != 0)
							{
								num7 = (array[j].Attributes & 15);
								num8 = (array[j].Attributes & 240) >> 4;
							}
							Color color = array2[num7];
							Color color2 = array2[num8];
							Color right2 = new Color();
							Color right3 = new Color();
							if (color != right2 | color2 != right3)
							{
								if (stringBuilder3.Length > 0)
								{
									if (num8 == num2 & num9 == num2)
									{
										stringBuilder5.Append(stringBuilder3.ToString().TrimEnd(new char[0]));
										stringBuilder4.Append(HttpUtility.HtmlEncode(stringBuilder3.ToString().TrimEnd(new char[0])).Replace(" ", "&nbsp;"));
									}
									else
									{
										stringBuilder5.Append(stringBuilder3);
										stringBuilder4.Append(HttpUtility.HtmlEncode(stringBuilder3.ToString()).Replace(" ", "&nbsp;"));
									}
								}
								if ((!(color2 == right3) || !(color == right2)) && array[j].UnicodeChar != 0)
								{
									string text2 = string.Concat(
										new string[]
											{
												"</span><span style=\"color:",
												ColorTranslator.ToHtml(color),
												";background-color:",
												ColorTranslator.ToHtml(color2),
												"\">"
											});
									if (Operators.CompareString(text2, right, false) != 0)
									{
										stringBuilder4.Append(text2);
										right = text2;
									}
								}
								right2 = color;
								right3 = color2;
								num9 = num8;
								stringBuilder3 = new StringBuilder();
							}
							if (array[j].UnicodeChar != 0)
								stringBuilder3.Append(Strings.ChrW(array[j].UnicodeChar));
						}
						string text3 = stringBuilder3.ToString();
						if (num9 == num2)
							text3 = text3.TrimEnd(new char[0]);
						stringBuilder4.Append(HttpUtility.HtmlEncode(text3).Replace(" ", "&nbsp;"));
						plaintext = plaintext + stringBuilder5 + text3 + "\r\n";
						if (stringBuilder5.Length > num4)
							num4 = stringBuilder5.Length;
						stringBuilder5.Append(stringBuilder3.ToString().TrimEnd(new char[0]));
						if (Operators.CompareString(stringBuilder5.ToString(), "", false) == 0)
							stringBuilder2.AppendLine(stringBuilder4.ToString());
						else
						{
							stringBuilder.Append(stringBuilder2);
							stringBuilder2 = new StringBuilder();
							stringBuilder.AppendLine(stringBuilder4.ToString());
						}
					}
					plaintext = plaintext.Trim();
					stringBuilder.AppendLine("</span></div>");
					stringBuilder.AppendLine("<!-- end HTML fragment created by PowerShell Plus http://www.powershell.com -->");
					string result = (stringBuilder.ToString().Trim() + "</body></html>").Replace("\r\n", "<br>\r\n");
					if (num4 > 0)
					{
						int num10 = x - num4 - 5;
						if (num10 > 0)
						{
						}
					}
					return result;
				}
				return "";
			}
		}

		public void AddTrigger(string key)
		{
			EventsPanel.AddTrigger(key);
		}

		public void RemoveTrigger(string key)
		{
			EventsPanel.RemoveTrigger(key);
		}

		public void EnableFormMove(int hwnd)
		{
			_moveform = true;
		}

		public void DisableFormMove()
		{
			_moveform = false;
		}

		public void SetFocus(IntPtr hwnd)
		{
			Native.SetFocusThread(hwnd);
		}

		public bool CheckForSelection(bool cancel)
		{
			Native.CONSOLE_SELECTION_INFO selectionInfo = default(Native.CONSOLE_SELECTION_INFO);
			if (Native.GetConsoleSelectionInfo(ref selectionInfo) && selectionInfo.dwFlags != Native.SelectionFlags.NoSelection)
			{
				if (cancel)
					Native.PostMessage((IntPtr)hwnd, 256, (IntPtr)27, (IntPtr)1);
				else
					Native.PostMessage((IntPtr)hwnd, 256, (IntPtr)13, (IntPtr)0);
				if (_isOneClickQuickEdit)
				{
					QuickEditMode = false;
					OnOneQuickEditDoneEventHandler onOneQuickEditDoneEventHandler = onOneQuickEditDoneEvent;
					if (onOneQuickEditDoneEventHandler != null)
						onOneQuickEditDoneEventHandler();
				}
				return true;
			}
			return false;
		}

		private Size CalcSize(Size desiredSize)
		{
			checked
			{
				if (hwnd != 0)
				{
					int num = desiredSize.Width - 2 * _offsetFrame;
					int num2 = desiredSize.Height - 2 * _offsetFrame;
					int num3 = (int)Math.Round(Conversion.Fix(num / (double)s.Width));
					int num4 = (int)Math.Round(Conversion.Fix(num2 / (double)s.Height));
					Size bufferSize = BufferSize;
					bool flag = bufferSize.Width > num3;
					bool flag2 = bufferSize.Height > num4;
					int systemMetrics = Native.GetSystemMetrics(3);
					int systemMetrics2 = Native.GetSystemMetrics(2);
					if (flag)
					{
						num4 = (int)Math.Round(Conversion.Fix((num2 - systemMetrics2) / (double)s.Height));
						flag2 = (bufferSize.Height > num4);
					}
					if (flag2)
					{
						num3 = (int)Math.Round(Conversion.Fix((num - systemMetrics) / (double)s.Width));
						if (!flag && bufferSize.Width > num3)
						{
							flag = true;
							num4 = (int)Math.Round(Conversion.Fix((num2 - systemMetrics2) / (double)s.Height));
						}
					}
					if (CanAutoAdjust)
					{
						Size bufferSize2 = BufferSize;
						bool flag3 = false;
						if (bufferSize.Width < num3)
						{
							flag3 = true;
							bufferSize2.Width = num3;
						}
						if (bufferSize.Height < num4)
						{
							flag3 = true;
							bufferSize2.Height = num4;
						}
						if (flag3)
							BufferSize = bufferSize2;
					}
					else
					{
						if (bufferSize.Width < num3)
							num3 = bufferSize.Width;
						if (bufferSize.Height < num4)
							num4 = bufferSize.Height;
					}
					if (num3 < MinSize.Width)
						num3 = MinSize.Width;
					int arg_1C7_0 = num4;
					Size minSize = MinSize;
					if (arg_1C7_0 < minSize.Height)
						num4 = MinSize.Height;
					int num5 = num3 * s.Width;
					int num6 = num4 * s.Height;
					if (flag)
						num6 += systemMetrics;
					if (flag2)
						num5 += systemMetrics2;
					if (flag && !EventsPanel.scrollH && num6 < desiredSize.Height)
						num6 += s.Height;
					if (flag2 && !EventsPanel.scrollV && num5 < desiredSize.Width)
						num5 += s.Width;
					minSize = new Size(num5 + 2 * _offsetFrame, num6 + 2 * _offsetFrame);
					return minSize;
				}
				return new Size();
			}
		}

		private void SetConsoleScreenBufferSize(SafeFileHandle consoleHandle, Size newSize)
		{
			checked
			{
				Native.Coord size;
				size.X = (short)newSize.Width;
				size.Y = (short)newSize.Height;
				Native.SetConsoleScreenBufferSize(consoleHandle.DangerousGetHandle(), size);
			}
		}

		private static SafeFileHandle GetBufferInfo(out Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo)
		{
			object obj = new SafeFileHandle(Native.CreateFile("CONOUT$", 3221225472u, 2u, IntPtr.Zero, 3u, 0u, IntPtr.Zero), true);
			bufferInfo = GetConsoleScreenBufferInfo((SafeFileHandle)obj);
			return (SafeFileHandle)obj;
		}

		private static Native.CONSOLE_SCREEN_BUFFER_INFO GetConsoleScreenBufferInfo(SafeFileHandle consoleHandle)
		{
			Native.CONSOLE_SCREEN_BUFFER_INFO result;
			Native.GetConsoleScreenBufferInfo(consoleHandle.DangerousGetHandle(), out result);
			return result;
		}

		private static void SetConsoleWindowInfo(SafeFileHandle consoleHandle, bool absolute, Native.SMALL_RECT windowInfo)
		{
			if (!Native.SetConsoleWindowInfo(consoleHandle.DangerousGetHandle(), absolute, ref windowInfo))
				Console.WriteLine("ERR:" + Conversions.ToString(Marshal.GetLastWin32Error()));
		}

		internal static SafeFileHandle GetInputHandle()
		{
			SafeFileHandle safeFileHandle = new SafeFileHandle(Native.CreateFile("CONIN$", 3221225472u, 1u, IntPtr.Zero, 3u, 0u, IntPtr.Zero), true);
			bool arg_29_0 = safeFileHandle.IsInvalid;
			return safeFileHandle;
		}

		internal static Native.ConsoleModes GetMode(SafeFileHandle consoleHandle)
		{
			uint result = 0u;
			Native.GetConsoleMode(consoleHandle.DangerousGetHandle(), out result);
			return (Native.ConsoleModes)result;
		}

		internal static void SetMode(SafeFileHandle consoleHandle, Native.ConsoleModes mode)
		{
			if (!Native.SetConsoleMode(consoleHandle.DangerousGetHandle(), (uint)mode))
				return;
		}

		internal static int ReadConsoleInput(SafeFileHandle consoleHandle, ref Native.INPUT_RECORD[] buffer)
		{
			uint num = 0u;
			checked
			{
				Native.ReadConsoleInput(consoleHandle.DangerousGetHandle(), buffer, (uint)buffer.Length, out num);
				return (int)num;
			}
		}

		private static void CheckCoordinateWithinBuffer(ref Coordinates c, ref Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo, string paramName)
		{
			if (c.X < 0 || c.X > bufferInfo.BufferSize.X)
			{
			}
			if (c.Y < 0 || c.Y > bufferInfo.BufferSize.Y)
			{
			}
		}

		internal static void SetConsoleCursorPosition(SafeFileHandle consoleHandle, Coordinates cursorPosition)
		{
			checked
			{
				Native.Coord cursorPosition2;
				cursorPosition2.X = (short)cursorPosition.X;
				cursorPosition2.Y = (short)cursorPosition.Y;
				Native.SetConsoleCursorPosition(consoleHandle.DangerousGetHandle(), cursorPosition2);
			}
		}

		private void HandleArrowUp()
		{
			Coordinates cursorPosition = CursorPosition;
			checked
			{
				cursorPosition.Y--;
				CursorPosition = cursorPosition;
			}
		}

		private void TransparentPanel1_Click(object sender, EventArgs e)
		{
			Native.SetFocusThread(Parent.Handle);
			Native.SendMessage((IntPtr)hwnd, 7u, (IntPtr)(-1L), (IntPtr)0);
		}

		private void TransparentPanel1_DoubleClick(object sender, EventArgs e)
		{
			OnDoubleClickEventHandler onDoubleClickEventHandler = _onDoubleClickEvent;
			if (onDoubleClickEventHandler != null)
				onDoubleClickEventHandler();
		}

		private void ConsoleControl_onCtrlC(ref bool cancel)
		{
		}

		internal void AdjustColorPanel()
		{
			p6.Left = 0;
			p6.Top = 0;
			checked
			{
				if (EventsPanel.VScrollBar1.Visible)
					p6.Width = EventsPanel.Width - EventsPanel.VScrollBar1.Width;
				else
					p6.Width = ContainerPanel.Width;
				if (EventsPanel.HScrollBar1.Visible)
					p6.Height = EventsPanel.Height - EventsPanel.HScrollBar1.Height;
				else
					p6.Height = ContainerPanel.Height;
			}
		}

		private void PSControl_Resize(object sender, EventArgs e)
		{
			bool flag = ModifierKeys == Keys.Shift;
			AdjustColorPanel();
			BeginUpdate();
			if (flag)
			{
				if (isVista())
				{
					if (_isDragging)
						UpdateFontSize(false);
					else
						UpdateSize(false);
				}
				else
					UpdateSize(false);
			}
			else
				UpdateSize(false);
			EndUpdate();
		}

		private void TransparentPanel1_GotFocus(object sender, EventArgs e)
		{
			ShowCursor();
			OnEnterEventHandler onEnterEventHandler = _onEnterEvent;
			if (onEnterEventHandler != null)
				onEnterEventHandler();
		}

		public void KillFocus()
		{
			HideCursor();
		}

		private void TransparentPanel1_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				if (QuickEditMode || RightClickPaste)
				{
					if (!CheckForSelection(false))
					{
						if (Clipboard.ContainsText())
							SendTextToConsole(Clipboard.GetText(), false);
						return;
					}
				}
				else
				{
					MinimodeToolStripMenuItem1.Visible = false;
					ContextMenuStrip1.Show(this, e.Location);
				}
				return;
			}
			bool flag = ModifierKeys == Keys.Shift;
			bool flag2;
			if (QuickEditMode)
				flag2 = (e.Button == MouseButtons.Left & _moveform & e.Clicks == 1 & flag);
			else
				flag2 = (e.Button == MouseButtons.Left & _moveform & e.Clicks == 1);
			if (flag2)
			{
				Native.ReleaseCapture();
				Native.SendMessage(ParentForm.Handle, 161u, (IntPtr)2L, (IntPtr)0);
			}
		}

		public bool RightClickPaste
		{
			get;
			set;
		}

		private void TransparentPanel1_onAlt()
		{
			OnMiniModeAltEventHandler onMiniModeAltEventHandler = onMiniModeAltEvent;
			if (onMiniModeAltEventHandler != null)
				onMiniModeAltEventHandler();
		}

		private string ReadTextFile(string path)
		{
			if (!File.Exists(path))
				return "";
			string result;
			try
			{
				StreamReader streamReader = new StreamReader(path);
				string text = streamReader.ReadToEnd();
				streamReader.Close();
				result = text;
			}
			catch (Exception expr_26)
			{
				ProjectData.SetProjectError(expr_26);
				result = "";
				ProjectData.ClearProjectError();
			}
			return result;
		}

		private void TransparentPanel1_DragDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] array = (string[])e.Data.GetData(DataFormats.FileDrop);
				if (array[0].ToLower().EndsWith(".license"))
				{
					OnLicenseAddedEventHandler onLicenseAddedEventHandler = _onLicenseAddedEvent;
					if (onLicenseAddedEventHandler != null)
						onLicenseAddedEventHandler(array[0]);
				}
				else
				{
					if (EventsPanel.isNativeApp)
						Native.SendText("\"" + array[0] + "\"");
				}
			}
			else
			{
				string text2 = Conversions.ToString(e.Data.GetData(DataFormats.Text));
				if (text2.Contains(" ") && (File.Exists(text2) || Directory.Exists(text2)) && (!text2.Trim().StartsWith("\"") & !text2.Trim().StartsWith("'")))
					text2 = "\"" + text2 + "\"";
				Native.SendText(text2);
			}
			Native.SetFocusThread(Parent.Handle);
			EventsPanel.Focus();
		}

		private void TransparentPanel1_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.Text) | e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.Copy;
			else
				e.Effect = DragDropEffects.None;
		}

		private void TransparentPanel1_onTrigger(string keycode, ref bool cancel)
		{
			OnTriggerEventHandler onTriggerEventHandler = _onTriggerEvent;
			if (onTriggerEventHandler != null)
				onTriggerEventHandler(keycode, ref cancel);
		}

		private void TransparentPanel1_onFunctionKey(Keys keycode, ref bool cancel)
		{
			OnFunctionKeyEventHandler onFunctionKeyEventHandler = _onFunctionKeyEvent;
			if (onFunctionKeyEventHandler != null)
				onFunctionKeyEventHandler(keycode, ref cancel);
		}

		private void TransparentPanel1_onCursorKey(Keys keycode, ref bool cancel)
		{
			OnCursorKeyEventHandler onCursorKeyEventHandler = _onCursorKeyEvent;
			if (onCursorKeyEventHandler != null)
				onCursorKeyEventHandler(keycode, ref cancel);
		}

		private void TransparentPanel1_onALTENTER()
		{
			OnAltEnterEventHandler onALTENTEREventHandler = onALTENTEREvent;
			if (onALTENTEREventHandler != null)
				onALTENTEREventHandler();
		}

		private void TransparentPanel1_onEnter()
		{
			if (_isOneClickQuickEdit)
			{
				QuickEditMode = false;
				OnOneQuickEditDoneEventHandler onOneQuickEditDoneEventHandler = onOneQuickEditDoneEvent;
				if (onOneQuickEditDoneEventHandler != null)
					onOneQuickEditDoneEventHandler();
			}
		}

		public void DoFire(int which)
		{
			checked
			{
				if (which == 1)
				{
					s = FontSize;
					Native.SetWindowPos((IntPtr)hwnd, (IntPtr)0, 0 - _offsetX + _offsetFrame - 1, 0 - _offsetY + _offsetFrame, 0, 0, 1);
					UpdateSize(false);
					AdjustBlank();
				}
			}
		}

		public void ReparentTo(string name)
		{
			string text = "*";
			IntPtr intPtr = (IntPtr)FindWindowLike(ref name, ref text);
			if (intPtr == (IntPtr)0)
			{
				MessageBox.Show("Not found");
				return;
			}
			Native.SetParent(intPtr, ConsolePanel.Handle);
			checked
			{
				Native.SetWindowPos(intPtr, (IntPtr)0, 0 - _offsetX + _offsetFrame, 0 - _offsetY + _offsetFrame, 0, 0, 1);
				UpdateSize(false);
				Parent.Width = Parent.Width - (ContainerPanel.Width - ConsolePanel.Width);
				Parent.Height = Parent.Height - (ContainerPanel.Height - ConsolePanel.Height);
				EventsPanel.hwnd = intPtr;
				EventsPanel.Hide();
			}
		}

		private void PropertyWatcher_Tick(object sender, EventArgs e)
		{
			checked
			{
				if (!_PropertyFound)
				{
					ew.GetWindows();
					safetycount--;

					IEnumerator enumerator = ew.Items.GetEnumerator();
					while (enumerator.MoveNext())
					{
						EnumWindowsItem enumWindowsItem = (EnumWindowsItem)enumerator.Current;
						if (Operators.CompareString(enumWindowsItem.ClassName, _PropertyClass, false) == 0 && !_windowMap.ContainsKey((int)enumWindowsItem.Handle))
						{
							_PropertyFound = true;
							_PropertyName = enumWindowsItem.Text;
							PropertyWatcher.Interval = 10;
							return;
						}
					}

					if (safetycount == 0)
					{
						PropertyWatcher.Enabled = false;
						_PropertyFound = false;
						DoFire(1);
						OnPropertyPageExitEventHandler onPropertyPageExitEventHandler = onPropertyPageExitEvent;
						if (onPropertyPageExitEventHandler != null)
							onPropertyPageExitEventHandler();
						Thread.Sleep(100);
						DoFire(1);
						Native.SetFocusThread(Parent.Handle);
						Native.SendMessage((IntPtr)hwnd, 7u, (IntPtr)(-1L), (IntPtr)0);
						EventsPanel.Focus();
						return;
					}
				}
				else
				{
					if (Native.FindWindow(_PropertyClass, _PropertyName) == IntPtr.Zero)
					{
						PropertyWatcher.Enabled = false;
						_PropertyFound = false;
						DoFire(1);
						OnPropertyPageExitEventHandler onPropertyPageExitEventHandler = onPropertyPageExitEvent;
						if (onPropertyPageExitEventHandler != null)
							onPropertyPageExitEventHandler();
						Thread.Sleep(100);
						DoFire(1);
						Native.SetFocusThread(Parent.Handle);
						Native.SendMessage((IntPtr)hwnd, 7u, (IntPtr)(-1L), (IntPtr)0);
						EventsPanel.Focus();
					}
				}
			}
		}

		private long FindWindowLike(ref string WindowText, ref string Classname)
		{
			for (long num = Native.GetWindow(Native.GetDesktopWindow(), 5); num != 0L; num = (long)Native.GetWindow(checked((int)num), 2))
			{
				string text = Strings.Space(255);
				long num2 = Native.GetWindowText(checked((int)num), ref text, 255);
				text = text.Substring(0, checked((int)num2));
				string text2 = Strings.Space(255);
				num2 = Native.GetClassName(checked((int)num), ref text2, 255);
				text2 = text2.Substring(0, checked((int)num2));
				if (LikeOperator.LikeString(text, WindowText, CompareMethod.Binary) &
					(LikeOperator.LikeString(text2, Classname, CompareMethod.Binary) | Operators.CompareString(text2, Classname, false) == 0))
				{
					WindowText = text;
					Classname = text2;
					return num;
				}
			}
			return 0;
		}

		public void SendEsc()
		{
			Native.SendMessage((IntPtr)hwnd, 256u, (IntPtr)27, (IntPtr)1);
		}

		private void TransparentPanel1_onClipInsert()
		{
			InsertClipboard();
		}

		public void InsertClipboard()
		{
			string text = Conversions.ToString(Clipboard.GetData(DataFormats.Text));
			if (Operators.CompareString(text, "", false) != 0)
			{
				text = text.Trim().Replace("\t", "    ").Replace("\r", "").Replace("\n", "");
				Native.SendText(text);
			}
		}

		private void ContextMenuStrip1_Opening(object sender, CancelEventArgs e)
		{
			PasteToolStripMenuItem.Enabled = Clipboard.ContainsText();
		}

		private void SelectToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OneQuickEdit();
		}

		private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (Clipboard.ContainsText())
				SendTextToConsole(Clipboard.GetText(), false);
		}

		private void PropertiesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ShowProperties();
		}

		public void ScrollUp()
		{
			Native.SendMessage((IntPtr)hwnd, 277u, (IntPtr)2, IntPtr.Zero);
		}

		public void ScrollDown()
		{
			Native.SendMessage((IntPtr)hwnd, 277u, (IntPtr)3, (IntPtr)0);
		}

		[DllImport("user32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

		public void SendCtrlC()
		{
			Native.PostMessage((IntPtr)hwnd, 256, (IntPtr)67, (IntPtr)3014657);
		}

		internal void EnableUpdateTimer(bool mode)
		{
			Timer1.Enabled = mode;
		}

		private void Timer1_Tick(object sender, EventArgs e)
		{
			if (MouseButtons != MouseButtons.Left)
				CheckForUpdate();
		}

		private void p1_onClick()
		{
			OnClickEventHandler onClickEventHandler = _onClickEvent;
			if (onClickEventHandler != null)
				onClickEventHandler();
		}

		private void p1_PageUp()
		{
			ScrollUp();
		}

		private void p1_PageDown()
		{
			ScrollDown();
		}

		private void p1_onClipCopy()
		{
			OnClipCopyEventHandler onClipCopyEventHandler = _onClipCopyEvent;
			if (onClipCopyEventHandler != null)
				onClipCopyEventHandler();
		}

		private void p1_onESC()
		{
			OnEscEventHandler onESCEventHandler = _onEscEvent;
			if (onESCEventHandler != null)
				onESCEventHandler();
		}

		private void ButtonTrans_Click(object sender, EventArgs e)
		{
			OnKeepFocusEventHandler onKeepFocusEventHandler = _onKeepFocusEvent;
			if (onKeepFocusEventHandler != null)
				onKeepFocusEventHandler(true);
			ToggleTopmost();
			EventsPanel.Focus();
			onKeepFocusEventHandler = _onKeepFocusEvent;
			if (onKeepFocusEventHandler != null)
				onKeepFocusEventHandler(false);
		}

		public void SwitchTopmost(bool mode)
		{
			if (mode)
				ParentForm.TopMost = true;
			else
				ParentForm.TopMost = false;
		}

		internal void SetTopMostButton(bool mode)
		{
		}

		private void ToggleTopmost()
		{
			if (ParentForm.TopMost)
				SwitchTopmost(false);
			else
				SwitchTopmost(true);
		}

		private void p1_onSendChar(string text)
		{
			if (Operators.CompareString(text, "^", false) == 0 | Operators.CompareString(text, "`", false) == 0)
				SendTextToConsole(text, false);
		}

		private void p1_onPrintScreen()
		{
			OnPrintScreenEventHandler onPrintScreenEventHandler = onPrintScreenEvent;
			if (onPrintScreenEventHandler != null)
				onPrintScreenEventHandler();
		}

		private void p1_onCtrlC(ref bool cancel)
		{
			if (!isSelection())
			{
				OnCtrlCEventHandler onCtrlCEventHandler = _onCtrlCEvent;
				if (onCtrlCEventHandler != null)
					onCtrlCEventHandler(ref cancel);
			}
		}

		internal void SetConsoleFocus()
		{
			Native.SetFocusThread(Parent.Handle);
			Native.SendMessage((IntPtr)hwnd, 7u, (IntPtr)(-1L), (IntPtr)0);
			EventsPanel.Focus();
		}

		public bool isSelection()
		{
			try
			{
				Native.CONSOLE_SELECTION_INFO cONSOLE_SELECTION_INFO = default(Native.CONSOLE_SELECTION_INFO);
				if (Native.GetConsoleSelectionInfo(ref cONSOLE_SELECTION_INFO) && cONSOLE_SELECTION_INFO.dwFlags == Native.SelectionFlags.NoSelection)
					return false;
			}
			catch (Exception expr_21)
			{
				ProjectData.SetProjectError(expr_21);
				Exception ex = expr_21;

				ProjectData.ClearProjectError();
			}
			return true;
		}

		private void p1_onAltCapture(ref bool handled)
		{
			OnAltCaptureEventHandler onAltCaptureEventHandler = _onAltCaptureEvent;
			if (onAltCaptureEventHandler != null)
				onAltCaptureEventHandler(ref handled);
		}

		public Size ConsoleSize()
		{
			return ContainerPanel.Size;
		}

		public void FocusPanel()
		{
			EventsPanel.Focus();
		}

		private void p1_onCtrlAlt()
		{
			OnCtrlAltEventHandler onCtrlAltEventHandler = _onCtrlAltEvent;
			if (onCtrlAltEventHandler != null)
				onCtrlAltEventHandler();
		}

		private void p1_onShiftAlt()
		{
			OnShiftAltEventHandler onShiftAltEventHandler = onShiftAltEvent;
			if (onShiftAltEventHandler != null)
				onShiftAltEventHandler();
		}

		private void p1_onSpaceAlt()
		{
			OnSpaceAltEventHandler onSpaceAltEventHandler = _onSpaceAltEvent;
			if (onSpaceAltEventHandler != null)
				onSpaceAltEventHandler();
		}

		internal Bitmap CreateHardcopy()
		{
			Bitmap bitmap = Helper.GetBitmap(EventsPanel);
			return Helper.GetBitmap(EventsPanel);
		}

		internal void CoverShow()
		{
		}

		internal void CoverHide()
		{
		}

		private void BeginUpdate()
		{
			Native.SendMessage(Handle, 11u, (IntPtr)0, (IntPtr)0);
		}

		private void EndUpdate()
		{
			Native.SendMessage(Handle, 11u, (IntPtr)(-1), (IntPtr)0);
			Invalidate(true);
		}

		internal void UpdateColorTable()
		{
			GetConsoleColorIndex(ref _consoleFront, ref _consoleBack);
			_consoleTable = Helper.GetConsoleColorTable();
			_consoleColor = new Color[16];
			int num = 0;
			checked
			{
				do
				{
					_consoleColor[num] = ColorTranslator.FromWin32((int)_consoleTable[num]);
					num++;
				} while (num <= 15);
			}
		}

		private void AdjustWidthToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Size bufferSize = BufferSize;
			bufferSize.Width = WindowSize.Width;
			BufferSize = bufferSize;
			UpdateSize(false);
			AdjustBlank();
			Form parentForm = ParentForm;
			checked
			{
				parentForm.Width++;
				parentForm = ParentForm;
				parentForm.Width--;
			}
		}

		private void MinimodeToolStripMenuItem1_Click(object sender, EventArgs e)
		{
		}

		private void Timer2_Tick(object sender, EventArgs e)
		{
			Timer2.Enabled = false;
			lblConsoleSize.Visible = false;
		}

		private void p1_LostFocus(object sender, EventArgs e)
		{
			HideCursor();
		}

		#region Nested type: EventFunc
		internal delegate int EventFunc(int HookHandle, int LEvent, int hwnd, int idObject, int idChild, int idEventThread, int dwmsEventTime);
		#endregion

		#region Nested type: FireEvent
		private delegate void FireEvent(int which);
		#endregion

		#region Nested type: xyCOORD
		[StructLayout(LayoutKind.Explicit)]
		internal struct xyCOORD
		{
			[FieldOffset(0)]
			internal short x;

			[FieldOffset(2)]
			internal short y;

			[FieldOffset(0)]
			internal int all;
		}
		#endregion
	}

	public class EnumWindowsItem
	{
		private IntPtr hWnd;

		public EnumWindowsItem(IntPtr hWnd)
		{
			this.hWnd = IntPtr.Zero;
			this.hWnd = hWnd;
		}

		public IntPtr Handle
		{
			get
			{
				return hWnd;
			}
		}

		public string Text
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder(260, 260);
				UnManagedMethods.GetWindowText(hWnd, stringBuilder, stringBuilder.Capacity);
				return stringBuilder.ToString();
			}
		}

		public string ClassName
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder(260, 260);
				UnManagedMethods.GetClassName(hWnd, stringBuilder, stringBuilder.Capacity);
				return stringBuilder.ToString();
			}
		}

		public bool Iconic
		{
			get
			{
				return Conversions.ToBoolean(Interaction.IIf(Operators.ConditionalCompareObjectEqual(UnManagedMethods.IsIconic(hWnd), 0, false), false, true));
			}
			set
			{
				IntPtr arg_1D_0 = hWnd;
				int arg_1D_1 = 274;
				IntPtr wParam = new IntPtr(61472);
				UnManagedMethods.SendMessage(arg_1D_0, arg_1D_1, wParam, IntPtr.Zero);
			}
		}

		public bool Maximised
		{
			get
			{
				return Conversions.ToBoolean(Interaction.IIf(UnManagedMethods.IsZoomed(hWnd) == 0, false, true));
			}
			set
			{
				IntPtr arg_1D_0 = hWnd;
				int arg_1D_1 = 274;
				IntPtr wParam = new IntPtr(61488);
				UnManagedMethods.SendMessage(arg_1D_0, arg_1D_1, wParam, IntPtr.Zero);
			}
		}

		public bool Visible
		{
			get
			{
				return Conversions.ToBoolean(Interaction.IIf(UnManagedMethods.IsWindowVisible(hWnd) == 0, false, true));
			}
		}

		public Rectangle Rectangle
		{
			get
			{
				RECT rECT = default(RECT);
				UnManagedMethods.GetWindowRect(hWnd, ref rECT);
				Rectangle result = checked(new Rectangle(rECT.Left, rECT.Top, rECT.Right - rECT.Left, rECT.Bottom - rECT.Top));
				return result;
			}
		}

		public Point Location
		{
			get
			{
				Rectangle rectangle = Rectangle;
				Point result = new Point(rectangle.Left, rectangle.Top);
				return result;
			}
		}

		public Size Size
		{
			get
			{
				Rectangle rectangle = Rectangle;
				Size result = checked(new Size(rectangle.Right - rectangle.Left, rectangle.Bottom - rectangle.Top));
				return result;
			}
		}

		public WindowStyleFlags WindowStyle
		{
			get
			{
				return (WindowStyleFlags)UnManagedMethods.GetWindowLong(hWnd, -16);
			}
		}

		public ExtendedWindowStyleFlags ExtendedWindowStyle
		{
			get
			{
				return (ExtendedWindowStyleFlags)UnManagedMethods.GetWindowLong(hWnd, -20);
			}
		}

		public override int GetHashCode()
		{
			return hWnd.ToInt32();
		}

		public void Restore()
		{
			if (Iconic)
			{
				IntPtr arg_25_0 = hWnd;
				int arg_25_1 = 274;
				IntPtr wParam = new IntPtr(61728);
				UnManagedMethods.SendMessage(arg_25_0, arg_25_1, wParam, IntPtr.Zero);
			}
			UnManagedMethods.BringWindowToTop(hWnd);
			UnManagedMethods.SetForegroundWindow(hWnd);
		}

		#region Nested type: FLASHWINFO
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		private struct FLASHWINFO
		{
			public int cbSize;
			public IntPtr hwnd;
			public int dwFlags;
			public int uCount;
			public int dwTimeout;
		}
		#endregion

		#region Nested type: RECT
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		private struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}
		#endregion

		#region Nested type: UnManagedMethods
		private class UnManagedMethods
		{
			public const int WM_COMMAND = 273;
			public const int WM_SYSCOMMAND = 274;
			public const int SC_RESTORE = 61728;
			public const int SC_CLOSE = 61536;
			public const int SC_MAXIMIZE = 61488;
			public const int SC_MINIMIZE = 61472;
			public const int GWL_STYLE = -16;
			public const int GWL_EXSTYLE = -20;
			public const int FLASHW_STOP = 0;
			public const int FLASHW_CAPTION = 1;
			public const int FLASHW_TRAY = 2;
			public const int FLASHW_ALL = 3;
			public const int FLASHW_TIMER = 4;
			public const int FLASHW_TIMERNOFG = 12;

			[DllImport("user32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
			public static extern int IsWindowVisible(IntPtr hWnd);

			[DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
			public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int cch);

			[DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
			public static extern int GetWindowTextLength(IntPtr hWnd);

			[DllImport("user32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
			public static extern int BringWindowToTop(IntPtr hWnd);

			[DllImport("user32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
			public static extern int SetForegroundWindow(IntPtr hWnd);

			[DllImport("user32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
			public static extern object IsIconic(IntPtr hWnd);

			[DllImport("user32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
			public static extern int IsZoomed(IntPtr hwnd);

			[DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
			public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

			[DllImport("user32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
			public static extern int FlashWindow(IntPtr hWnd, ref FLASHWINFO pwfi);

			[DllImport("user32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
			public static extern int GetWindowRect(IntPtr hWnd, ref RECT lpRect);

			[DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
			public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

			[DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
			public static extern int GetWindowLong(IntPtr hwnd, int nIndex);
		}
		#endregion
	}

	[Flags]
	public enum ExtendedWindowStyleFlags
	{
		WS_EX_DLGMODALFRAME = 1,
		WS_EX_NOPARENTNOTIFY = 4,
		WS_EX_TOPMOST = 8,
		WS_EX_ACCEPTFILES = 16,
		WS_EX_TRANSPARENT = 32,
		WS_EX_MDICHILD = 64,
		WS_EX_TOOLWINDOW = 128,
		WS_EX_WINDOWEDGE = 256,
		WS_EX_CLIENTEDGE = 512,
		WS_EX_CONTEXTHELP = 1024,
		WS_EX_RIGHT = 4096,
		WS_EX_LEFT = 0,
		WS_EX_RTLREADING = 8192,
		WS_EX_LTRREADING = 0,
		WS_EX_LEFTSCROLLBAR = 16384,
		WS_EX_RIGHTSCROLLBAR = 0,
		WS_EX_CONTROLPARENT = 65536,
		WS_EX_STATICEDGE = 131072,
		WS_EX_APPWINDOW = 262144,
		WS_EX_LAYERED = 524288,
		WS_EX_NOINHERITLAYOUT = 1048576,
		WS_EX_LAYOUTRTL = 4194304,
		WS_EX_COMPOSITED = 33554432,
		WS_EX_NOACTIVATE = 134217728
	}

	public enum WindowStyleFlags
	{
		WS_OVERLAPPED,
		WS_POPUP = -2147483648,
		WS_CHILD = 1073741824,
		WS_MINIMIZE = 536870912,
		WS_VISIBLE = 268435456,
		WS_DISABLED = 134217728,
		WS_CLIPSIBLINGS = 67108864,
		WS_CLIPCHILDREN = 33554432,
		WS_MAXIMIZE = 16777216,
		WS_BORDER = 8388608,
		WS_DLGFRAME = 4194304,
		WS_VSCROLL = 2097152,
		WS_HSCROLL = 1048576,
		WS_SYSMENU = 524288,
		WS_THICKFRAME = 262144,
		WS_GROUP = 131072,
		WS_TABSTOP = 65536,
		WS_MINIMIZEBOX = 131072,
		WS_MAXIMIZEBOX = 65536
	}

	public class EnumWindowsCollection : ReadOnlyCollectionBase
	{
		public EnumWindowsItem this[int index]
		{
			get
			{
				return (EnumWindowsItem)InnerList[index];
			}
		}

		public void Add(IntPtr hWnd)
		{
			EnumWindowsItem value = new EnumWindowsItem(hWnd);
			base.InnerList.Add(value);
		}
	}

	public class EnumWindows
	{
		private EnumWindowsCollection m_items;

		public EnumWindows()
		{
			m_items = null;
		}

		public EnumWindowsCollection Items
		{
			get
			{
				return m_items;
			}
		}

		public void GetWindows()
		{
			m_items = new EnumWindowsCollection();
			UnManagedMethods.EnumWindows(WindowEnum, 0);
		}

		public void GetWindows(IntPtr hWndParent)
		{
			m_items = new EnumWindowsCollection();
			UnManagedMethods.EnumChildWindows(hWndParent, WindowEnum, 0);
		}

		private int WindowEnum(IntPtr hWnd, int lParam)
		{
			if (OnWindowEnum(hWnd))
				return 1;
			return 0;
		}

		protected virtual bool OnWindowEnum(IntPtr hWnd)
		{
			m_items.Add(hWnd);
			return true;
		}

		#region Nested type: EnumWindowsProc
		private delegate int EnumWindowsProc(IntPtr hwnd, int lParam);
		#endregion

		#region Nested type: UnManagedMethods
		private class UnManagedMethods
		{
			[DllImport("user32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
			public static extern int EnumWindows(EnumWindowsProc lpEnumFunc, int lParam);

			[DllImport("user32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
			public static extern int EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, int lParam);
		}
		#endregion
	}

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

		[Flags]
		internal enum ConsoleModes : uint
		{
			AutoPosition = 256u,
			EchoInput = 4u,
			Extended = 128u,
			Insert = 32u,
			LineInput = 2u,
			MouseInput = 16u,
			ProcessedInput = 1u,
			ProcessedOutput = 1u,
			QuickEdit = 64u,
			WindowInput = 8u,
			WrapEndOfLine = 2u,
			isNull = 0u
		}
	}

	internal class SpecComponent : UserControl
	{
		private IContainer components;
		[AccessedThroughProperty("VScrollBar1")]
		private VScrollBar _VScrollBar1;
		[AccessedThroughProperty("HScrollBar1")]
		private HScrollBar _HScrollBar1;
		[AccessedThroughProperty("Timer1")]
		private Timer _Timer1;
		private SpecComponent.onCtrlAltEventHandler onCtrlAltEvent;
		private SpecComponent.onShiftAltEventHandler onShiftAltEvent;
		private onShiftDownEventHandler onShiftDownEvent;
		private onShiftUpEventHandler onShiftUpEvent;
		private SpecComponent.onSpaceAltEventHandler onSpaceAltEvent;
		private SpecComponent.onAltCaptureEventHandler onAltCaptureEvent;
		private SpecComponent.onCtrlCEventHandler onCtrlCEvent;
		private SpecComponent.onPrintScreenEventHandler onPrintScreenEvent;
		private SpecComponent.onTriggerEventHandler onTriggerEvent;
		private SpecComponent.onFunctionKeyEventHandler onFunctionKeyEvent;
		private SpecComponent.onCursorKeyEventHandler onCursorKeyEvent;
		private SpecComponent.onMouseUpEventHandler onMouseUpEvent;
		private SpecComponent.onAltDownEventHandler onAltDownEvent;
		private SpecComponent.onAltUpEventHandler onAltUpEvent;
		private SpecComponent.onClipInsertEventHandler onClipInsertEvent;
		private SpecComponent.onALTENTEREventHandler onALTENTEREvent;
		private SpecComponent.onEnterEventHandler onEnterEvent;
		private SpecComponent.onKeyEventHandler onKeyEvent;
		private SpecComponent.onClickEventHandler onClickEvent;
		private SpecComponent.PageUpEventHandler PageUpEvent;
		private SpecComponent.PageDownEventHandler PageDownEvent;
		private SpecComponent.onClipCopyEventHandler onClipCopyEvent;
		private SpecComponent.onESCEventHandler onESCEvent;
		private SpecComponent.onFocusEventHandler onFocusEvent;
		private SpecComponent.onSendCharEventHandler onSendCharEvent;
		private const int WM_LBUTTONDOWN = 513;
		private const int WM_KEYDOWN = 256;
		private const int WM_KEYUP = 257;
		private const int WM_SYSKEYDOWN = 260;
		private const int WM_MOUSEMOVE = 512;
		private const int WM_LBUTTONUP = 514;
		private const int WM_RBUTTONDOWN = 516;
		private const int WM_RBUTTONUP = 517;
		private const int WM_SYSKEYUP = 261;
		private bool _AltDown;
		private bool _PGUPDOWN;
		private Dictionary<string, bool> _TriggerKeys;
		private bool _isNativeApp;
		private bool _isListening;
		private bool _isListeningTrigger;
		private string _currentInput;
		public IntPtr hwnd;
		public bool _newfocus;

		internal virtual VScrollBar VScrollBar1
		{
			get
			{
				return this._VScrollBar1;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				this._VScrollBar1 = value;
			}
		}

		internal virtual HScrollBar HScrollBar1
		{
			get
			{
				return this._HScrollBar1;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				this._HScrollBar1 = value;
			}
		}

		internal virtual Timer Timer1
		{
			get
			{
				return this._Timer1;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.Timer1_Tick);
				if (this._Timer1 != null)
					this._Timer1.Tick -= eventHandler;
				this._Timer1 = value;
				if (this._Timer1 == null)
					return;
				this._Timer1.Tick += eventHandler;
			}
		}

		public bool scrollH
		{
			get
			{
				return this.HScrollBar1.Visible;
			}
			set
			{
				if (this.HScrollBar1.Visible == value)
					return;
				this.HScrollBar1.Visible = value;
			}
		}

		public bool scrollV
		{
			get
			{
				return this.VScrollBar1.Visible;
			}
			set
			{
				if (this.VScrollBar1.Visible == value)
					return;
				this.VScrollBar1.Visible = value;
			}
		}

		public string CurrentInput
		{
			get
			{
				return this._currentInput;
			}
			set
			{
				this._currentInput = value;
			}
		}

		public bool isNativeApp
		{
			get
			{
				return this._isNativeApp;
			}
			set
			{
				this._isNativeApp = value;
			}
		}

		public bool ListenTrigger
		{
			get
			{
				return this._isListeningTrigger;
			}
			set
			{
				this._isListeningTrigger = value;
			}
		}

		public bool Listen
		{
			get
			{
				return this._isListening;
			}
			set
			{
				this._isListening = value;
			}
		}

		public bool PGUPDOWNScrollsConsole
		{
			get
			{
				return this._PGUPDOWN;
			}
			set
			{
				this._PGUPDOWN = value;
			}
		}

		public bool NewFocus
		{
			get
			{
				return this._newfocus;
			}
			set
			{
				if (value)
					this._newfocus = value;
				else
					this.Timer1.Enabled = true;
			}
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.ExStyle = createParams.ExStyle | 32;
				return createParams;
			}
		}

		public event SpecComponent.onCtrlAltEventHandler onCtrlAlt
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.onCtrlAltEvent = this.onCtrlAltEvent + value;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.onCtrlAltEvent = this.onCtrlAltEvent - value;
			}
		}

		public event SpecComponent.onShiftDownEventHandler onShiftDown
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.onShiftDownEvent = this.onShiftDownEvent + value;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.onShiftDownEvent = this.onShiftDownEvent - value;
			}
		}

		public event SpecComponent.onShiftUpEventHandler onShiftUp
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.onShiftUpEvent = this.onShiftUpEvent + value;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.onShiftUpEvent = this.onShiftUpEvent - value;
			}
		}

		public event SpecComponent.onShiftAltEventHandler onShiftAlt
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.onShiftAltEvent = this.onShiftAltEvent + value;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.onShiftAltEvent = this.onShiftAltEvent - value;
			}
		}

		public event SpecComponent.onSpaceAltEventHandler onSpaceAlt
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.onSpaceAltEvent = this.onSpaceAltEvent + value;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.onSpaceAltEvent = this.onSpaceAltEvent - value;
			}
		}

		public event SpecComponent.onAltCaptureEventHandler onAltCapture
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.onAltCaptureEvent = this.onAltCaptureEvent + value;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.onAltCaptureEvent = this.onAltCaptureEvent - value;
			}
		}

		public event SpecComponent.onCtrlCEventHandler onCtrlC
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.onCtrlCEvent = this.onCtrlCEvent + value;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.onCtrlCEvent = this.onCtrlCEvent - value;
			}
		}

		public event SpecComponent.onPrintScreenEventHandler onPrintScreen
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.onPrintScreenEvent = this.onPrintScreenEvent + value;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.onPrintScreenEvent = this.onPrintScreenEvent - value;
			}
		}

		public event SpecComponent.onTriggerEventHandler onTrigger
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.onTriggerEvent = this.onTriggerEvent + value;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.onTriggerEvent = this.onTriggerEvent - value;
			}
		}

		public event SpecComponent.onFunctionKeyEventHandler onFunctionKey
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.onFunctionKeyEvent = this.onFunctionKeyEvent + value;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.onFunctionKeyEvent = this.onFunctionKeyEvent - value;
			}
		}

		public event SpecComponent.onCursorKeyEventHandler onCursorKey
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.onCursorKeyEvent = this.onCursorKeyEvent + value;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.onCursorKeyEvent = this.onCursorKeyEvent - value;
			}
		}

		public event SpecComponent.onMouseUpEventHandler onMouseUp
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.onMouseUpEvent = this.onMouseUpEvent + value;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.onMouseUpEvent = this.onMouseUpEvent - value;
			}
		}

		public event SpecComponent.onAltDownEventHandler onAltDown
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.onAltDownEvent = this.onAltDownEvent + value;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.onAltDownEvent = this.onAltDownEvent - value;
			}
		}

		public event SpecComponent.onAltUpEventHandler onAltUp
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.onAltUpEvent = this.onAltUpEvent + value;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.onAltUpEvent = this.onAltUpEvent - value;
			}
		}

		public event SpecComponent.onClipInsertEventHandler onClipInsert
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.onClipInsertEvent = this.onClipInsertEvent + value;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.onClipInsertEvent = this.onClipInsertEvent - value;
			}
		}

		public event SpecComponent.onALTENTEREventHandler onALTENTER
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.onALTENTEREvent = this.onALTENTEREvent + value;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.onALTENTEREvent = this.onALTENTEREvent - value;
			}
		}

		public event SpecComponent.onEnterEventHandler onEnter
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.onEnterEvent = this.onEnterEvent + value;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.onEnterEvent = this.onEnterEvent - value;
			}
		}

		public event SpecComponent.onKeyEventHandler onKey
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.onKeyEvent = this.onKeyEvent + value;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.onKeyEvent = this.onKeyEvent - value;
			}
		}

		public event SpecComponent.onClickEventHandler onClick
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.onClickEvent = this.onClickEvent + value;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.onClickEvent = this.onClickEvent - value;
			}
		}

		public event SpecComponent.PageUpEventHandler PageUp
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.PageUpEvent = this.PageUpEvent + value;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.PageUpEvent = this.PageUpEvent - value;
			}
		}

		public event SpecComponent.PageDownEventHandler PageDown
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.PageDownEvent = this.PageDownEvent + value;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.PageDownEvent = this.PageDownEvent - value;
			}
		}

		public event SpecComponent.onClipCopyEventHandler onClipCopy
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.onClipCopyEvent = this.onClipCopyEvent + value;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.onClipCopyEvent = this.onClipCopyEvent - value;
			}
		}

		public event SpecComponent.onESCEventHandler onESC
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.onESCEvent = this.onESCEvent + value;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.onESCEvent = this.onESCEvent - value;
			}
		}

		public event SpecComponent.onFocusEventHandler onFocus
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.onFocusEvent = this.onFocusEvent + value;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.onFocusEvent = this.onFocusEvent - value;
			}
		}

		public event SpecComponent.onSendCharEventHandler onSendChar
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this.onSendCharEvent = this.onSendCharEvent + value;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this.onSendCharEvent = this.onSendCharEvent - value;
			}
		}

		public SpecComponent()
		{
			this.GotFocus += new EventHandler(this.TransparentPanel_GotFocus);
			this.MouseUp += new MouseEventHandler(this.TransparentPanel_MouseUp);
			this.Resize += new EventHandler(this.TransparentPanel_Resize);
			this._AltDown = false;
			this._PGUPDOWN = true;
			this._TriggerKeys = new Dictionary<string, bool>();
			this._isNativeApp = false;
			this._isListening = true;
			this._isListeningTrigger = true;
			this._currentInput = "";
			this._newfocus = false;
			this.InitializeComponent();
		}

		[DebuggerNonUserCode]
		protected override void Dispose(bool disposing)
		{
			try
			{
				if (!disposing || this.components == null)
					return;
				this.components.Dispose();
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		[DebuggerStepThrough]
		private void InitializeComponent()
		{
			this.components = (IContainer)new System.ComponentModel.Container();
			this.VScrollBar1 = new VScrollBar();
			this.HScrollBar1 = new HScrollBar();
			this.Timer1 = new Timer(this.components);
			this.SuspendLayout();
			VScrollBar vscrollBar1_1 = this.VScrollBar1;
			Point point1 = new Point(320, 0);
			Point point2 = point1;
			vscrollBar1_1.Location = point2;
			this.VScrollBar1.Name = "VScrollBar1";
			VScrollBar vscrollBar1_2 = this.VScrollBar1;
			Size size1 = new Size(17, 309);
			Size size2 = size1;
			vscrollBar1_2.Size = size2;
			this.VScrollBar1.TabIndex = 3;
			HScrollBar hscrollBar1_1 = this.HScrollBar1;
			point1 = new Point(0, 309);
			Point point3 = point1;
			hscrollBar1_1.Location = point3;
			this.HScrollBar1.Name = "HScrollBar1";
			HScrollBar hscrollBar1_2 = this.HScrollBar1;
			size1 = new Size(337, 17);
			Size size3 = size1;
			hscrollBar1_2.Size = size3;
			this.HScrollBar1.TabIndex = 2;
			this.Timer1.Interval = 50;
			this.AutoScaleDimensions = new SizeF(6f, 13f);
			this.AutoScaleMode = AutoScaleMode.Font;
			this.Controls.Add((Control)this.VScrollBar1);
			this.Controls.Add((Control)this.HScrollBar1);
			this.Name = "TransparentPanel";
			SpecComponent specComponent = this;
			size1 = new Size(337, 326);
			Size size4 = size1;
			specComponent.Size = size4;
			this.ResumeLayout(false);
		}

		[DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern int GetForegroundWindow();

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true)]
		private static extern int ToAscii(int uVirtKey, int uScanCode, byte[] lpbKeyState, ref int lpwTransKey, int fuState);

		[DllImport("user32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		private static extern bool GetKeyboardState(byte[] pbKeyState);

		[DllImport("User32", CharSet = CharSet.Ansi, SetLastError = true)]
		private static extern void ReleaseCapture();

		[DllImport("user32.dll")]
		internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		public void SetHScrollbar(int max, int value)
		{
			HScrollBar hscrollBar1 = this.HScrollBar1;
			if (hscrollBar1.Maximum != checked(max + 8))
				hscrollBar1.Maximum = checked(max + 8);
			hscrollBar1.Value = value;
		}

		public void SetVScrollbar(int max, int value)
		{
			VScrollBar vscrollBar1 = this.VScrollBar1;
			if (vscrollBar1.Maximum != checked(max + 8))
				vscrollBar1.Maximum = checked(max + 8);
			vscrollBar1.Value = value;
		}

		public void AddTrigger(string key)
		{
			if (this._TriggerKeys.ContainsKey(key))
				return;
			this._TriggerKeys.Add(key, true);
		}

		public void RemoveTrigger(string key)
		{
			if (!this._TriggerKeys.ContainsKey(key))
				return;
			this._TriggerKeys.Remove(key);
		}

		private string GetCharFromKey(int KeyCode)
		{
			byte[] numArray = new byte[256];
			int lpwTransKey = 0;
			if (SpecComponent.GetKeyboardState(numArray) && SpecComponent.ToAscii(KeyCode, 0, numArray, ref lpwTransKey, 0) != 0)
				return Conversions.ToString(Strings.ChrW(lpwTransKey));
			else
				return "VK_" + Conversions.ToString(KeyCode);
		}

		protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData)
		{
			string Left1 = "";
			bool cancel1 = false;
			string charFromKey = this.GetCharFromKey((int)keyData);
			bool flag1 = false;
			if (keyData == (Keys.Return | Keys.Alt))
			{
				SpecComponent.onALTENTEREventHandler altenterEventHandler = this.onALTENTEREvent;
				if (altenterEventHandler != null)
					altenterEventHandler();
				return true;
			}
			else
			{
				if (keyData == Keys.Return)
				{
					SpecComponent.onEnterEventHandler enterEventHandler = this.onEnterEvent;
					if (enterEventHandler != null)
						enterEventHandler();
				}
				if (!this._isListening)
					return base.ProcessCmdKey(ref msg, keyData);
				switch (keyData)
				{
					case Keys.V | Keys.Control:
						SpecComponent.onClipInsertEventHandler insertEventHandler = this.onClipInsertEvent;
						if (insertEventHandler != null)
							insertEventHandler();
						return true;
					case Keys.ControlKey | Keys.Control:
						return true;
					case Keys.N | Keys.Alt:
						SpecComponent.onClipCopyEventHandler copyEventHandler1 = this.onClipCopyEvent;
						if (copyEventHandler1 != null)
							copyEventHandler1();
						return true;
					case Keys.C | Keys.Control:
						object obj = (object)false;
						SpecComponent.onCtrlCEventHandler ctrlCeventHandler1 = this.onCtrlCEvent;
						if (ctrlCeventHandler1 != null)
						{
							SpecComponent.onCtrlCEventHandler ctrlCeventHandler2 = ctrlCeventHandler1;
							bool flag2 = Conversions.ToBoolean(obj);
							ctrlCeventHandler2(ref flag2);
							obj = (object)flag2;
						}
						if (Conversions.ToBoolean(obj))
						{
							SpecComponent.onClipCopyEventHandler copyEventHandler2 = this.onClipCopyEvent;
							if (copyEventHandler2 != null)
								copyEventHandler2();
							return true;
						}
						else
							break;
				}
				if (msg.Msg == 257)
					return true;
				if (msg.Msg != 259)
					;
				if (!(msg.Msg == 256 | msg.Msg == 260))
					return base.ProcessCmdKey(ref msg, keyData);
				string Left2 = charFromKey;
				if (Operators.CompareString(Left2, "`", false) == 0 || Operators.CompareString(Left2, "^", false) == 0 || Operators.CompareString(Left2, "´", false) == 0)
				{
					SpecComponent.onSendCharEventHandler charEventHandler = this.onSendCharEvent;
					if (charEventHandler != null)
						charEventHandler(charFromKey);
					return true;
				}
				else
				{
					if (Operators.CompareString(charFromKey, ":", false) == 0 & Operators.CompareString(Left1, ":", false) == 0)
						flag1 = true;
					if (!this._isNativeApp && (0 & (msg.WParam == (IntPtr)220 ? 1 : 0) | (msg.WParam == (IntPtr)221 ? 1 : 0)) != 0)
					{
						SpecComponent.PostMessage(this.hwnd, checked((uint)msg.Msg), msg.WParam, msg.LParam);
						SpecComponent.onSendCharEventHandler charEventHandler = this.onSendCharEvent;
						if (charEventHandler != null)
							charEventHandler(charFromKey);
						return true;
					}
					else
					{
						SpecComponent.onKeyEventHandler onKeyEventHandler = this.onKeyEvent;
						if (onKeyEventHandler != null)
							onKeyEventHandler(charFromKey);
						if (this._TriggerKeys.ContainsKey(charFromKey) | flag1)
						{
							if (!this._isListeningTrigger)
							{
								if (keyData == Keys.Tab & this._isNativeApp)
									SpecComponent.PostMessage(this.hwnd, checked((uint)msg.Msg), msg.WParam, msg.LParam);
								else if (keyData != Keys.Tab)
								{
									SpecComponent.PostMessage(this.hwnd, checked((uint)msg.Msg), msg.WParam, msg.LParam);
									return true;
								}
							}
							SpecComponent.onTriggerEventHandler triggerEventHandler = this.onTriggerEvent;
							if (triggerEventHandler != null)
								triggerEventHandler(charFromKey, ref cancel1);
							this._currentInput = "";
						}
						else
						{
							switch (keyData)
							{
								case Keys.Prior:
									if (this._PGUPDOWN & !this._isNativeApp)
									{
										SpecComponent.PageUpEventHandler pageUpEventHandler = this.PageUpEvent;
										if (pageUpEventHandler != null)
										{
											pageUpEventHandler();
											break;
										}
										else
											break;
									}
									else
									{
										SpecComponent.PostMessage(this.hwnd, checked((uint)msg.Msg), msg.WParam, msg.LParam);
										break;
									}
								case Keys.Next:
									if (this._PGUPDOWN & !this._isNativeApp)
									{
										SpecComponent.PageDownEventHandler downEventHandler = this.PageDownEvent;
										if (downEventHandler != null)
										{
											downEventHandler();
											break;
										}
										else
											break;
									}
									else
									{
										SpecComponent.PostMessage(this.hwnd, checked((uint)msg.Msg), msg.WParam, msg.LParam);
										break;
									}
								case Keys.Menu | Keys.Alt:
									if (this._isNativeApp)
									{
										SpecComponent.PostMessage(this.hwnd, checked((uint)msg.Msg), msg.WParam, msg.LParam);
										break;
									}
									else
									{
										SpecComponent.onAltDownEventHandler downEventHandler = this.onAltDownEvent;
										if (downEventHandler != null)
											downEventHandler();
										this._AltDown = true;
										bool handled = false;
										SpecComponent.onAltCaptureEventHandler captureEventHandler = this.onAltCaptureEvent;
										if (captureEventHandler != null)
											captureEventHandler(ref handled);
										this._AltDown = !handled;
										break;
									}
								case Keys.Menu | Keys.Control | Keys.Alt:
									SpecComponent.onCtrlAltEventHandler ctrlAltEventHandler = this.onCtrlAltEvent;
									if (ctrlAltEventHandler != null)
										ctrlAltEventHandler();
									this._AltDown = false;
									break;
								case Keys.Menu | Keys.Shift | Keys.Alt:
									SpecComponent.onShiftAltEventHandler shiftAltEventHandler = this.onShiftAltEvent;
									if (shiftAltEventHandler != null)
										shiftAltEventHandler();
									this._AltDown = false;
									break;
								case Keys.Shift | Keys.ShiftKey:
									SpecComponent.onShiftDownEventHandler shiftDownEventHandler = this.onShiftDownEvent;
									if (shiftDownEventHandler != null)
										shiftDownEventHandler();
									break;
								case Keys.Space | Keys.Alt:
									SpecComponent.onSpaceAltEventHandler spaceAltEventHandler = this.onSpaceAltEvent;
									if (spaceAltEventHandler != null)
										spaceAltEventHandler();
									this._AltDown = false;
									break;
								case Keys.F1:
								case Keys.F2:
								case Keys.F3:
								case Keys.F4:
								case Keys.F5:
								case Keys.F6:
								case Keys.F7:
								case Keys.F8:
								case Keys.F9:
								case Keys.F10:
								case Keys.F11:
								case Keys.F12:
									SpecComponent.onFunctionKeyEventHandler functionKeyEventHandler = this.onFunctionKeyEvent;
									if (functionKeyEventHandler != null)
										functionKeyEventHandler(keyData, ref cancel1);
									if (!cancel1 && this.hwnd != IntPtr.Zero)
									{
										SpecComponent.PostMessage(this.hwnd, checked((uint)msg.Msg), msg.WParam, msg.LParam);
										break;
									}
									else
										break;
								case Keys.Left:
								case Keys.Right:
								case Keys.Up:
								case Keys.Down:
									SpecComponent.onCursorKeyEventHandler cursorKeyEventHandler = this.onCursorKeyEvent;
									if (cursorKeyEventHandler != null)
										cursorKeyEventHandler(keyData, ref cancel1);
									if (!cancel1 && this.hwnd != IntPtr.Zero)
									{
										SpecComponent.PostMessage(this.hwnd, checked((uint)msg.Msg), msg.WParam, msg.LParam);
										break;
									}
									else
										break;
								default:
									if (this.hwnd != IntPtr.Zero)
									{
										SpecComponent.PostMessage(this.hwnd, checked((uint)msg.Msg), msg.WParam, msg.LParam);
										if (keyData == Keys.Tab)
											this._currentInput = "";
										if (!charFromKey.StartsWith("VK_") & Operators.CompareString(charFromKey, "\t", false) != 0)
										{
											this._currentInput = this._currentInput + charFromKey;
											break;
										}
										else
											break;
									}
									else
										break;
							}
						}
						return true;
					}
				}
			}
		}

		[DebuggerStepThrough]
		protected override void WndProc(ref System.Windows.Forms.Message m)
		{
			if (m.Msg != 259)
				;
			int msg = m.Msg;
			switch (msg)
			{
				case 261:
					if (m.WParam == (IntPtr)18 && this._AltDown & !this.isNativeApp)
					{
						SpecComponent.onAltUpEventHandler altUpEventHandler = this.onAltUpEvent;
						if (altUpEventHandler != null)
							altUpEventHandler();
					}
					this._AltDown = false;
					break;
				case 257:
					if (m.WParam == (IntPtr)44)
					{
						SpecComponent.onPrintScreenEventHandler screenEventHandler = this.onPrintScreenEvent;
						if (screenEventHandler != null)
							screenEventHandler();
					}
					else if (m.WParam == (IntPtr)16)
					{
						SpecComponent.onShiftUpEventHandler shiftUpEventHandler = this.onShiftUpEvent;
						if (shiftUpEventHandler != null)
							shiftUpEventHandler();
					}
					this._AltDown = false;
					SpecComponent.PostMessage(this.hwnd, checked((uint)m.Msg), m.WParam, m.LParam);
					break;
				default:
					if (msg == 513 || msg == 514)
					{
						bool flag = (uint)(Control.ModifierKeys & (Keys.KeyCode | Keys.Modifiers)) > 0U;
						if (this.hwnd != IntPtr.Zero & !flag & !this._newfocus)
							SpecComponent.SendMessage(this.hwnd, checked((uint)m.Msg), m.WParam, m.LParam);
						if (this._newfocus)
							this._newfocus = false;
						SpecComponent.onClickEventHandler clickEventHandler = this.onClickEvent;
						if (clickEventHandler != null)
						{
							clickEventHandler();
							break;
						}
						else
							break;
					}
					else if (msg == 512)
					{
						if (this.hwnd != IntPtr.Zero)
						{
							SpecComponent.SendMessage(this.hwnd, checked((uint)m.Msg), m.WParam, m.LParam);
							break;
						}
						else
							break;
					}
					else if (msg == 256 || msg == 260)
					{
						if (this.hwnd != IntPtr.Zero)
						{
							if (!this._isNativeApp)
							{
								if (!(m.WParam == (IntPtr)220))
									SpecComponent.SendMessage(this.hwnd, checked((uint)m.Msg), m.WParam, m.LParam);
							}
							else
								SpecComponent.SendMessage(this.hwnd, checked((uint)m.Msg), m.WParam, m.LParam);
						}
						if (this._newfocus)
						{
							this._newfocus = false;
							break;
						}
						else
							break;
					}
					else if (msg == 642 || msg == 81)
					{
						SpecComponent.SendMessage(this.hwnd, checked((uint)m.Msg), m.WParam, m.LParam);
						break;
					}
					else if (msg == 8)
					{
						SpecComponent.SendMessage(this.hwnd, 31U, (IntPtr)0, (IntPtr)0);
						break;
					}
					else if (msg != 160 && msg != 32 && (msg != 33 && msg != 132) && (msg != 161 && msg != 516 && (msg != 517 && msg != 123)) && msg != 162)
					{
						if (msg == 31)
						{
							SpecComponent.SendMessage(this.hwnd, checked((uint)m.Msg), m.WParam, m.LParam);
							break;
						}
						else if (msg == 641 || msg == 269 || (msg == 270 || msg == 271) || (msg == 641 || msg == 642 || (msg == 643 || msg == 644)) || (msg == 645 || msg == 646 || (msg == 656 || msg == 657)))
						{
							SpecComponent.SendMessage(this.hwnd, checked((uint)m.Msg), m.WParam, m.LParam);
							break;
						}
						else if (msg != 5 && msg != 7 && (msg != 15 && msg != 14) && (msg != 13 && msg != 70 && (msg != 131 && msg != 133)) && (msg != 71 && msg != 20 && (msg != 311 && msg != 129) && (msg != 1 && msg != 3 && (msg != 8720 && msg != 24))) && (msg != 528 && msg != 49581 && (msg != 675 && msg != 673)))
						{
							if (msg == 276 || msg == 277)
							{
								SpecComponent.SendMessage(this.hwnd, checked((uint)m.Msg), m.WParam, m.LParam);
								break;
							}
							else if (msg != 518)
							{
								SpecComponent.SendMessage(this.hwnd, checked((uint)m.Msg), m.WParam, m.LParam);
								break;
							}
							else
								break;
						}
						else
							break;
					}
					else
						break;
			}
			base.WndProc(ref m);
		}

		[DebuggerStepThrough]
		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
		}

		private void TransparentPanel_GotFocus(object sender, EventArgs e)
		{
			SpecComponent.onFocusEventHandler focusEventHandler = this.onFocusEvent;
			if (focusEventHandler == null)
				return;
			focusEventHandler();
		}

		private void TransparentPanel_MouseUp(object sender, MouseEventArgs e)
		{
		}

		private void TransparentPanel_Resize(object sender, EventArgs e)
		{
			this.Adjustbars();
		}

		public void Adjustbars()
		{
			HScrollBar hscrollBar1 = this.HScrollBar1;
			if (hscrollBar1.Left != this.Left)
				hscrollBar1.Left = this.Left;
			if (hscrollBar1.Top != checked(this.Bottom - hscrollBar1.Height))
				hscrollBar1.Top = checked(this.Bottom - hscrollBar1.Height);
			if (this.VScrollBar1.Visible & hscrollBar1.Visible)
			{
				if (hscrollBar1.Width != checked(this.Width - this.VScrollBar1.Width + 1))
					hscrollBar1.Width = checked(this.Width - this.VScrollBar1.Width + 1);
			}
			else if (hscrollBar1.Width != this.Width)
				hscrollBar1.Width = this.Width;
			VScrollBar vscrollBar1 = this.VScrollBar1;
			if (vscrollBar1.Left != checked(this.Right - vscrollBar1.Width))
				vscrollBar1.Left = checked(this.Right - vscrollBar1.Width);
			if (vscrollBar1.Top != this.Top)
				vscrollBar1.Top = this.Top;
			if (this.HScrollBar1.Visible & vscrollBar1.Visible)
			{
				if (vscrollBar1.Height != checked(this.Height - this.HScrollBar1.Height + 1))
					vscrollBar1.Height = checked(this.Height - this.HScrollBar1.Height + 1);
			}
			else if (vscrollBar1.Height != this.Height)
				vscrollBar1.Height = this.Height;
		}

		private void Timer1_Tick(object sender, EventArgs e)
		{
			this.Timer1.Enabled = false;
			this._newfocus = false;
		}

		public delegate void onCtrlAltEventHandler();

		public delegate void onShiftAltEventHandler();

		public delegate void onShiftDownEventHandler();
		public delegate void onShiftUpEventHandler();

		public delegate void onSpaceAltEventHandler();

		public delegate void onAltCaptureEventHandler(ref bool handled);

		public delegate void onCtrlCEventHandler(ref bool cancel);

		public delegate void onPrintScreenEventHandler();

		public delegate void onTriggerEventHandler(string keycode, ref bool cancel);

		public delegate void onFunctionKeyEventHandler(Keys keycode, ref bool cancel);

		public delegate void onCursorKeyEventHandler(Keys keycode, ref bool cancel);

		public delegate void onMouseUpEventHandler(MouseEventArgs e);

		public delegate void onAltDownEventHandler();

		public delegate void onAltUpEventHandler();

		public delegate void onClipInsertEventHandler();

		public delegate void onALTENTEREventHandler();

		public delegate void onEnterEventHandler();

		public delegate void onKeyEventHandler(string keycode);

		public delegate void onClickEventHandler();

		public delegate void PageUpEventHandler();

		public delegate void PageDownEventHandler();

		public delegate void onClipCopyEventHandler();

		public delegate void onESCEventHandler();

		public delegate void onFocusEventHandler();

		public delegate void onSendCharEventHandler(string text);
	}

	public class Helper
	{
		internal const int STARTF_USESHOWWINDOW = 1;
		internal const int STARTF_USESTDHANDLES = 256;
		internal const int STARTF_USEPOSITION = 4;
		internal const int STARTF_USESIZE = 2;
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
		internal const int GW_HWNDNEXT = 2;
		private const int SRCCOPY = 13369376;
		private const int SYNCHRONIZE = 1048576;
		private const int PROCESS_TERMINATE = 1;
		private const int WM_CLOSE = 16;

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern bool AttachConsole(int dwProcessId);

		[DllImport("user32", EntryPoint = "FindWindowA", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern int FindWindow([MarshalAs(UnmanagedType.VBByRefStr)] ref string lpClassName, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpWindowName);

		[DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern int FreeConsole();

		[DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern int GetParent(int hwnd);

		[DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern int GetWindow(int hwnd, int wCmd);

		[DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern int GetWindowThreadProcessId(int hwnd, ref int lpdwprocessid);

		[DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern int IsWindow(int hwnd);

		[DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern int ShowWindow(int hwnd, int nCmdShow);

		[DllImport("gdi32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		private static extern bool BitBlt(IntPtr hdcDest, int x, int y, int Width, int Height, IntPtr hdcSrc, int xSrc, int ySrc, int dwRop);

		[DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
		private static extern int OpenProcess(int dwDesiredAccess, int bInheritHandle, int dwProcessId);

		[DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
		private static extern int TerminateProcess(int hProcess, int uExitCode);

		[DllImport("kernel32", EntryPoint = "SearchPathA", CharSet = CharSet.Ansi, SetLastError = true)]
		private static extern int SearchPath([MarshalAs(UnmanagedType.VBByRefStr)] ref string lpPath, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpFileName, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpExtension, int nBufferLength, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpBuffer, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpFilePart);

		[DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SHGetFileInfo([MarshalAs(UnmanagedType.VBByRefStr)] ref string pszPath, int dwFileAttributes, ref Helper.SHFILEINFO psfi, int cbFileInfo, int uFlags);

		[DllImport("kernel32.dll")]
		private static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref Helper.STARTUPINFO lpStartupInfo, ref Helper.PROCESS_INFORMATION lpProcessInformation);

		internal static string SearchPath(string path)
		{
			if (path.Contains("\\"))
				return path;
			string str1 = new string(char.MinValue, 260);
			string str2 = (string)null;
			string str3 = (string)null;
			int nBufferLength = 260;
			int num = 0;
			string str4 = Conversions.ToString(num);
			int length = Helper.SearchPath(ref str2, ref path, ref str3, nBufferLength, ref str1, ref str4);
			return str1.Substring(0, length);
		}

		internal static bool isCUI(string path)
		{
			Helper.SHFILEINFO psfi = new Helper.SHFILEINFO();
			return (int)Helper.SHGetFileInfo(ref path, 0, ref psfi, 0, 8192) == 17744;
		}

		internal static int ProcIDFromWnd(int hwnd)
		{
			int lpdwprocessid = 0;
			Helper.GetWindowThreadProcessId(hwnd, ref lpdwprocessid);
			return lpdwprocessid;
		}

		internal static int GetWinHandle(int hInstance)
		{
			string str1 = (string)null;
			string str2 = (string)null;
			int num = 0;
			for (int window = Helper.FindWindow(ref str1, ref str2); window != 0; window = Helper.GetWindow(window, 2))
			{
				if (Helper.GetParent(window) == 0 && hInstance == Helper.ProcIDFromWnd(window))
				{
					num = window;
					break;
				}
			}
			return num;
		}

		internal static void ShowWindow(int hwnd)
		{
			Helper.ShowWindow(hwnd, 5);
		}

		internal static int LaunchExe(string path, ref int processID, bool visible = true)
		{
			Helper.PROCESS_INFORMATION lpProcessInformation = new Helper.PROCESS_INFORMATION();

			STARTUPINFO startupInfo = new Helper.STARTUPINFO()
			{
				dwFlags = !visible
							  ? 5U
							  : 4U,
				dwX = 10000U,
				dwY = 10000U,
				wShowWindow = (short)0
			};
			Helper.CreateProcess(path, (string)null, IntPtr.Zero, IntPtr.Zero, false, 0U, IntPtr.Zero, (string)null, ref startupInfo, ref lpProcessInformation);
			processID = checked((int)lpProcessInformation.dwProcessID);
			int num = 1;
			int winHandle;
			do
			{
				winHandle = Helper.GetWinHandle(processID);
				if (winHandle == 0)
				{
					Thread.Sleep(200);
					checked { ++num; }
				}
				else
					break;
			}
			while (num <= 10);
			return winHandle;
		}

		internal static bool Attach(int pid)
		{
			return Helper.AttachConsole(pid);
		}

		internal static System.Drawing.Color NativeColor2Color(uint colorvalue)
		{
			Helper.MyColor myColor = new MyColor();
			myColor.nativecolor = colorvalue;
			return System.Drawing.Color.FromArgb((int)myColor.red, (int)myColor.green, (int)myColor.blue);
		}

		private static bool isVista()
		{
			return Environment.OSVersion.Version.Major >= 6;
		}

		internal static uint[] GetConsoleColorTable()
		{
			string Expression = Application.ExecutablePath;
			uint[] numArray = new uint[17];
			if (Helper.isVista())
			{
				IntPtr hConsole = Native.GetStdHandle(4294967285U);
				Native.CONSOLE_SCREEN_BUFFER_INFOEX csbi = new Native.CONSOLE_SCREEN_BUFFER_INFOEX
				{
					cbSize = 96U
				};
				Native.GetConsoleScreenBufferInfoEx((uint)hConsole.ToInt32(), ref csbi);
				return csbi.ColorTable;
			}
			else
			{
				try
				{
					numArray[0] = 128U;
					numArray[1] = 16777215U;
					numArray[2] = 32768U;
					numArray[3] = 8421376U;
					numArray[4] = 16777215U;
					numArray[5] = 5645313U;
					numArray[6] = 15792110U;
					numArray[7] = 0U;
					numArray[8] = 8421504U;
					numArray[9] = 16711680U;
					numArray[10] = 65280U;
					numArray[11] = 16776960U;
					numArray[12] = (uint)byte.MaxValue;
					numArray[13] = 16711935U;
					numArray[14] = (uint)ushort.MaxValue;
					numArray[15] = 1048575U;
					string environmentVariable = Environment.GetEnvironmentVariable("systemroot");
					if (Expression.ToLower().StartsWith(environmentVariable.ToLower()))
						Expression = "%SystemRoot%" + Expression.Substring(environmentVariable.Length);
					RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Console\\" + Strings.Replace(Expression, "\\", "_", 1, -1, CompareMethod.Binary), true);
					if (registryKey == null)
						return numArray;
					int index = 0;
					do
					{
						int num = Conversions.ToInteger(registryKey.GetValue("ColorTable" + Strings.Right("0" + Conversions.ToString(index), 2), (object)-1));
						if (num > -1)
							numArray[index] = checked((uint)num);
						checked { ++index; }
					}
					while (index <= 15);
					registryKey.Close();
					return numArray;
				}
				finally
				{
				}
			}
		}

		internal static uint GetConsoleBackgroundColorFromReg(string path)
		{
			uint num = 0U;
			try
			{
				string environmentVariable = Environment.GetEnvironmentVariable("systemroot");
				if (path.ToLower().StartsWith(environmentVariable.ToLower()))
					path = "%SystemRoot%" + path.Substring(environmentVariable.Length);
				RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Console\\" + Strings.Replace(path, "\\", "_", 1, -1, CompareMethod.Binary), true);
				if (registryKey != null)
				{
					num = Conversions.ToUInteger(registryKey.GetValue("ColorTable" + Strings.Right("0" + Conversions.ToString((int)Console.BackgroundColor), 2)));
					registryKey.Close();
				}
			}
			finally
			{
			}
			return num;
		}

		internal static void CopyBitmap(Control orig, PictureBox dest)
		{
			Graphics graphics1 = orig.CreateGraphics();
			Bitmap bitmap = new Bitmap(orig.Width, orig.Height, graphics1);
			Graphics graphics2 = Graphics.FromImage((Image)bitmap);
			IntPtr hdc1 = graphics1.GetHdc();
			IntPtr hdc2 = graphics2.GetHdc();
			Helper.BitBlt(hdc2, 0, 0, orig.ClientRectangle.Width, orig.ClientRectangle.Height, hdc1, orig.ClientRectangle.X, orig.ClientRectangle.Y, 13369376);
			graphics1.ReleaseHdc(hdc1);
			graphics1.Dispose();
			graphics2.ReleaseHdc(hdc2);
			graphics2.Dispose();
			dest.Image = (Image)bitmap;
		}

		internal static Bitmap GetBitmap(Control orig)
		{
			Graphics graphics1 = orig.CreateGraphics();
			Bitmap bitmap = new Bitmap(orig.Width, orig.Height, graphics1);
			Graphics graphics2 = Graphics.FromImage((Image)bitmap);
			IntPtr hdc1 = graphics1.GetHdc();
			IntPtr hdc2 = graphics2.GetHdc();
			Helper.BitBlt(hdc2, 0, 0, orig.ClientRectangle.Width, orig.ClientRectangle.Height, hdc1, orig.ClientRectangle.X, orig.ClientRectangle.Y, 13369376);
			graphics1.ReleaseHdc(hdc1);
			graphics1.Dispose();
			graphics2.ReleaseHdc(hdc2);
			graphics2.Dispose();
			return bitmap;
		}

		internal static void TerminateProcess(int id)
		{
			int hProcess = Helper.OpenProcess(1048577, 0, id);
			if (hProcess == 0 || Helper.TerminateProcess(hProcess, 0) == 0)
				return;
			int num = (int)MessageBox.Show("Killed Process ID=" + Conversions.ToString(id));
		}

		[StructLayout(LayoutKind.Explicit)]
		internal struct MyColor
		{
			[FieldOffset(0)]
			internal uint nativecolor;
			[FieldOffset(0)]
			internal byte red;
			[FieldOffset(1)]
			internal byte green;
			[FieldOffset(2)]
			internal byte blue;
			[FieldOffset(3)]
			internal byte alpha;
		}

		internal struct PROCESS_INFORMATION
		{
			internal IntPtr hProcess;
			internal IntPtr hThread;
			internal uint dwProcessID;
			internal uint dwThreadID;
		}

		internal struct STARTUPINFO
		{
			internal uint cb;
			internal string lpReserved;
			internal string lpDesktop;
			internal string lpTitle;
			internal uint dwX;
			internal uint dwY;
			internal uint dwXSize;
			internal uint dwYSize;
			internal uint dwXCountChars;
			internal uint dwYCountChars;
			internal uint dwFillAttribute;
			internal uint dwFlags;
			internal short wShowWindow;
			internal short cbReserved2;
			internal IntPtr lpReserved2;
			internal IntPtr hStdInput;
			internal IntPtr hStdOutput;
			internal IntPtr hStdError;
		}

		private struct SHFILEINFO
		{
			public IntPtr hIcon;
			public int iIcon;
			public int dwAttributes;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szDisplayName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public string szTypeName;
		}
	}
}
