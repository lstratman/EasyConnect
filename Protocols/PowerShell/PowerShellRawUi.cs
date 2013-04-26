using System;
using System.Management.Automation.Host;

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
		/// <summary>
		/// Gets or sets the background color of text to be written.
		/// This maps to the corresponding Console.Background property.
		/// </summary>
		public override ConsoleColor BackgroundColor
		{
			get
			{
				return Console.BackgroundColor;
			}
			set
			{
				Console.BackgroundColor = value;
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
				return new Size(Console.BufferWidth, Console.BufferHeight);
			}
			set
			{
				Console.SetBufferSize(value.Width, value.Height);
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
				return new Coordinates(Console.CursorLeft, Console.CursorTop);
			}
			set
			{
				Console.SetCursorPosition(value.X, value.Y);
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
				return Console.CursorSize;
			}
			set
			{
				Console.CursorSize = value;
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
				return Console.ForegroundColor;
			}
			set
			{
				Console.ForegroundColor = value;
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
				return Console.KeyAvailable;
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
				return new Size(Console.LargestWindowWidth, Console.LargestWindowHeight);
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
				return new Size(Console.LargestWindowWidth, Console.LargestWindowHeight);
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
				return new Coordinates(Console.WindowLeft, Console.WindowTop);
			}
			set
			{
				Console.SetWindowPosition(value.X, value.Y);
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
				return new Size(Console.WindowWidth, Console.WindowHeight);
			}
			set
			{
				Console.SetWindowSize(value.Width, value.Height);
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
				return Console.Title;
			}
			set
			{
				Console.Title = value;
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
			throw new NotImplementedException(
				"The method or operation is not implemented.");
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
			throw new NotImplementedException(
				"The method or operation is not implemented.");
		}
	}
}