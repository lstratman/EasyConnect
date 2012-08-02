using System;
using System.Runtime.InteropServices;

namespace WalburySoftware
{
	public class WinApi
	{
		#region imported constants
		public const UInt16 WSADESCRIPTION_LEN = 256;
		public const UInt16 WSASYS_STATUS_LEN  = 128;
		#region winsock error codes
		// * All Windows Sockets error constants are biased by WSABASEERR from
		// * the "normal"
		public const uint WSABASEERR  = 10000;
		// Windows Sockets definitions of regular Microsoft C error constants
		public const uint WSAEINTR  =  (WSABASEERR+4);
		public const uint WSAEBADF  =  (WSABASEERR+9);
		public const uint WSAEACCES =  (WSABASEERR+13);
		public const uint WSAEFAULT =  (WSABASEERR+14);
		public const uint WSAEINVAL =  (WSABASEERR+22);
		public const uint WSAEMFILE =  (WSABASEERR+24);
		//Windows Sockets definitions of regular Berkeley error constants
		public const uint WSAEWOULDBLOCK         = (WSABASEERR+35);
		public const uint WSAEINPROGRESS         = (WSABASEERR+36);
		public const uint WSAEALREADY            = (WSABASEERR+37);
		public const uint WSAENOTSOCK            = (WSABASEERR+38);
		public const uint WSAEDESTADDRREQ        = (WSABASEERR+39);
		public const uint WSAEMSGSIZE            = (WSABASEERR+40);
		public const uint WSAEPROTOTYPE          = (WSABASEERR+41);
		public const uint WSAENOPROTOOPT         = (WSABASEERR+42);
		public const uint WSAEPROTONOSUPPORT     = (WSABASEERR+43);
		public const uint WSAESOCKTNOSUPPORT     = (WSABASEERR+44);
		public const uint WSAEOPNOTSUPP          = (WSABASEERR+45);
		public const uint WSAEPFNOSUPPORT        = (WSABASEERR+46);
		public const uint WSAEAFNOSUPPORT        = (WSABASEERR+47);
		public const uint WSAEADDRINUSE          = (WSABASEERR+48);
		public const uint WSAEADDRNOTAVAIL       = (WSABASEERR+49);
		public const uint WSAENETDOWN            = (WSABASEERR+50);
		public const uint WSAENETUNREACH         = (WSABASEERR+51);
		public const uint WSAENETRESET           = (WSABASEERR+52);
		public const uint WSAECONNABORTED        = (WSABASEERR+53);
		public const uint WSAECONNRESET          = (WSABASEERR+54);
		public const uint WSAENOBUFS             = (WSABASEERR+55);
		public const uint WSAEISCONN             = (WSABASEERR+56);
		public const uint WSAENOTCONN            = (WSABASEERR+57);
		public const uint WSAESHUTDOWN           = (WSABASEERR+58);
				public const uint WSAETOOMANYREFS        = (WSABASEERR+59);
				public const uint WSAETIMEDOUT           = (WSABASEERR+60);
				public const uint WSAECONNREFUSED        = (WSABASEERR+61);
				public const uint WSAELOOP               = (WSABASEERR+62);
				public const uint WSAENAMETOOLONG        = (WSABASEERR+63);
				public const uint WSAEHOSTDOWN           = (WSABASEERR+64);
				public const uint WSAEHOSTUNREACH        = (WSABASEERR+65);
				public const uint WSAENOTEMPTY           = (WSABASEERR+66);
				public const uint WSAEPROCLIM            = (WSABASEERR+67);
				public const uint WSAEUSERS              = (WSABASEERR+68);
				public const uint WSAEDQUOT              = (WSABASEERR+69);
				public const uint WSAESTALE              = (WSABASEERR+70);
				public const uint WSAEREMOTE             = (WSABASEERR+71);
				//Extended Windows Sockets error constant definitions
				public const uint WSASYSNOTREADY         = (WSABASEERR+91);
				public const uint WSAVERNOTSUPPORTED     = (WSABASEERR+92);
				public const uint WSANOTINITIALISED      = (WSABASEERR+93);
				public const uint WSAEDISCON             = (WSABASEERR+101);
				public const uint WSAENOMORE             = (WSABASEERR+102);
				public const uint WSAECANCELLED          = (WSABASEERR+103);
				public const uint WSAEINVALIDPROCTABLE   = (WSABASEERR+104);
				public const uint WSAEINVALIDPROVIDER    = (WSABASEERR+105);
				public const uint WSAEPROVIDERFAILEDINIT = (WSABASEERR+106);
				public const uint WSASYSCALLFAILURE      = (WSABASEERR+107);
				public const uint WSASERVICE_NOT_FOUND   = (WSABASEERR+108);
				public const uint WSATYPE_NOT_FOUND      = (WSABASEERR+109);
				public const uint WSA_E_NO_MORE          = (WSABASEERR+110);
				public const uint WSA_E_CANCELLED        = (WSABASEERR+111);
				public const uint WSAEREFUSED            = (WSABASEERR+112);
				//Error return codes from gethostbyname() and gethostbyaddr()
				//(when using the resolver). Note that these errors are
				//retrieved via WSAGetLastError() and must therefore follow
				//the rules for avoiding clashes with error numbers from
				//specific implementations or language run-time systems.
				//For this reason the codes are based at WSABASEERR+1001.
				//Note also that [WSA]NO_ADDRESS is defined only for
				//compatibility purposes.

