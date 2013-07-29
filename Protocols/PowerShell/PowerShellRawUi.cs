using System;
using System.Drawing;
using System.Management.Automation.Host;
using System.Text;
using System.Threading;
using Poderosa.Communication;
using Poderosa.Terminal;
using Poderosa.Text;
using WalburySoftware;
using Win32Interop.Methods;
using Rectangle = System.Management.Automation.Host.Rectangle;
using Size = System.Management.Automation.Host.Size;

namespace EasyConnect.Protocols.PowerShell
{
	/// <summary>
	/// Interfaces between <see cref="PowerShellHost"/> and <see cref="PowerShellHostUi"/> and the terminal and provides raw access to the console itself.
	/// </summary>
	public class PowerShellRawUi : PSHostRawUserInterface
	{
		/// <summary>
		/// Background color of the console.
		/// </summary>
		protected ConsoleColor _backgroundColor;

		/// <summary>
		/// Text color of the console.
		/// </summary>
		protected ConsoleColor _foregroundColor;

		/// <summary>
		/// Semaphore used to signal when input has been entered by the user.  Used by <see cref="ReadKey"/>.
		/// </summary>
		protected ManualResetEvent _inputSemaphore = new ManualResetEvent(false);

		/// <summary>
		/// Thread used to watch for input from the user in <see cref="ReadKey"/>.
		/// </summary>
		protected Thread _inputThread;

		/// <summary>
		/// Info for the key that was pressed (for <see cref="ReadKey"/>).
		/// </summary>
		protected KeyInfo _readKey;

		/// <summary>
		/// Terminal control used to display the shell.
		/// </summary>
		protected TerminalControl _terminal;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="terminal">Terminal control that will display the PowerShell console.</param>
		public PowerShellRawUi(TerminalControl terminal)
		{
			_terminal = terminal;
			_backgroundColor = ClosestConsoleColor(_terminal.TerminalPane.ConnectionTag.RenderProfile.BackColor);
			_foregroundColor = ClosestConsoleColor(_terminal.TerminalPane.ConnectionTag.RenderProfile.ForeColor);
		}

		/// <summary>
		/// Gets or sets the background color of text to be written.
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

