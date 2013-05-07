using System;
using System.Drawing;
using System.Management.Automation.Host;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Poderosa.Terminal;
using WalburySoftware;
using Win32Interop.Methods;
using Rectangle = System.Management.Automation.Host.Rectangle;
using Size = System.Management.Automation.Host.Size;

namespace EasyConnect.Protocols.PowerShell
{
	/// <summary>
	/// A sample implementation of the PSHostRawUserInterface for console
	/// applications. Members of this class that easily map to the .NET 
	/// console class are implemented. More complex methods are not 
	/// implemented and throw a NotImplementedException exception.
	/// </summary>
	internal class PowerShellRawUi : PSHostRawUserInterface
	{
		protected TerminalControl _terminal;
		private ConsoleColor _foregroundColor;
		private ConsoleColor _backgroundColor;
		protected ManualResetEvent _inputSemaphore = new ManualResetEvent(false);
		protected Thread _inputThread;
		protected KeyInfo _readKey;

		public PowerShellRawUi(TerminalControl terminal)
		{
			_terminal = terminal;
			_backgroundColor = ClosestConsoleColor(_terminal.TerminalPane.ConnectionTag.RenderProfile.BackColor);
			_backgroundColor = ClosestConsoleColor(_terminal.TerminalPane.ConnectionTag.RenderProfile.ForeColor);
		}

		protected ConsoleColor ClosestConsoleColor(Color color)
		{
			ConsoleColor ret = 0;
			double rr = color.R, gg = color.G, bb = color.B, delta = double.MaxValue;

			foreach (ConsoleColor cc in Enum.GetValues(typeof(ConsoleColor)))
			{
				var n = Enum.GetName(typeof(ConsoleColor), cc);
				var c = System.Drawing.Color.FromName(n == "DarkYellow" ? "Orange" : n); // bug fix
				var t = Math.Pow(c.R - rr, 2.0) + Math.Pow(c.G - gg, 2.0) + Math.Pow(c.B - bb, 2.0);
				if (t == 0.0)
					return cc;
				if (t < delta)
				{
					delta = t;
					ret = cc;
				}
			}
			return ret;
		}

		public void RestoreForegroundColor()
		{
			byte[] data = Encoding.UTF8.GetBytes("\x001B[39m");
			_terminal.TerminalPane.ConnectionTag.Receiver.DataArrived(data, 0, data.Length);
		}

		public void RestoreBackgroundColor()
		{
			byte[] data = Encoding.UTF8.GetBytes("\x001B[49m");
			_terminal.TerminalPane.ConnectionTag.Receiver.DataArrived(data, 0, data.Length);
		}

		/// <summary>
		/// Gets or sets the background color of text to be written.
		/// This maps to the corresponding Console.Background property.
		/// </summary>
		public override ConsoleColor BackgroundColor
		{
			get
			{
				return _backgroundColor;
			}
			set
			{
				_backgroundColor = value;
				int commandValue = 0;

				switch (value)
				{
					case ConsoleColor.Black:
						commandValue = 40;
						break;

					case ConsoleColor.DarkBlue:
					case ConsoleColor.Blue:
						commandValue = 44;
						break;

					case ConsoleColor.DarkCyan:
					case ConsoleColor.Cyan:
						commandValue = 46;
						break;

					case ConsoleColor.DarkGray:
					case ConsoleColor.Gray:
					case ConsoleColor.White:
						commandValue = 47;
						break;

					case ConsoleColor.DarkGreen:
					case ConsoleColor.Green:
						commandValue = 42;
						break;

					case ConsoleColor.DarkMagenta:
					case ConsoleColor.Magenta:
						commandValue = 45;
						break;

					case ConsoleColor.DarkRed:
					case ConsoleColor.Red:
						commandValue = 41;
						break;

					case ConsoleColor.DarkYellow:
					case ConsoleColor.Yellow:
						commandValue = 43;
						break;
				}

				byte[] data = Encoding.UTF8.GetBytes("\x001B[" + commandValue + "m");
				_terminal.TerminalPane.ConnectionTag.Receiver.DataArrived(data, 0, data.Length);
			}
		}