				//Authoritative Answer: Host not found
				public const uint WSAHOST_NOT_FOUND      = (WSABASEERR+1001);

				//Non-Authoritative: Host not found, or SERVERFAIL
				public const uint WSATRY_AGAIN           = (WSABASEERR+1002);

				//Non-recoverable errors, FORMERR, REFUSED, NOTIMP
				public const uint WSANO_RECOVERY         = (WSABASEERR+1003);

				//Valid name, no data record of requested type
				public const uint WSANO_DATA             = (WSABASEERR+1004);

				//Define QOS related error return codes

				public const uint WSA_QOS_RECEIVERS              = (WSABASEERR + 1005);
				//at least one Reserve has arrived
				public const uint WSA_QOS_SENDERS                = (WSABASEERR + 1006);
				//at least one Path has arrived
				public const uint WSA_QOS_NO_SENDERS             = (WSABASEERR + 1007);
				//there are no senders
				public const uint WSA_QOS_NO_RECEIVERS           = (WSABASEERR + 1008);
				//there are no receivers
				public const uint WSA_QOS_REQUEST_CONFIRMED      = (WSABASEERR + 1009);
				//Reserve has been confirmed
				public const uint WSA_QOS_ADMISSION_FAILURE      = (WSABASEERR + 1010);
				//error due to lack of resources
				public const uint WSA_QOS_POLICY_FAILURE         = (WSABASEERR + 1011);
				//rejected for administrative reasons - bad credentials
				public const uint WSA_QOS_BAD_STYLE              = (WSABASEERR + 1012);
				//unknown or conflicting style
				public const uint WSA_QOS_BAD_OBJECT             = (WSABASEERR + 1013);
				//problem with some part of the filterspec or providerspecific
				//buffer in general
				public const uint WSA_QOS_TRAFFIC_CTRL_ERROR     = (WSABASEERR + 1014);
				//problem with some part of the flowspec
				public const uint WSA_QOS_GENERIC_ERROR          = (WSABASEERR + 1015);
				//general error
				public const uint WSA_QOS_ESERVICETYPE           = (WSABASEERR + 1016);
				//invalid service type in flowspec
				public const uint WSA_QOS_EFLOWSPEC              = (WSABASEERR + 1017);
				//invalid flowspec
				public const uint WSA_QOS_EPROVSPECBUF           = (WSABASEERR + 1018);
				//invalid provider specific buffer
				public const uint WSA_QOS_EFILTERSTYLE           = (WSABASEERR + 1019);
				//invalid filter style
				public const uint WSA_QOS_EFILTERTYPE            = (WSABASEERR + 1020);
				//invalid filter type
				public const uint WSA_QOS_EFILTERCOUNT           = (WSABASEERR + 1021);
				//incorrect number of filters
				public const uint WSA_QOS_EOBJLENGTH             = (WSABASEERR + 1022);
				//invalid object length
				public const uint WSA_QOS_EFLOWCOUNT             = (WSABASEERR + 1023);
				//incorrect number of flows
				public const uint WSA_QOS_EUNKOWNPSOBJ           = (WSABASEERR + 1024);
				//unknown object in provider specific buffer
				public const uint WSA_QOS_EPOLICYOBJ             = (WSABASEERR + 1025);
				//invalid policy object in provider specific buffer
				public const uint WSA_QOS_EFLOWDESC              = (WSABASEERR + 1026);
				//invalid flow descriptor in the list
				public const uint WSA_QOS_EPSFLOWSPEC            = (WSABASEERR + 1027);
				//inconsistent flow spec in provider specific buffer
				public const uint WSA_QOS_EPSFILTERSPEC          = (WSABASEERR + 1028);
				//invalid filter spec in provider specific buffer
				public const uint WSA_QOS_ESDMODEOBJ             = (WSABASEERR + 1029);
				//invalid shape discard mode object in provider specific buffer
				public const uint WSA_QOS_ESHAPERATEOBJ          = (WSABASEERR + 1030);
				//invalid shaping rate object in provider specific buffer
				public const uint WSA_QOS_RESERVED_PETYPE        = (WSABASEERR + 1031);
				//reserved policy element in provider specific buffer
				#endregion
				#endregion
		#region imported functions
		[DllImport("WS2_32.DLL")]
		public static extern int WSAStartup(UInt16 wVersionRequested, WSADATA lpWSAData);
		[DllImport("WS2_32.DLL")]
		public static extern uint inet_addr(string cp);
		//Requires Icmp.dll on Windows 2000 Server and Windows 2000 Professional. 
		//Requires Iphlpapi.dll on Windows Server 2003 and Windows XP
		[DllImport("iphlpapi.DLL", EntryPoint="IcmpCreateFile",  SetLastError=true,
			 CallingConvention=CallingConvention.StdCall)]
		public static extern IntPtr IcmpCreateFile();
		[DllImport("iphlpapi.DLL", EntryPoint="IcmpSendEcho",  SetLastError=true,
			 CallingConvention=CallingConvention.StdCall)]
		public static extern uint IcmpSendEcho(
			IntPtr IcmpHandle,
			uint DestinationAddress,
			byte[] RequestData,
			short RequestSize,
			IntPtr Null,
			byte[] ReplyBuffer,
			uint ReplySize,
			uint Timeout
			);
		[DllImport("iphlpapi.DLL", EntryPoint="IcmpCloseHandle",  SetLastError=true,
			 CallingConvention=CallingConvention.StdCall)]
		public static extern bool IcmpCloseHandle(IntPtr IcmpHandle);
		#endregion
		#region typedefs
		[StructLayout(LayoutKind.Sequential)]
		public class WSADATA
		{
			public ushort wVersion = 0;  
			public ushort wHighVersion = 0;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=(WSADESCRIPTION_LEN+1))]
			public string szDescription = "";
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=(WSASYS_STATUS_LEN+1))]
			public string szSystemStatus = "";
			public ushort iMaxSockets = 0;  
			public ushort iMaxUdpDg = 0;  
			[MarshalAs(UnmanagedType.LPStr)]
			public string lpVendorInfo = "";
		};
		#endregion
		#region 'C' Macros redefined as functions
		public static ushort MAKEWORD(byte a, byte b) 
		{ 
			return (ushort)(a | (b << 8)); 
		} 
		#endregion
		#region stuff for System Menu
		#region Message Constants
		public const Int32 WM_NOTIFY          = 0x004E;
		public const Int32 WM_COMMAND         = 0x0111;
		public const Int32 WM_SYSCOMMAND      = 0x112;
		public const Int32 WM_NOTIFYFORMAT    = 0x0055;
		public const Int32 WM_NCMOUSEMOVE                 = 0x00A0;
		public const Int32 WM_NCLBUTTONDOWN               = 0x00A1;
		public const Int32 WM_NCLBUTTONUP                 = 0x00A2;
		public const Int32 WM_NCLBUTTONDBLCLK             = 0x00A3;
		public const Int32 WM_NCRBUTTONDOWN               = 0x00A4;
		public const Int32 WM_NCRBUTTONUP                 = 0x00A5;
		public const Int32 WM_NCRBUTTONDBLCLK             = 0x00A6;
		public const Int32 WM_NCMBUTTONDOWN               = 0x00A7;
		public const Int32 WM_NCMBUTTONUP                 = 0x00A8;
		public const Int32 WM_NCMBUTTONDBLCLK             = 0x00A9;
		public const Int32 WM_DEVMODECHANGE               = 0x001B;
		public const Int32 WM_ACTIVATEAPP                 = 0x001C;
		public const Int32 WM_FONTCHANGE                  = 0x001D;
		public const Int32 WM_TIMECHANGE                  = 0x001E;
		public const Int32 WM_CANCELMODE                  = 0x001F;
		public const Int32 WM_SETCURSOR                   = 0x0020;
		public const Int32 WM_MOUSEACTIVATE               = 0x0021;
		public const Int32 WM_CHILDACTIVATE               = 0x0022;
		public const Int32 WM_QUEUESYNC                   = 0x0023;
		#endregion 

		#region Menu Flags
		//public const Int32 MF_SEPARATOR       = 0x800;
		//public const Int32 MF_BYPOSITION      = 0x400;
		//public const Int32 MF_STRING          = 0x0;
		/*
		 * Menu flags for Add/Check/EnableMenuItem()
		 */
		public const Int32 MF_INSERT          = 0x00000000;
		public const Int32 MF_CHANGE          = 0x00000080;
		public const Int32 MF_APPEND          = 0x00000100;
		public const Int32 MF_DELETE          = 0x00000200;
		public const Int32 MF_REMOVE          = 0x00001000;
		
		public const Int32 MF_BYCOMMAND       = 0x00000000;
		public const Int32 MF_BYPOSITION      = 0x00000400;

		public const Int32 MF_SEPARATOR       = 0x00000800;

		public const Int32 MF_ENABLED         = 0x00000000;
		public const Int32 MF_GRAYED          = 0x00000001;
		public const Int32 MF_DISABLED        = 0x00000002;

		public const Int32 MF_UNCHECKED       = 0x00000000;
		public const Int32 MF_CHECKED         = 0x00000008;
		public const Int32 MF_USECHECKBITMAPS = 0x00000200;

		public const Int32 MF_STRING          = 0x00000000;
		public const Int32 MF_BITMAP          = 0x00000004;
		public const Int32 MF_OWNERDRAW       = 0x00000100;

		public const Int32 MF_POPUP           = 0x00000010;
		public const Int32 MF_MENUBARBREAK    = 0x00000020;
		public const Int32 MF_MENUBREAK       = 0x00000040;

		public const Int32 MF_UNHILITE       =  0x00000000;
		public const Int32 MF_HILITE         =  0x00000080;
		#endregion
		#region menu defs
		public const Int32 IDM_RENAMECAPTION     = 1000;
		public const Int32 IDM_HELP			     = 1001;
		public const Int32 IDM_HELPABOUT	     = 1002;
		public const Int32 IDM_DIAGNOSTICS       = 1003;
		public const Int32 IDM_TOGGLELITE_HEIGHT = 1004;
		public const Int32 IDM_TOGGLELITE_WIDTH  = 1005;
		public const Int32 IDM_TOGGLE_FORM_SIZABLE = 1006;
		#endregion
		#region fuctions
		[DllImport("user32.dll")]
		public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
		[DllImport("user32.dll")]
		public static extern bool InsertMenu (IntPtr hMenu, Int32 wPosition, Int32 wFlags, Int32 wIDNewItem, string lpNewItem);
		[DllImport("user32.dll")]
		public static extern uint CheckMenuItem(IntPtr hMenu, uint nIDCheckItem, uint nCheck);
		[DllImport("user32.dll")]
		public static extern uint GetMenuState(IntPtr hMenu, uint nID, uint nFlags); 
		/// <summary>
		/// The EnableMenuItem function enables, disables, or grays the specified menu item. 
		/// </summary>
		/// <param name="hMenu">[in] Handle to the menu. </param>
		/// <param name="uIDEnableItem">[in] Specifies the menu item to be enabled, disabled, or grayed, 
		/// as determined by the uEnable parameter. This parameter specifies an item in a menu bar, menu, or submenu. 
		/// </param>
		/// <param name="uEnable">
		/// [in] Controls the interpretation of the uIDEnableItem parameter and indicate 
		/// whether the menu item is enabled, disabled, or grayed. This parameter must be 
		/// a combination of either MF_BYCOMMAND or MF_BYPOSITION and MF_ENABLED, MF_DISABLED, or MF_GRAYED. 
		/// MF_BYCOMMAND - Indicates that uIDEnableItem gives the identifier of the menu item. 
		/// If neither the MF_BYCOMMAND nor MF_BYPOSITION flag is specified, the MF_BYCOMMAND 
		/// flag is the default flag.
		/// MF_BYPOSITION - Indicates that uIDEnableItem gives the zero-based relative position of the menu item.
		/// MF_DISABLED - Indicates that the menu item is disabled, but not grayed, so it cannot be selected.
		/// MF_ENABLED - Indicates that the menu item is enabled and restored from a grayed state so that it can be selected.
		/// MF_GRAYED - Indicates that the menu item is disabled and grayed so that it cannot be selected.
		/// </param>
		/// <returns>The return value specifies the previous state of the menu item (it is either 
		/// MF_DISABLED, MF_ENABLED, or MF_GRAYED). If the menu item does not exist, the return value is -1.</returns>
		[DllImport("user32.dll")]
		public static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

		#endregion
		#endregion
		#region stuff for process communication i'm not even using
		const uint PROCESS_ALL_ACCESS = (uint)(0x000F0000L | 0x00100000L | 0xFFF);
		const uint MEM_COMMIT         = 0x1000;
		const uint MEM_RELEASE        = 0x8000;
		const uint PAGE_READWRITE     = 0x04;

		[DllImport("user32.dll")]
		static extern bool SendMessage(IntPtr hWnd, Int32 msg, Int32 wParam, IntPtr lParam);
	
		[DllImport("user32")]
		static extern IntPtr GetWindowThreadProcessId( IntPtr hWnd, out int lpwdProcessID );    
  
		[DllImport("kernel32")]
		static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

		[DllImport("kernel32")]
		static extern IntPtr VirtualAllocEx( IntPtr hProcess, IntPtr lpAddress, int dwSize, uint flAllocationType, uint flProtect);

		[DllImport("kernel32")]
		static extern bool VirtualFreeEx( IntPtr hProcess, IntPtr lpAddress, int dwSize, uint dwFreeType );

		[DllImport("kernel32")]
		static extern bool WriteProcessMemory( IntPtr hProcess, IntPtr lpBaseAddress, ref string buffer, int dwSize, IntPtr lpNumberOfBytesWritten );

		[DllImport("kernel32")]
		static extern bool ReadProcessMemory( IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int dwSize, IntPtr lpNumberOfBytesRead );

		[DllImport("kernel32")]
		static extern bool CloseHandle( IntPtr hObject );
		#endregion
		#region time.h stuff
		[DllImport("MSVCRT.DLL")]
		public static extern void time(ref long t);
		#endregion
	}
}
