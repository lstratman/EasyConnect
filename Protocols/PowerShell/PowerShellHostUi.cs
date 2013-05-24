using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Language;
using System.Security;
using System.Text;
using System.Threading;
using WalburySoftware;
using System.Linq;

namespace EasyConnect.Protocols.PowerShell
{
	/// <summary>
	/// A sample implementation of the PSHostUserInterface abstract class for 
	/// console applications. Not all members are implemented. Those that are 
	/// not implemented throw a NotImplementedException exception or return 
	/// nothing. Members that are implemented include those that map easily to 
	/// Console APIs and a basic implementation of the prompt API provided. 
	/// </summary>
	internal class PowerShellHostUi : PSHostUserInterface, IHostUISupportsMultipleChoiceSelection
	{
		/// <summary>
		/// A reference to the PSRawUserInterface implementation.
		/// </summary>
		private PowerShellRawUi _powerShellRawUi;

		protected TerminalControl _terminal;

		protected StringBuilder _currentInputLine = new StringBuilder();

		protected ManualResetEvent _inputSemaphore = new ManualResetEvent(false);

		protected Thread _inputThread = null;

		protected Stack<string> _upCommandHistory = new Stack<string>();
		protected Stack<string> _downCommandHistory = new Stack<string>();
		protected string _currentHistoryCommand;
		protected List<string> _intellisenseCommands = new List<string>();
		protected Dictionary<string, List<string>> _intellisenseParameters = new Dictionary<string, List<string>>();
		protected Func<string, Collection<PSObject>> _executeHelper;
		protected Thread _intellisenseThread = null;
		protected bool _readingInput = false;
		protected Thread _readingInputThread = null;

		public PowerShellHostUi(TerminalControl terminal, Func<string, Collection<PSObject>> executeHelper)
		{
			_terminal = terminal;
			_powerShellRawUi = new PowerShellRawUi(terminal);
			_executeHelper = executeHelper;
		}

		protected void GetIntellisenseCommands()
		{
			Collection<PSObject> commands = _executeHelper("get-command");

			if (commands != null)
			{
				_intellisenseCommands.AddRange(
					from command in commands
					select command.Properties["Name"].Value.ToString());
				_intellisenseCommands.Sort();
			}
		}

		public void AddToCommandHistory(string command)
		{
			if(!String.IsNullOrEmpty(_currentHistoryCommand))
				_upCommandHistory.Push(_currentHistoryCommand);

			if (!String.IsNullOrEmpty(_currentHistoryCommand) && _currentHistoryCommand == command)
			{
				_currentHistoryCommand = null;
				return;
			}

			_currentHistoryCommand = null;

			foreach (string historyCommand in _downCommandHistory)
				_upCommandHistory.Push(historyCommand);

			_downCommandHistory.Clear();
			_upCommandHistory.Push(command);

			if (_upCommandHistory.Count > 20)
				_upCommandHistory = new Stack<string>(_upCommandHistory.ToArray().Take(20).Reverse());
		}

		public bool AtCommandPrompt
		{
			get;
			set;
		}

		public void EndInput()
		{
			try
			{
				if (_inputThread != null)
					_inputThread.Abort();
			}

			catch
			{
			}

			try
			{
				if (_intellisenseThread != null)
					_intellisenseThread.Abort();
			}

			catch
			{
			}
		}

		/// <summary>
		/// Gets an instance of the PSRawUserInterface object for this host
		/// application.
		/// </summary>
		public override PSHostRawUserInterface RawUI
		{
			get
			{
				return _powerShellRawUi;
			}
		}

		public bool ReadingInput
		{
			get
			{
				return _readingInput;
			}

			protected set
			{
				_readingInput = value;
				_readingInputThread = value
					                ? Thread.CurrentThread
					                : null;
			}
		}