				// Send the appropriate ANSI sequence to the terminal
				byte[] data = Encoding.UTF8.GetBytes("\x001B[" + commandValue + "m");
				_terminal.TerminalPane.ConnectionTag.Receiver.DataArrived(data, 0, data.Length);
			}
		}

		/// <summary>
		/// Gets or sets the host buffer size.  Maps to <see cref="TerminalConnection.TerminalWidth"/> and <see cref="TerminalDocument.Size"/>.
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
		/// Gets or sets the cursor position in the terminal.
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

				// Send the appropriate ANSI sequence to the terminal to set the cursor position
				byte[] data = Encoding.UTF8.GetBytes(String.Format("\x001B[{1};{0}f", relativePosition.X, relativePosition.Y));
				_terminal.TerminalPane.ConnectionTag.Receiver.DataArrived(data, 0, data.Length);
			}
		}

		/// <summary>
		/// Gets or sets the cursor size.  This is simply <see cref="RenderProfile.FontSize"/>.
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

				// Send the appropriate ANSI sequence to the terminal
				byte[] data = Encoding.UTF8.GetBytes("\x001B[" + commandValue + "m");
				_terminal.TerminalPane.ConnectionTag.Receiver.DataArrived(data, 0, data.Length);
			}
		}

		/// <summary>
		/// Gets a value indicating whether a key is available.  Simply checks to see if <see cref="ITerminal.TerminalMode"/> is equal to
		/// <see cref="TerminalMode.Normal"/>.
		/// </summary>
		public override bool KeyAvailable
		{
			get
			{
				return _terminal.TerminalPane.ConnectionTag.Terminal.TerminalMode == TerminalMode.Normal;
			}
		}

		/// <summary>
		/// Gets the maximum physical size of the window by simply returning <see cref="WindowSize"/>.
		/// </summary>
		public override Size MaxPhysicalWindowSize
		{
			get
			{
				return WindowSize;
			}
		}

		/// <summary>
		/// Gets the maximum window size by simply returning <see cref="WindowSize"/>.
		/// </summary>
		public override Size MaxWindowSize
		{
			get
			{
				return WindowSize;
			}
		}

		/// <summary>
		/// Gets or sets the window position, which is the currently visible portion of the buffer.
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
		/// Gets or sets the window size by using <see cref="TerminalConnection.TerminalWidth"/> and <see cref="TerminalConnection.TerminalHeight"/>.
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
		/// Gets or sets the title of the window.  Unused, so we simply return an empty string.
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
		/// Finds the closest corresponding <see cref="ConsoleColor"/> value to the given <paramref name="color"/>.
		/// </summary>
		/// <param name="color">Color that we are trying to match.</param>
		/// <returns>The closest corresponding <see cref="ConsoleColor"/> value to the given <paramref name="color"/>.</returns>
		protected ConsoleColor ClosestConsoleColor(Color color)
		{
			ConsoleColor returnColor = 0;
			double r = color.R, g = color.G, b = color.B, delta = double.MaxValue;

			foreach (ConsoleColor consoleColor in Enum.GetValues(typeof (ConsoleColor)))
			{
				Color consoleColorColor = Color.FromName(
					consoleColor.ToString("G") == "DarkYellow"
						? "Orange"
						: consoleColor.ToString("G"));
				double t = Math.Pow(consoleColorColor.R - r, 2.0) + Math.Pow(consoleColorColor.G - g, 2.0) + Math.Pow(consoleColorColor.B - b, 2.0);

				// ReSharper disable CompareOfFloatsByEqualityOperator
				if (t == 0)
					// ReSharper restore CompareOfFloatsByEqualityOperator
					return consoleColor;

				if (t < delta)
				{
					delta = t;
					returnColor = consoleColor;
				}
			}

			return returnColor;
		}

		/// <summary>
		/// Resets the console's text color to its default value.
		/// </summary>
		public void RestoreForegroundColor()
		{
			byte[] data = Encoding.UTF8.GetBytes("\x001B[39m");
			_terminal.TerminalPane.ConnectionTag.Receiver.DataArrived(data, 0, data.Length);
		}

		/// <summary>
		/// Resets the console's background color to its default value.
		/// </summary>
		public void RestoreBackgroundColor()
		{
			byte[] data = Encoding.UTF8.GetBytes("\x001B[49m");
			_terminal.TerminalPane.ConnectionTag.Receiver.DataArrived(data, 0, data.Length);
		}

		/// <summary>
		/// This API resets the input buffer.
		/// </summary>
		public override void FlushInputBuffer()
		{
		}

		/// <summary>
		/// This API returns a rectangular region of the screen buffer. In this case, this functionality is not needed so the method throws a 
		/// <see cref="NotImplementedException"/> exception.
		/// </summary>
		/// <param name="rectangle">Defines the size of the rectangle.</param>
		/// <returns>Throws a <see cref="NotImplementedException"/> exception.</returns>
		public override BufferCell[,] GetBufferContents(Rectangle rectangle)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// This API Reads a pressed, released, or pressed and released keystroke from the keyboard device, blocking processing until a keystroke is typed that 
		/// matches the specified keystroke options.
		/// </summary>
		/// <param name="options">Options, such as IncludeKeyDown, used when reading the keyboard.</param>
		/// <returns>Data for the key that the user pressed.</returns>
		public override KeyInfo ReadKey(ReadKeyOptions options)
		{
			StreamConnection connection = _terminal.TerminalPane.ConnectionTag.Connection as StreamConnection;
			connection.Capture = true;
			_inputSemaphore.Reset();

			// Start up a thread to watch the terminal's input bufer
			_inputThread = new Thread(ReadInput)
				               {
					               Name = "PowerShellRawUi Input Thread"
				               };
			_inputThread.Start(new Tuple<StreamConnection, ReadKeyOptions>(connection, options));

			// ReadInput will signal through this semaphore when a key has been pressed
			_inputSemaphore.WaitOne();
			connection.Capture = false;

			return _readKey;
		}

		/// <summary>
		/// Thread method invoked from <see cref="ReadKey"/> that waits for the user to press a key.
		/// </summary>
		/// <param name="state"><see cref="Tuple{T1, T2}"/> object containing a <see cref="StreamConnection"/> object and a <see cref="ReadKeyOptions"/>
		/// object.</param>
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

						// Handle the backspace key
						if (currentByte == 8)
						{
							_readKey = new KeyInfo
								           {
									           VirtualKeyCode = 8
								           };

							// If we're not already at the beginning of the line and we're echoing the output, move the cursor left
							if ((options & ReadKeyOptions.NoEcho) != ReadKeyOptions.NoEcho && CursorPosition.X > 1)
								CursorPosition = new Coordinates(CursorPosition.X - 1, CursorPosition.Y);

							readKey = true;
							break;
						}

							// The ^X character signifies the start of an ANSI escape sequence
						else if (currentByte == 27)
							inEscapeSequence = true;

							// If we're in an escape sequence, read past the "[" and "~" characters
						else if (currentByte == 91 && inEscapeSequence)
						{
						}

						else if (currentByte == 126 && inEscapeSequence)
						{
						}

							// ^X7 is the home key
						else if (currentByte == 55 && inEscapeSequence)
						{
							_readKey = new KeyInfo
								           {
									           VirtualKeyCode = 0x24
								           };

							// If we're not already at the beginning of the line and we're echoing the output, move the cursor to the start of the line
							if ((options & ReadKeyOptions.NoEcho) != ReadKeyOptions.NoEcho)
								CursorPosition = new Coordinates(1, CursorPosition.Y);

							readKey = true;
							break;
						}

							// ^X8 or ^X3 is the end key
						else if ((currentByte == 56 || currentByte == 51) && inEscapeSequence)
						{
							_readKey = new KeyInfo
								           {
									           VirtualKeyCode = 0x23
								           };

							// If we're not already at the beginning of the line and we're echoing the output, move the cursor to the end of the line
							if ((options & ReadKeyOptions.NoEcho) != ReadKeyOptions.NoEcho)
								CursorPosition = new Coordinates(BufferSize.Width, CursorPosition.Y);

							readKey = true;
							break;
						}

							// ^XD is the left arrow
						else if (currentByte == 68 && inEscapeSequence)
						{
							_readKey = new KeyInfo
								           {
									           VirtualKeyCode = 0x25
								           };

							// If we're not already at the beginning of the line and we're echoing the output, move the cursor left
							if ((options & ReadKeyOptions.NoEcho) != ReadKeyOptions.NoEcho && CursorPosition.X > 1)
								CursorPosition = new Coordinates(CursorPosition.X - 1, CursorPosition.Y);

							readKey = true;
							break;
						}

							// ^XC is the right arrow
						else if (currentByte == 67 && inEscapeSequence)
						{
							_readKey = new KeyInfo
								           {
									           VirtualKeyCode = 0x27
								           };

							// If we're not already at the beginning of the line and we're echoing the output, move the cursor right
							if ((options & ReadKeyOptions.NoEcho) != ReadKeyOptions.NoEcho && CursorPosition.X < BufferSize.Width)
								CursorPosition = new Coordinates(CursorPosition.X + 1, CursorPosition.Y);

							readKey = true;
							break;
						}

							// Handle the carriage return sequence
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

							// Otherwise, get the virtual key code and character and populate _readKey
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
							{
								_terminal.TerminalPane.ConnectionTag.Receiver.DataArrived(
									new byte[]
										{
											currentByte
										}, 0, 1);
							}

							readKey = true;
							break;
						}
					}
				}

				// If we didn't read a key, sleep for a bit
				if (!readKey)
					Thread.Sleep(50);
			}

			// Signal to ReadKey() that we've read a key
			_inputSemaphore.Set();
		}

		/// <summary>
		/// This API crops a region of the screen buffer. In this case, this functionality is not needed so the method throws a 
		/// <see cref="NotImplementedException"/> exception.
		/// </summary>
		/// <param name="source">The region of the screen to be scrolled.</param>
		/// <param name="destination">The region of the screen to receive the source region contents.</param>
		/// <param name="clip">The region of the screen to include in the operation.</param>
		/// <param name="fill">The character and attributes to be used to fill all cell.</param>
		public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		/// <summary>
		/// This API copies an array of buffer cells into the screen buffer at a specified location. In this case, this functionality is not needed so the 
		/// method throws a <see cref="NotImplementedException"/> exception.
		/// </summary>
		/// <param name="origin">Coordinates on the screen to start writing.</param>
		/// <param name="contents">Data to write to the screen.</param>
		public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		/// <summary>
		/// This API copies a given character, foreground color, and background color to a region of the screen buffer. In this case, this functionality is not 
		/// needed so the method throws a <see cref="NotImplementedException"/> exception.
		/// </summary>
		/// <param name="rectangle">Defines the area to be filled.</param>
		/// <param name="fill">Defines the fill character.</param>
		public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
		{
			BufferCell[,] contents = new BufferCell[rectangle.Right - rectangle.Left,rectangle.Bottom - rectangle.Top];

			for (int i = 0; i < rectangle.Right - rectangle.Left; i++)
			{
				for (int j = 0; j < rectangle.Bottom - rectangle.Top; j++)
					contents[i, j] = fill;
			}

			SetBufferContents(new Coordinates(rectangle.Left, rectangle.Top), contents);
		}
	}
}