		/// <summary>
		/// Gets or sets the host buffer size adapted from the Console buffer 
		/// size members.
		/// </summary>
		public override Size BufferSize
		{
			get
			{
				return new Size(_terminal.TerminalPane.Connection.TerminalWidth, _terminal.TerminalPane.ConnectionTag.Document.Size);
			}
			set
			{
			}
		}

		/// <summary>
		/// Gets or sets the cursor position. In this example this 
		/// functionality is not needed so the property throws a 
		/// NotImplementException exception.
		/// </summary>
		public override Coordinates CursorPosition
		{
			get
			{
				return new Coordinates(_terminal.TerminalPane.ConnectionTag.Document.CaretColumn, _terminal.TerminalPane.ConnectionTag.Document.CurrentLineNumber);
			}
			set
			{
				Coordinates relativePosition = new Coordinates(value.X + 1, value.Y + 1 - _terminal.TerminalPane.ConnectionTag.Document.TopLineNumber);

				byte[] data = Encoding.UTF8.GetBytes(String.Format("\x001B[{1};{0}f", relativePosition.X, relativePosition.Y));
				_terminal.TerminalPane.ConnectionTag.Receiver.DataArrived(data, 0, data.Length);
			}
		}

		/// <summary>
		/// Gets or sets the cursor size taken directly from the 
		/// Console.CursorSize property.
		/// </summary>
		public override int CursorSize
		{
			get
			{
				return Convert.ToInt32(_terminal.TerminalPane.ConnectionTag.RenderProfile.FontSize);
			}
			set
			{
			}
		}

		/// <summary>
		/// Gets or sets the foreground color of the text to be written.
		/// This maps to the corresponding Console.ForgroundColor property.
		/// </summary>
		public override ConsoleColor ForegroundColor
		{
			get
			{
				return _foregroundColor;
			}
			set
			{
				_foregroundColor = value;

				int commandValue = 0;

				switch (value)
				{
					case ConsoleColor.Black:
						commandValue = 30;
						break;

					case ConsoleColor.DarkBlue:
					case ConsoleColor.Blue:
						commandValue = 34;
						break;

					case ConsoleColor.DarkCyan:
					case ConsoleColor.Cyan:
						commandValue = 36;
						break;

					case ConsoleColor.DarkGray:
					case ConsoleColor.Gray:
					case ConsoleColor.White:
						commandValue = 37;
						break;

					case ConsoleColor.DarkGreen:
					case ConsoleColor.Green:
						commandValue = 32;
						break;

					case ConsoleColor.DarkMagenta:
					case ConsoleColor.Magenta:
						commandValue = 35;
						break;

					case ConsoleColor.DarkRed:
					case ConsoleColor.Red:
						commandValue = 31;
						break;

					case ConsoleColor.DarkYellow:
					case ConsoleColor.Yellow:
						commandValue = 33;
						break;
				}

				byte[] data = Encoding.UTF8.GetBytes("\x001B[" + commandValue + "m");
				_terminal.TerminalPane.ConnectionTag.Receiver.DataArrived(data, 0, data.Length);
			}
		}

		/// <summary>
		/// Gets a value indicating whether a key is available. This maps to  
		/// the corresponding Console.KeyAvailable property.
		/// </summary>
		public override bool KeyAvailable
		{
			get
			{
				return _terminal.TerminalPane.ConnectionTag.Terminal.TerminalMode == TerminalMode.Normal;
			}
		}

		/// <summary>
		/// Gets the maximum physical size of the window adapted from the  
		/// Console.LargestWindowWidth and Console.LargestWindowHeight 
		/// properties.
		/// </summary>
		public override Size MaxPhysicalWindowSize
		{
			get
			{
				return WindowSize;
			}
		}

		/// <summary>
		/// Gets the maximum window size adapted from the 
		/// Console.LargestWindowWidth and console.LargestWindowHeight 
		/// properties.
		/// </summary>
		public override Size MaxWindowSize
		{
			get
			{
				return WindowSize;
			}
		}

		/// <summary>
		/// Gets or sets the window position adapted from the Console window position 
		/// members.
		/// </summary>
		public override Coordinates WindowPosition
		{
			get
			{
				return new Coordinates(0, _terminal.TerminalPane.ConnectionTag.Document.TopLineNumber);
			}
			set
			{
				_terminal.TerminalPane.ConnectionTag.Document.TopLineNumber = value.Y;
			}
		}