		/// <summary>
		/// Prompts the user for input. 
		/// <param name="caption">The caption or title of the prompt.</param>
		/// <param name="message">The text of the prompt.</param>
		/// <param name="descriptions">A collection of FieldDescription objects  
		/// that describe each field of the prompt.</param>
		/// <returns>A dictionary object that contains the results of the user 
		/// prompts.</returns>
		public override Dictionary<string, PSObject> Prompt(
			string caption,
			string message,
			Collection<FieldDescription> descriptions)
		{
			WriteLine(
				ConsoleColor.Blue,
				ConsoleColor.Black,
				caption + "\n" + message + " ");
			Dictionary<string, PSObject> results =
				new Dictionary<string, PSObject>();

			for (int i = 0; i < descriptions.Count; i++)
			{
				FieldDescription fd = descriptions[i];
				string[] label = GetHotkeyAndLabel(fd.Label);
				Write(String.Format("{0}[{1}]: ", label[1], i));
				string userData = ReadLine();
				if (userData == null)
					return null;

				results[fd.Name] = PSObject.AsPSObject(userData);
			}

			return results;
		}

		/// <summary>
		/// Provides a set of choices that enable the user to choose a 
		/// single option from a set of options. 
		/// </summary>
		/// <param name="caption">Text that proceeds (a title) the choices.</param>
		/// <param name="message">A message that describes the choice.</param>
		/// <param name="choices">A collection of ChoiceDescription objects that  
		/// describ each choice.</param>
		/// <param name="defaultChoice">The index of the label in the Choices  
		/// parameter collection. To indicate no default choice, set to -1.</param>
		/// <returns>The index of the Choices parameter collection element that 
		/// corresponds to the option that is selected by the user.</returns>
		public override int PromptForChoice(
			string caption,
			string message,
			Collection<ChoiceDescription> choices,
			int defaultChoice)
		{
			// Write the caption and message strings in Blue.
			WriteLine(
				ConsoleColor.Blue,
				ConsoleColor.Black,
				caption + "\n" + message + "\n");

			// Convert the choice collection into something that is
			// easier to work with. See the BuildHotkeysAndPlainLabels 
			// method for details.
			string[,] promptData = BuildHotkeysAndPlainLabels(choices);

			// Format the overall choice prompt string to display.
			StringBuilder sb = new StringBuilder();
			for (int element = 0; element < choices.Count; element++)
			{
				sb.Append(
					String.Format(
						CultureInfo.CurrentCulture,
						"|{0}> {1} ",
						promptData[0, element],
						promptData[1, element]));
			}

			sb.Append(
				String.Format(
					CultureInfo.CurrentCulture,
					"[Default is ({0})]",
					promptData[0, defaultChoice]));

			// Read prompts until a match is made, the default is
			// chosen, or the loop is interrupted with ctrl-C.
			while (true)
			{
				WriteLine(ConsoleColor.Cyan, ConsoleColor.Black, sb.ToString());
				string data = ReadLine().Trim().ToUpper(CultureInfo.CurrentCulture);

				// If the choice string was empty, use the default selection.
				if (data.Length == 0)
					return defaultChoice;

				// See if the selection matched and return the
				// corresponding index if it did.
				for (int i = 0; i < choices.Count; i++)
				{
					if (promptData[0, i] == data)
						return i;
				}

				WriteErrorLine("Invalid choice: " + data);
			}
		}

		/// <summary>
		/// Prompts the user for credentials with a specified prompt window 
		/// caption, prompt message, user name, and target name. In this 
		/// example this functionality is not needed so the method throws a 
		/// NotImplementException exception.
		/// </summary>
		/// <param name="caption">The caption for the message window.</param>
		/// <param name="message">The text of the message.</param>
		/// <param name="userName">The user name whose credential is to be 
		/// prompted for.</param>
		/// <param name="targetName">The name of the target for which the 
		/// credential is collected.</param>
		/// <returns>Throws a NotImplementedException exception.</returns>
		public override PSCredential PromptForCredential(
			string caption,
			string message,
			string userName,
			string targetName)
		{
			throw new NotImplementedException(
				"The method or operation is not implemented.");
		}