		/// <summary>
		/// Gets or sets the window size adapted from the corresponding Console 
		/// calls.
		/// </summary>
		public override Size WindowSize
		{
			get
			{
				return new Size(_terminal.TerminalPane.Connection.TerminalWidth, _terminal.TerminalPane.Connection.TerminalHeight);
			}
			set
			{
			}
		}

		/// <summary>
		/// Gets or sets the title of the window mapped to the Console.Title 
		/// property.
		/// </summary>
		public override string WindowTitle
		{
			get
			{
				return "";
			}
			set
			{
			}
		}

		/// <summary>
		/// This API resets the input buffer. In this example this 
		/// functionality is not needed so the method returns nothing.
		/// </summary>
		public override void FlushInputBuffer()
		{
		}

		/// <summary>
		/// This API returns a rectangular region of the screen buffer. In 
		/// this example this functionality is not needed so the method throws 
		/// a NotImplementException exception.
		/// </summary>
		/// <param name="rectangle">Defines the size of the rectangle.</param>
		/// <returns>Throws a NotImplementedException exception.</returns>
		public override BufferCell[,] GetBufferContents(Rectangle rectangle)
		{
			throw new NotImplementedException(
				"The method or operation is not implemented.");
		}

		/// <summary>
		/// This API Reads a pressed, released, or pressed and released keystroke 
		/// from the keyboard device, blocking processing until a keystroke is 
		/// typed that matches the specified keystroke options. In this example 
		/// this functionality is not needed so the method throws a
		/// NotImplementException exception.
		/// </summary>
		/// <param name="options">Options, such as IncludeKeyDown,  used when 
		/// reading the keyboard.</param>
		/// <returns>Throws a NotImplementedException exception.</returns>
		public override KeyInfo ReadKey(ReadKeyOptions options)
		{
			StreamConnection connection = _terminal.TerminalPane.ConnectionTag.Connection as StreamConnection;
			connection.Capture = true;
			_inputSemaphore.Reset();

			_inputThread = new Thread(ReadInput);

			_inputThread.Start(new Tuple<StreamConnection, ReadKeyOptions>(connection, options));
			_inputSemaphore.WaitOne();
			connection.Capture = false;

			return _readKey;
		}