		/// <summary>
		/// Prompts the user for credentials by using a specified prompt window 
		/// caption, prompt message, user name and target name, credential 
		/// types allowed to be returned, and UI behavior options. In this 
		/// example this functionality is not needed so the method throws a 
		/// NotImplementException exception.
		/// </summary>
		/// <param name="caption">The caption for the message window.</param>
		/// <param name="message">The text of the message.</param>
		/// <param name="userName">The user name whose credential is to be 
		/// prompted for.</param>
		/// <param name="targetName">The name of the target for which the 
		/// credential is collected.</param>
		/// <param name="allowedCredentialTypes">A PSCredentialTypes constant  
		/// that identifies the type of credentials that can be returned.</param>
		/// <param name="options">A PSCredentialUIOptions constant that 
		/// identifies the UI behavior when it gathers the credentials.</param>
		/// <returns>Throws a NotImplementedException exception.</returns>
		public override PSCredential PromptForCredential(
			string caption,
			string message,
			string userName,
			string targetName,
			PSCredentialTypes allowedCredentialTypes,
			PSCredentialUIOptions options)
		{
			throw new NotImplementedException(
				"The method or operation is not implemented.");
		}

		/// <summary>
		/// Reads characters that are entered by the user until a newline 
		/// (carriage return) is encountered.
		/// </summary>
		/// <returns>The characters that are entered by the user.</returns>
		public override string ReadLine()
		{
			try
			{
				ReadingInput = true;

				if (_intellisenseThread == null)
				{
					_intellisenseThread = new Thread(GetIntellisenseCommands)
						                      {
							                      Name = "PowerShellHostUi Intellisense Thread"
						                      };
					_intellisenseThread.Start();
				}

				StreamConnection connection = _terminal.TerminalPane.ConnectionTag.Connection as StreamConnection;

				connection.Capture = true;
				_currentInputLine.Clear();
				_inputSemaphore.Reset();

				_inputThread = new Thread(ReadInput)
					               {
						               Name = "PowerShellHostUi Input Thread"
					               };
				_inputThread.Start(connection);

				_inputSemaphore.WaitOne();
				connection.Capture = false;

				return _currentInputLine.ToString();
			}

			finally
			{
				ReadingInput = false;
			}
		}

		private void ReadInput(object state)
		{
			StreamConnection connection = state as StreamConnection;
			bool foundCarriageReturn = false;
			Coordinates promptStart = RawUI.CursorPosition;
			Coordinates promptEnd = RawUI.CursorPosition;
			bool inEscapeSequence = false;
			int insertPosition = 0;
			int? intellisenseStartLocation = null;
			List<string> intellisenseCandidates = new List<string>();
			int? intellisenseCandidatesIndex = null;
			string currentIntellisenseCommand = "";

			while (!foundCarriageReturn)
			{
				if (connection.OutputQueue.Count > 0)
				{
					while (connection.OutputQueue.Count > 0)
					{
						byte currentByte = connection.OutputQueue.Dequeue();

						if (currentByte == 9)
						{
							if (intellisenseStartLocation == null)
							{
								Token[] tokens;
								ParseError[] parseErrors;

								Parser.ParseInput(_currentInputLine.ToString().Substring(0, insertPosition), out tokens, out parseErrors);

								Token currentToken = tokens.LastOrDefault(t => t.Kind != TokenKind.EndOfInput);

								if (currentToken != null && !String.IsNullOrEmpty(currentToken.Text))
								{
									if (((currentToken.Kind == TokenKind.Generic || currentToken.Kind == TokenKind.Identifier) && currentToken.TokenFlags == TokenFlags.CommandName) || currentToken.Kind == TokenKind.Parameter || (currentToken.Kind == TokenKind.Generic && (currentToken.TokenFlags & TokenFlags.BinaryPrecedenceAdd) == TokenFlags.BinaryPrecedenceAdd) || (currentToken.Kind == TokenKind.Identifier && currentToken.TokenFlags == TokenFlags.None) || (currentToken.Kind == TokenKind.Generic && currentToken.TokenFlags == TokenFlags.None) || currentToken.Kind == TokenKind.StringLiteral)
									{
										intellisenseStartLocation = insertPosition - currentToken.Text.Length;
										intellisenseCandidatesIndex = -1;
										currentIntellisenseCommand = _currentInputLine.ToString().Substring(intellisenseStartLocation.Value, insertPosition - intellisenseStartLocation.Value);

										if (((currentToken.Kind == TokenKind.Generic || currentToken.Kind == TokenKind.Identifier) && currentToken.TokenFlags == TokenFlags.CommandName) || (currentToken.Kind == TokenKind.Identifier && currentToken.TokenFlags == TokenFlags.None) || (currentToken.Kind == TokenKind.Generic && currentToken.TokenFlags == TokenFlags.None) || currentToken.Kind == TokenKind.StringLiteral)
										{
											string match = currentToken.Text.Replace("'", "").Replace("\"", "");
											string searchPath = null;

											if (match.Contains("\\"))
											{
												searchPath = match.Substring(0, match.LastIndexOf("\\") + 1);
												match = match.Substring(match.LastIndexOf("\\") + 1);
											}

											IEnumerable<string> childItems = _executeHelper(
												"get-childitem -Filter '" + match + "*'" + (!String.IsNullOrEmpty(searchPath)
													                                          ? " -Path '" + searchPath + "'"
													                                          : "")).Select(
														                                          i => String.IsNullOrEmpty(searchPath)
															                                               ? ".\\" + i.ToString()
															                                               : searchPath + i.ToString());

											intellisenseCandidates =
												(((currentToken.Kind == TokenKind.Generic || currentToken.Kind == TokenKind.Identifier) && currentToken.TokenFlags == TokenFlags.CommandName)
													 ? _intellisenseCommands.Where(c => c.ToLower().StartsWith(currentToken.Text.ToLower()))
													 : new List<string>()).Union(childItems).Select(
														 c => c.Contains(" ")
															      ? "'" + c + "'"
															      : c).OrderBy(c => c).ToList();
										}

										else if (currentToken.Kind == TokenKind.Parameter ||
												 (currentToken.Kind == TokenKind.Generic && (currentToken.TokenFlags & TokenFlags.BinaryPrecedenceAdd) == TokenFlags.BinaryPrecedenceAdd))
										{
											if (tokens.ToList().IndexOf(currentToken) > 0)
											{
												Token commandToken = tokens[tokens.ToList().IndexOf(currentToken) - 1];

												if ((currentToken.Kind == TokenKind.Generic || currentToken.Kind == TokenKind.Identifier) && commandToken.TokenFlags == TokenFlags.CommandName)
												{
													if (!_intellisenseParameters.ContainsKey(commandToken.Text.ToLower()))
													{
														_intellisenseParameters[commandToken.Text.ToLower()] = new List<string>();

														Collection<PSObject> command = _executeHelper("get-command " + commandToken.Text);

														if (command != null)
															_intellisenseParameters[commandToken.Text.ToLower()].AddRange(
																from parameter in (command[0].Properties["Parameters"].Value as Dictionary<string, ParameterMetadata>).Keys
																select "-" + parameter);
													}

													intellisenseCandidates =
														_intellisenseParameters[commandToken.Text.ToLower()].Where(p => p.ToLower().StartsWith(currentToken.Text.ToLower())).OrderBy(p => p).ToList();
												}
											}

											if (intellisenseCandidates == null)
												intellisenseCandidates = new List<string>();
										}
									}
								}
							}

							if (intellisenseStartLocation != null && intellisenseCandidates.Count > 0)
							{
								intellisenseCandidatesIndex++;

								if (intellisenseCandidatesIndex >= intellisenseCandidates.Count)
									intellisenseCandidatesIndex = 0;

								string command = intellisenseCandidates[intellisenseCandidatesIndex.Value];

								_currentInputLine.Remove(intellisenseStartLocation.Value, currentIntellisenseCommand.Length);
								_currentInputLine.Insert(intellisenseStartLocation.Value, command);

								RawUI.CursorPosition = promptStart;
								Write(
									_currentInputLine + (currentIntellisenseCommand.Length > command.Length
										                     ? new string(' ', currentIntellisenseCommand.Length - command.Length)
										                     : ""));
								promptEnd = RawUI.CursorPosition;

								RawUI.CursorPosition = new Coordinates(
									(promptStart.X + intellisenseStartLocation.Value + command.Length) % RawUI.BufferSize.Width,
									promptStart.Y + Convert.ToInt32(Math.Floor((promptStart.X + intellisenseStartLocation.Value + command.Length) / (double)RawUI.BufferSize.Width)));

								insertPosition = intellisenseStartLocation.Value + command.Length;
								currentIntellisenseCommand = command;
							}
						}

						else if (intellisenseStartLocation != null)
						{
							intellisenseStartLocation = null;
							intellisenseCandidatesIndex = null;
							currentIntellisenseCommand = null;
							intellisenseCandidates.Clear();
						}

						if (currentByte == 8)
						{
							Coordinates currentPosition = RawUI.CursorPosition;

							if (insertPosition > 0)
							{
								_currentInputLine.Remove(insertPosition - 1, 1);
								insertPosition--;

								currentPosition = currentPosition.X > 1
									                  ? new Coordinates(currentPosition.X - 1, currentPosition.Y)
									                  : new Coordinates(RawUI.BufferSize.Width, currentPosition.Y - 1);

								RawUI.CursorPosition = currentPosition;
								Write(_currentInputLine.ToString(insertPosition, _currentInputLine.Length - insertPosition));
								Write(" ");
								RawUI.CursorPosition = currentPosition;

								promptEnd = promptEnd.X > 1
										? new Coordinates(promptEnd.X - 1, promptEnd.Y)
										: new Coordinates(RawUI.BufferSize.Width, promptEnd.Y - 1);
							}
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
							RawUI.CursorPosition = promptStart;
							insertPosition = 0;
						}

						else if (currentByte == 56 && inEscapeSequence)
						{
							RawUI.CursorPosition = promptEnd;
							insertPosition = _currentInputLine.Length;
						}

						else if (currentByte == 68 && inEscapeSequence)
						{
							Coordinates currentPosition = RawUI.CursorPosition;

							if (RawUI.CursorPosition != promptStart)
							{
								RawUI.CursorPosition = currentPosition.X > 1
															? new Coordinates(currentPosition.X - 1, currentPosition.Y)
															: new Coordinates(RawUI.BufferSize.Width, currentPosition.Y - 1);
								insertPosition--;
							}

							inEscapeSequence = false;
						}

						else if (currentByte == 67 && inEscapeSequence)
						{
							Coordinates currentPosition = RawUI.CursorPosition;

							if (RawUI.CursorPosition != promptEnd)
							{
								RawUI.CursorPosition = currentPosition.X < RawUI.BufferSize.Width
															? new Coordinates(currentPosition.X + 1, currentPosition.Y)
															: new Coordinates(1, currentPosition.Y + 1);
								insertPosition++;
							}

							inEscapeSequence = false;
						}

						else if (currentByte == 51 && inEscapeSequence)
						{
							Coordinates currentPosition = RawUI.CursorPosition;

							if (RawUI.CursorPosition != promptEnd)
							{
								_currentInputLine.Remove(insertPosition, 1);

								if (insertPosition < _currentInputLine.Length)
								{
									Write(_currentInputLine.ToString(insertPosition, _currentInputLine.Length - insertPosition));
									Write(" ");
									RawUI.CursorPosition = currentPosition;

									promptEnd = promptEnd.X > 1
											? new Coordinates(promptEnd.X - 1, promptEnd.Y)
											: new Coordinates(RawUI.BufferSize.Width, promptEnd.Y - 1);
								}
							}

							inEscapeSequence = false;
						}

						else if (currentByte == 65 && inEscapeSequence)
						{
							if (AtCommandPrompt && _upCommandHistory.Count > 0)
							{
								if (!String.IsNullOrEmpty(_currentHistoryCommand))
									_downCommandHistory.Push(_currentHistoryCommand);

								_currentHistoryCommand = _upCommandHistory.Pop();

								RawUI.CursorPosition = promptStart;
								Write(new string(' ', _currentInputLine.Length));
								RawUI.CursorPosition = promptStart;
								Write(_currentHistoryCommand);
								promptEnd = RawUI.CursorPosition;
								_currentInputLine = new StringBuilder(_currentHistoryCommand);
								insertPosition = _currentInputLine.Length;
							}

							inEscapeSequence = false;
						}

						else if (currentByte == 66 && inEscapeSequence)
						{
							if (AtCommandPrompt && _downCommandHistory.Count > 0)
							{
								if (!String.IsNullOrEmpty(_currentHistoryCommand))
									_upCommandHistory.Push(_currentHistoryCommand);

								_currentHistoryCommand = _downCommandHistory.Pop();

								RawUI.CursorPosition = promptStart;
								Write(new string(' ', _currentInputLine.Length));
								RawUI.CursorPosition = promptStart;
								Write(_currentHistoryCommand);
								promptEnd = RawUI.CursorPosition;
								_currentInputLine = new StringBuilder(_currentHistoryCommand);
								insertPosition = _currentInputLine.Length;
							}

							inEscapeSequence = false;
						}

						else if (currentByte == 13)
						{
							WriteLine();
							foundCarriageReturn = true;

							break;
						}

						else if (currentByte >= 32)
						{
							inEscapeSequence = false;

							Write(
								Encoding.UTF8.GetString(
									new byte[]
										{
											currentByte
										}));
							_currentInputLine.Insert(
								insertPosition, Encoding.UTF8.GetString(
									new byte[]
										{
											currentByte
										}));
							insertPosition++;

							if (insertPosition < _currentInputLine.Length)
							{
								Coordinates currentPosition = RawUI.CursorPosition;
								Write(_currentInputLine.ToString(insertPosition, _currentInputLine.Length - insertPosition));
								RawUI.CursorPosition = currentPosition;
							}

							promptEnd = promptEnd.X < RawUI.BufferSize.Width
											? new Coordinates(promptEnd.X + 1, promptEnd.Y)
											: new Coordinates(1, promptEnd.Y + 1);
						}

						else
							inEscapeSequence = false;
					}
				}

				if (!foundCarriageReturn)
					Thread.Sleep(50);
			}

			_inputSemaphore.Set();
		}