		private void ReadInput(object state)
		{
			StreamConnection connection = (state as Tuple<StreamConnection, ReadKeyOptions>).Item1;
			ReadKeyOptions options = (state as Tuple<StreamConnection, ReadKeyOptions>).Item2;
			bool inEscapeSequence = false;
			bool readKey = false;

			while (!readKey)
			{
				if (connection.OutputQueue.Count > 0)
				{
					while (connection.OutputQueue.Count > 0)
					{
						byte currentByte = connection.OutputQueue.Dequeue();

						if (currentByte == 8)
						{
							_readKey = new KeyInfo
								           {
									           VirtualKeyCode = 8
								           };

							if ((options & ReadKeyOptions.NoEcho) != ReadKeyOptions.NoEcho && CursorPosition.X > 1)
								CursorPosition = new Coordinates(CursorPosition.X - 1, CursorPosition.Y);

							readKey = true;
							break;
						}

						else if (currentByte == 27)
							inEscapeSequence = true;

						else if (currentByte == 91 && inEscapeSequence)
						{
						}

						else if (currentByte == 126 && inEscapeSequence)
						{
						}

						else if (currentByte == 55 && inEscapeSequence)
						{
							_readKey = new KeyInfo
								           {
									           VirtualKeyCode = 0x24
								           };

							if ((options & ReadKeyOptions.NoEcho) != ReadKeyOptions.NoEcho)
								CursorPosition = new Coordinates(1, CursorPosition.Y);

							readKey = true;
							break;
						}

						else if (currentByte == 56 && inEscapeSequence)
						{
							_readKey = new KeyInfo
								           {
									           VirtualKeyCode = 0x23
								           };

							if ((options & ReadKeyOptions.NoEcho) != ReadKeyOptions.NoEcho)
								CursorPosition = new Coordinates(BufferSize.Width, CursorPosition.Y);

							readKey = true;
							break;
						}

						else if (currentByte == 68 && inEscapeSequence)
						{
							_readKey = new KeyInfo
								           {
									           VirtualKeyCode = 25
								           };

							if ((options & ReadKeyOptions.NoEcho) != ReadKeyOptions.NoEcho && CursorPosition.X > 1)
								CursorPosition = new Coordinates(CursorPosition.X - 1, CursorPosition.Y);

							readKey = true;
							break;
						}

						else if (currentByte == 67 && inEscapeSequence)
						{
							_readKey = new KeyInfo
								           {
									           VirtualKeyCode = 27
								           };

							if ((options & ReadKeyOptions.NoEcho) != ReadKeyOptions.NoEcho && CursorPosition.X < BufferSize.Width)
								CursorPosition = new Coordinates(CursorPosition.X + 1, CursorPosition.Y);

							readKey = true;
							break;
						}

						else if (currentByte == 51 && inEscapeSequence)
						{
							_readKey = new KeyInfo
								           {
									           VirtualKeyCode = 0x23
								           };

							if ((options & ReadKeyOptions.NoEcho) != ReadKeyOptions.NoEcho)
								CursorPosition = new Coordinates(BufferSize.Width, CursorPosition.Y);

							readKey = true;
							break;
						}

						else if (currentByte == 13)
						{
							_readKey = new KeyInfo
								           {
											   Character = '\r',
									           VirtualKeyCode = 0x0D
								           };

							if ((options & ReadKeyOptions.NoEcho) != ReadKeyOptions.NoEcho)
								CursorPosition = new Coordinates(1, CursorPosition.Y);

							readKey = true;
							break;
						}

						else
						{
							short virtualKey = User32.VkKeyScan((char) currentByte);
							int modifiers = virtualKey >> 8;
							ControlKeyStates controlKeys = 0;

							if ((modifiers & 2) != 0)
								controlKeys |= ControlKeyStates.LeftCtrlPressed;

							if ((modifiers & 4) != 0)
								controlKeys |= ControlKeyStates.LeftAltPressed;

							_readKey = new KeyInfo
								           {
									           Character = (char) currentByte,
									           VirtualKeyCode = (virtualKey & 0xFF),
											   ControlKeyState = controlKeys
								           };

							if ((options & ReadKeyOptions.NoEcho) != ReadKeyOptions.NoEcho)
								_terminal.TerminalPane.ConnectionTag.Receiver.DataArrived(
									new byte[]
										{
											currentByte
										}, 0, 1);

							readKey = true;
							break;
						}
					}
				}
				
				if (!readKey)
					Thread.Sleep(50);
			}

			_inputSemaphore.Set();
		}

		/// <summary>
		/// This API crops a region of the screen buffer. In this example 
		/// this functionality is not needed so the method throws a
		/// NotImplementException exception.
		/// </summary>
		/// <param name="source">The region of the screen to be scrolled.</param>
		/// <param name="destination">The region of the screen to receive the 
		/// source region contents.</param>
		/// <param name="clip">The region of the screen to include in the operation.</param>
		/// <param name="fill">The character and attributes to be used to fill all cell.</param>
		public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
		{
			throw new NotImplementedException(
				"The method or operation is not implemented.");
		}

		/// <summary>
		/// This API copies an array of buffer cells into the screen buffer 
		/// at a specified location. In this example this  functionality is 
		/// not needed si the method  throws a NotImplementedException exception.
		/// </summary>
		/// <param name="origin">The parameter is not used.</param>
		/// <param name="contents">The parameter is not used.</param>
		public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
		{
			throw new NotImplementedException(
				"The method or operation is not implemented.");
		}

		/// <summary>
		/// This API Copies a given character, foreground color, and background 
		/// color to a region of the screen buffer. In this example this 
		/// functionality is not needed so the method throws a
		/// NotImplementException exception./// </summary>
		/// <param name="rectangle">Defines the area to be filled. </param>
		/// <param name="fill">Defines the fill character.</param>
		public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
		{
			BufferCell[,] contents = new BufferCell[rectangle.Right - rectangle.Left, rectangle.Bottom - rectangle.Top];

			for (int i = 0; i < rectangle.Right - rectangle.Left; i++)
			{
				for (int j = 0; j < rectangle.Bottom - rectangle.Top; j++)
					contents[i, j] = fill;
			}

			SetBufferContents(new Coordinates(rectangle.Left, rectangle.Top), contents);
		}
	}
}