		/// <summary>
		/// Reads characters entered by the user until a newline (carriage return) 
		/// is encountered and returns the characters as a secure string. In this 
		/// example this functionality is not needed so the method throws a 
		/// NotImplementException exception.
		/// </summary>
		/// <returns>Throws a NotImplemented exception.</returns>
		public override SecureString ReadLineAsSecureString()
		{
			SecureString password = new SecureString();
			ConsoleKeyInfo key = new ConsoleKeyInfo();

			do
			{
				if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
					password.AppendChar(key.KeyChar);

				else
				{
					if (key.Key == ConsoleKey.Backspace && password.Length > 0)
						password.RemoveAt(password.Length - 1);
				}
			} while (key.Key != ConsoleKey.Enter);

			WriteLine();
			return password;
		}

		/// <summary>
		/// Writes characters to the output display of the host.
		/// </summary>
		/// <param name="value">The characters to be written.</param>
		public override void Write(string value)
		{
			byte[] buffer = Encoding.UTF8.GetBytes(value.Replace("\n", "\x001BE"));
			_terminal.TerminalPane.ConnectionTag.Receiver.DataArrived(buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Writes characters to the output display of the host with possible 
		/// foreground and background colors. 
		/// </summary>
		/// <param name="foregroundColor">The color of the characters.</param>
		/// <param name="backgroundColor">The backgound color to use.</param>
		/// <param name="value">The characters to be written.</param>
		public override void Write(
			ConsoleColor foregroundColor,
			ConsoleColor backgroundColor,
			string value)
		{
			RawUI.ForegroundColor = foregroundColor;
			RawUI.BackgroundColor = backgroundColor;
			Write(value);
			(RawUI as PowerShellRawUi).RestoreForegroundColor();
			(RawUI as PowerShellRawUi).RestoreBackgroundColor();
		}

		/// <summary>
		/// Writes a line of characters to the output display of the host 
		/// with foreground and background colors and appends a newline (carriage return). 
		/// </summary>
		/// <param name="foregroundColor">The forground color of the display. </param>
		/// <param name="backgroundColor">The background color of the display. </param>
		/// <param name="value">The line to be written.</param>
		public override void WriteLine(
			ConsoleColor foregroundColor,
			ConsoleColor backgroundColor,
			string value)
		{
			Write(foregroundColor, backgroundColor, value + "\n");
		}

		/// <summary>
		/// Writes a debug message to the output display of the host.
		/// </summary>
		/// <param name="message">The debug message that is displayed.</param>
		public override void WriteDebugLine(string message)
		{
			WriteLine(
				ConsoleColor.DarkYellow,
				ConsoleColor.Black,
				String.Format(CultureInfo.CurrentCulture, "DEBUG: {0}", message));
		}

		/// <summary>
		/// Writes an error message to the output display of the host.
		/// </summary>
		/// <param name="value">The error message that is displayed.</param>
		public override void WriteErrorLine(string value)
		{
			WriteLine(
				ConsoleColor.Red,
				ConsoleColor.Black,
				value);
		}

		/// <summary>
		/// Writes a newline character (carriage return) 
		/// to the output display of the host. 
		/// </summary>
		public override void WriteLine()
		{
			WriteLine("");
		}

		/// <summary>
		/// Writes a line of characters to the output display of the host 
		/// and appends a newline character(carriage return). 
		/// </summary>
		/// <param name="value">The line to be written.</param>
		public override void WriteLine(string value)
		{
			Write(value + "\n");
		}

		/// <summary>
		/// Writes a progress report to the output display of the host.
		/// </summary>
		/// <param name="sourceId">Unique identifier of the source of the record. </param>
		/// <param name="record">A ProgressReport object.</param>
		public override void WriteProgress(long sourceId, ProgressRecord record)
		{
		}

		/// <summary>
		/// Writes a verbose message to the output display of the host.
		/// </summary>
		/// <param name="message">The verbose message that is displayed.</param>
		public override void WriteVerboseLine(string message)
		{
			WriteLine(
				ConsoleColor.Green,
				ConsoleColor.Black,
				String.Format(CultureInfo.CurrentCulture, "VERBOSE: {0}", message));
		}

		/// <summary>
		/// Writes a warning message to the output display of the host.
		/// </summary>
		/// <param name="message">The warning message that is displayed.</param>
		public override void WriteWarningLine(string message)
		{
			WriteLine(
				ConsoleColor.Yellow,
				ConsoleColor.Black,
				String.Format(CultureInfo.CurrentCulture, "WARNING: {0}", message));
		}

		/// <summary>
		/// Parse a string containing a hotkey character.
		/// Take a string of the form
		///    Yes to &amp;all
		/// and returns a two-dimensional array split out as
		///    "A", "Yes to all".
		/// </summary>
		/// <param name="input">The string to process</param>
		/// <returns>
		/// A two dimensional array containing the parsed components.
		/// </returns>
		private static string[] GetHotkeyAndLabel(string input)
		{
			string[] result = new string[]
				                  {
					                  String.Empty, String.Empty
				                  };
			string[] fragments = input.Split('&');
			if (fragments.Length == 2)
			{
				if (fragments[1].Length > 0)
				{
					result[0] = fragments[1][0].ToString().
					                            ToUpper(CultureInfo.CurrentCulture);
				}

				result[1] = (fragments[0] + fragments[1]).Trim();
			}
			else
				result[1] = input;

			return result;
		}

		/// <summary>
		/// This is a private worker function splits out the
		/// accelerator keys from the menu and builds a two
		/// dimentional array with the first access containing the
		/// accelerator and the second containing the label string
		/// with the &amp; removed.
		/// </summary>
		/// <param name="choices">The choice collection to process</param>
		/// <returns>
		/// A two dimensional array containing the accelerator characters
		/// and the cleaned-up labels</returns>
		private static string[,] BuildHotkeysAndPlainLabels(
			Collection<ChoiceDescription> choices)
		{
			// Allocate the result array
			string[,] hotkeysAndPlainLabels = new string[2,choices.Count];

			for (int i = 0; i < choices.Count; ++i)
			{
				string[] hotkeyAndLabel = GetHotkeyAndLabel(choices[i].Label);
				hotkeysAndPlainLabels[0, i] = hotkeyAndLabel[0];
				hotkeysAndPlainLabels[1, i] = hotkeyAndLabel[1];
			}

			return hotkeysAndPlainLabels;
		}

		#region IHostUISupportsMultipleChoiceSelection Members
		/// <summary>
		/// Provides a set of choices that enable the user to choose a one or 
		/// more options from a set of options. 
		/// </summary>
		/// <param name="caption">Text that proceeds (a title) the choices.</param>
		/// <param name="message">A message that describes the choice.</param>
		/// <param name="choices">A collection of ChoiceDescription objects that  
		/// describ each choice.</param>
		/// <param name="defaultChoices">The index of the label in the Choices  
		/// parameter collection. To indicate no default choice, set to -1.</param>
		/// <returns>The index of the Choices parameter collection element that 
		/// corresponds to the option that is selected by the user.</returns>
		public Collection<int> PromptForChoice(
			string caption,
			string message,
			Collection<ChoiceDescription> choices,
			IEnumerable<int> defaultChoices)
		{
			// Write the caption and message strings in Blue.
			WriteLine(
				ConsoleColor.Blue,
				ConsoleColor.Black,
				caption + "\n" + message + "\n");

			// Convert the choice collection into something that is 
			// easier to work with. See the BuildHotkeysAndPlainLabels  
			// method for details.
			string[,] promptData = BuildHotkeysAndPlainLabels(choices);

			// Format the overall choice prompt string to display.
			StringBuilder sb = new StringBuilder();
			for (int element = 0; element < choices.Count; element++)
			{
				sb.Append(
					String.Format(
						CultureInfo.CurrentCulture,
						"|{0}> {1} ",
						promptData[0, element],
						promptData[1, element]));
			}

			Collection<int> defaultResults = new Collection<int>();
			if (defaultChoices != null)
			{
				int countDefaults = 0;
				foreach (int defaultChoice in defaultChoices)
				{
					++countDefaults;
					defaultResults.Add(defaultChoice);
				}

				if (countDefaults != 0)
				{
					sb.Append(
						countDefaults == 1
							? "[Default choice is "
							: "[Default choices are ");
					foreach (int defaultChoice in defaultChoices)
					{
						sb.AppendFormat(
							CultureInfo.CurrentCulture,
							"\"{0}\",",
							promptData[0, defaultChoice]);
					}

					sb.Remove(sb.Length - 1, 1);
					sb.Append("]");
				}
			}

			WriteLine(
				ConsoleColor.Cyan,
				ConsoleColor.Black,
				sb.ToString());

			// Read prompts until a match is made, the default is
			// chosen, or the loop is interrupted with ctrl-C.
			Collection<int> results = new Collection<int>();
			while (true)
			{
				ReadNext:
				string prompt = string.Format(CultureInfo.CurrentCulture, "Choice[{0}]:", results.Count);
				Write(ConsoleColor.Cyan, ConsoleColor.Black, prompt);
				string data = ReadLine().Trim().ToUpper(CultureInfo.CurrentCulture);

				// If the choice string was empty, no more choices have been made.
				// If there were no choices made, return the defaults
				if (data.Length == 0)
				{
					return (results.Count == 0)
								? defaultResults
								: results;
				}

				// See if the selection matched and return the
				// corresponding index if it did.
				for (int i = 0; i < choices.Count; i++)
				{
					if (promptData[0, i] == data)
					{
						results.Add(i);
						goto ReadNext;
					}
				}

				WriteErrorLine("Invalid choice: " + data);
			}
		}
		#endregion

		public void StopCurrentPipeline()
		{
			try
			{
				_readingInputThread.Abort();
			}

			catch (Exception)
			{
			}

			ReadingInput = false;
			EndInput();
		}
	}
}