using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Language;
using System.Security;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using Poderosa.Terminal;
using Timer = System.Timers.Timer;

namespace EasyConnect.Protocols.PowerShell
{
	/// <summary>
	/// UI used to display the PowerShell console through a <see cref="TerminalControl"/>.
	/// </summary>
	public class PowerShellHostUi : PSHostUserInterface, IHostUISupportsMultipleChoiceSelection
	{
		/// <summary>
		/// Current command, if any, from the history buffer that is being displayed in the prompt.
		/// </summary>
		protected string _currentHistoryCommand;

	    protected int _currentInputLineMaxLength = 0;

		/// <summary>
		/// Current line of text that the user has entered at the command prompt.
		/// </summary>
		protected StringBuilder _currentInputLine = new StringBuilder();

		/// <summary>
		/// List of commands that were executed after the current history entry.
		/// </summary>
		protected Stack<string> _downCommandHistory = new Stack<string>();

		/// <summary>
		/// Method used to execute PowerShell commands within the current session.
		/// </summary>
		protected Func<string, Collection<PSObject>> _executeHelper;

		/// <summary>
		/// Semaphore used to signal when input has been entered by the user.
		/// </summary>
		protected ManualResetEvent _inputSemaphore = new ManualResetEvent(false);

		/// <summary>
		/// Thread that is capturing user input via <see cref="ReadLine"/>.
		/// </summary>
		protected Thread _inputThread = null;

		/// <summary>
		/// List of commands that are available for tab completion.  Retrieved via the "Get-Command" cmdlet.
		/// </summary>
		protected List<string> _intellisenseCommands = new List<string>();

		/// <summary>
		/// List of parameters that are available for tab completion for each command.
		/// </summary>
		protected Dictionary<string, List<string>> _intellisenseParameters = new Dictionary<string, List<string>>();

		/// <summary>
		/// Thread that is retrieving Intellisense commands.
		/// </summary>
		protected Thread _intellisenseThread = null;

		/// <summary>
		/// Raw access to the console.
		/// </summary>
		protected PowerShellRawUi _powerShellRawUi;

		/// <summary>
		/// Progress bar UI element to update when writing progress records.
		/// </summary>
		protected ToolStripProgressBar _progressBar;

		/// <summary>
		/// Label UI element to update when writing progress records.
		/// </summary>
		protected ToolStripStatusLabel _progressLabel;

		/// <summary>
		/// Flag indicating whether or not we are currently reading input from the user.
		/// </summary>
		protected bool _readingInput = false;

		/// <summary>
		/// Thread where <see cref="ReadingInput"/> was set to true from.  This is the currently-executing pipeline thread and is saved so we can terminate it
		/// if someone hits Ctrl+C when we're in the middle of reading input.
		/// </summary>
		protected Thread _readingInputThread = null;

		/// <summary>
		/// Terminal control that the shell is displayed in.
		/// </summary>
		protected TerminalControl _terminal;

	    protected StreamConnection _connection;

		/// <summary>
		/// Timer to use for asynchronous UI events.
		/// </summary>
		protected Timer _timer = new Timer();

		/// <summary>
		/// List of commands that were executed before the current history entry.
		/// </summary>
		protected Stack<string> _upCommandHistory = new Stack<string>();

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="terminal">Terminal control that will display the PowerShell console.</param>
		/// <param name="executeHelper">Method used to execute PowerShell commands within the current session.</param>
		/// <param name="progressBar">Progress bar UI element to update when writing progress records.</param>
		/// <param name="progressLabel">Label UI element to update when writing progress records.</param>
		public PowerShellHostUi(
			TerminalControl terminal, StreamConnection connection, Func<string, Collection<PSObject>> executeHelper, ToolStripProgressBar progressBar, ToolStripStatusLabel progressLabel)
		{
			_terminal = terminal;
		    _connection = connection;
			_powerShellRawUi = new PowerShellRawUi(terminal, connection);
			_executeHelper = executeHelper;
			_progressBar = progressBar;
			_progressLabel = progressLabel;
		}

		/// <summary>
		/// Flag indicating whether the console is currently at the command prompt reading a command to run from the user.
		/// </summary>
		public bool AtCommandPrompt
		{
			get;
			set;
		}

		/// <summary>
		/// Raw access to the console.
		/// </summary>
		public override PSHostRawUserInterface RawUI
		{
			get
			{
				return _powerShellRawUi;
			}
		}

		/// <summary>
		/// Flag indicating whether or not we are currently reading input from the user.
		/// </summary>
		public bool ReadingInput
		{
			get
			{
				return _readingInput;
			}

			protected set
			{
				_readingInput = value;

				// If we are reading input, save the currently-executing pipeline thread so we can terminate it when if someone hits Ctrl+C when we're in the 
				// middle of reading input.
				_readingInputThread = value
					                      ? Thread.CurrentThread
					                      : null;
			}
		}

		/// <summary>
		/// Run the "Get-Command" cmdlet to get the list of available commands in the shell so that we can power tab completion.
		/// </summary>
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

		/// <summary>
		/// Add a command that the user ran to the history buffer.
		/// </summary>
		/// <param name="command">Command that the user ran.</param>
		public void AddToCommandHistory(string command)
		{
			// Add the current history command to the "up" buffer
			if (!String.IsNullOrEmpty(_currentHistoryCommand))
				_upCommandHistory.Push(_currentHistoryCommand);

			// If the user's command matches the current history command, simply return so we don't polute the history buffer with a bunch of identical entries
			// when the user just hits up or down to navigate through the history and runs a command without changing it
			if (!String.IsNullOrEmpty(_currentHistoryCommand) && _currentHistoryCommand == command)
			{
				_currentHistoryCommand = null;
				return;
			}

			_currentHistoryCommand = null;

			// Move everything in the "down" buffer to the "up" buffer (effectively navigating to the end of the buffer)
			foreach (string historyCommand in _downCommandHistory)
				_upCommandHistory.Push(historyCommand);

			_downCommandHistory.Clear();
			_upCommandHistory.Push(command);

			// Restrict the history buffer to 20 items
			if (_upCommandHistory.Count > 20)
				_upCommandHistory = new Stack<string>(_upCommandHistory.ToArray().Take(20).Reverse());
		}

		/// <summary>
		/// Stop gathering input from the user; abort <see cref="_inputThread"/> and <see cref="_intellisenseThread"/>.
		/// </summary>
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
		/// Prompts the user for input.
		/// </summary>
		/// <param name="caption">The caption or title of the prompt.</param>
		/// <param name="message">The text of the prompt.</param>
		/// <param name="descriptions">A collection of <see cref="FieldDescription"/> objects that describe each field of the prompt.</param>
		/// <returns>A dictionary object that contains the results of the user prompts.</returns>
		public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
		{
			WriteLine(ConsoleColor.Blue, ConsoleColor.Black, caption + "\n" + message + " ");

			Dictionary<string, PSObject> results = new Dictionary<string, PSObject>();

			// Get the input for each prompted value
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
		/// Provides a set of choices that enable the user to choose a single option from a set of options. 
		/// </summary>
		/// <param name="caption">Text that proceeds (a title) the choices.</param>
		/// <param name="message">A message that describes the choice.</param>
		/// <param name="choices">A collection of <see cref="ChoiceDescription"/> objects that describe each choice.</param>
		/// <param name="defaultChoice">The index of the label in the <paramref name="choices"/> parameter collection. To indicate no default choice, set to 
		/// -1.</param>
		/// <returns>The index of the Choices parameter collection element that corresponds to the option that is selected by the user.</returns>
		public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
		{
			// Write the caption and message strings in blue.
			WriteLine(ConsoleColor.Blue, ConsoleColor.Black, caption + "\n" + message + "\n");

			// Convert the choice collection into something that is easier to work with
			string[,] promptData = BuildHotkeysAndPlainLabels(choices);

			// Format the overall choice prompt string to display.
			StringBuilder promptText = new StringBuilder();

			for (int i = 0; i < choices.Count; i++)
				promptText.Append(String.Format(CultureInfo.CurrentCulture, "|{0}> {1} ", promptData[0, i], promptData[1, i]));

			promptText.Append(String.Format(CultureInfo.CurrentCulture, "[Default is ({0})]", promptData[0, defaultChoice]));

			// Read prompts until a match is made, the default is chosen, or the loop is interrupted with Ctrl-C.
			while (true)
			{
				WriteLine(ConsoleColor.Cyan, ConsoleColor.Black, promptText.ToString());
				string data = ReadLine().Trim().ToUpper(CultureInfo.CurrentCulture);

				// If the choice string was empty, use the default selection.
				if (data.Length == 0)
					return defaultChoice;

				// See if the selection matched and return the corresponding index if it did.
				for (int i = 0; i < choices.Count; i++)
				{
					if (promptData[0, i] == data)
						return i;
				}

				WriteErrorLine("Invalid choice: " + data);
			}
		}

		/// <summary>
		/// Prompts the user for credentials with a specified prompt window caption, prompt message, user name, and target name.
		/// </summary>
		/// <param name="caption">The caption for the message window.</param>
		/// <param name="message">The text of the message.</param>
		/// <param name="userName">The user name whose credential is to be prompted for.</param>
		/// <param name="targetName">The name of the target for which the credential is collected.</param>
		/// <returns>The populated credentials that the user entered.</returns>
		public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		/// <summary>
		/// Prompts the user for credentials by using a specified prompt window caption, prompt message, user name and target name, credential types allowed to 
		/// be returned, and UI behavior options.
		/// </summary>
		/// <param name="caption">The caption for the message window.</param>
		/// <param name="message">The text of the message.</param>
		/// <param name="userName">The user name whose credential is to be prompted for.</param>
		/// <param name="targetName">The name of the target for which the credential is collected.</param>
		/// <param name="allowedCredentialTypes">A constant that identifies the type of credentials that can be returned.</param>
		/// <param name="options">A constant that identifies the UI behavior when it gathers the credentials.</param>
		/// <returns>The populated credentials that the user entered.</returns>
		public override PSCredential PromptForCredential(
			string caption, string message, string userName, string targetName,
			PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		/// <summary>
		/// Reads characters that are entered by the user until a newline (carriage return) is encountered.
		/// </summary>
		/// <returns>The characters that are entered by the user.</returns>
		public override string ReadLine()
		{
			try
			{
				ReadingInput = true;

				// If we haven't already retrieved a list of commands available for Intellisense, kick off the process now
				if (_intellisenseThread == null)
				{
					_intellisenseThread = new Thread(GetIntellisenseCommands)
						                      {
							                      Name = "PowerShellHostUi Intellisense Thread"
						                      };
					_intellisenseThread.Start();
				}

				// Tell the stream to start capturing
				_connection.Capture = true;

			    _currentInputLineMaxLength = 0;
				_currentInputLine.Clear();
				_inputSemaphore.Reset();

				// Spin up a thread to read from the console; when it hits a new line, it will signal via _inputSemaphore
				_inputThread = new Thread(ReadInput)
					               {
						               Name = "PowerShellHostUi Input Thread"
					               };
				_inputThread.Start(_connection);

				// Wait until the input thread sees a new line
				_inputSemaphore.WaitOne();

				// Stop capturing characters via the console
				_connection.Capture = false;

				return _currentInputLine.ToString();
			}

			finally
			{
				ReadingInput = false;
			}
		}

		/// <summary>
		/// Thread method invoked from <see cref="ReadLine"/> that reads user input from the console (via a <see cref="StreamConnection"/> object provided in
		/// the <paramref name="state"/> parameter) until a new line character is encountered.
		/// </summary>
		/// <param name="state"><see cref="StreamConnection"/> that we are to read the user's input from.</param>
		private void ReadInput(object state)
		{
            Thread.Sleep(250);

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

						// Handle the TAB character
						if (currentByte == 9)
						{
							// This is the first time that the user has activated tab completion (i.e. they haven't hit tab to bring up the first tab 
							// completion command and then hit tab again to cycle through it)
							if (intellisenseStartLocation == null)
							{
								Token[] tokens;
								ParseError[] parseErrors;

								// Parse the current input line up to the cursor location
								Parser.ParseInput(_currentInputLine.ToString().Substring(0, insertPosition), out tokens, out parseErrors);

								// Get the last grammar token prior to the end of input
								Token currentToken = tokens.LastOrDefault(t => t.Kind != TokenKind.EndOfInput);

								if (currentToken != null && !String.IsNullOrEmpty(currentToken.Text))
								{
									if (((currentToken.Kind == TokenKind.Generic || currentToken.Kind == TokenKind.Identifier) && currentToken.TokenFlags == TokenFlags.CommandName) ||
									    currentToken.Kind == TokenKind.Parameter ||
									    (currentToken.Kind == TokenKind.Generic && (currentToken.TokenFlags & TokenFlags.BinaryPrecedenceAdd) == TokenFlags.BinaryPrecedenceAdd) ||
									    (currentToken.Kind == TokenKind.Identifier && currentToken.TokenFlags == TokenFlags.None) ||
									    (currentToken.Kind == TokenKind.Generic && currentToken.TokenFlags == TokenFlags.None) || currentToken.Kind == TokenKind.StringLiteral)
									{
										// Set the location that we're "Intellisensing" from, basically the location in the line that we're going to insert
										// the Intellisense candidates in
										intellisenseStartLocation = insertPosition - currentToken.Text.Length;
										intellisenseCandidatesIndex = -1;
										currentIntellisenseCommand = _currentInputLine.ToString().Substring(intellisenseStartLocation.Value, insertPosition - intellisenseStartLocation.Value);

										// If the grammar token is a command name, an identifier, or a string literal
										if (((currentToken.Kind == TokenKind.Generic || currentToken.Kind == TokenKind.Identifier) && currentToken.TokenFlags == TokenFlags.CommandName) ||
										    (currentToken.Kind == TokenKind.Identifier && currentToken.TokenFlags == TokenFlags.None) ||
										    (currentToken.Kind == TokenKind.Generic && currentToken.TokenFlags == TokenFlags.None) || currentToken.Kind == TokenKind.StringLiteral)
										{
											string match = currentToken.Text.Replace("'", "").Replace("\"", "");
											string searchPath = null;

											if (match.Contains("\\"))
											{
												// ReSharper disable StringLastIndexOfIsCultureSpecific.1
												searchPath = match.Substring(0, match.LastIndexOf("\\") + 1);
												match = match.Substring(match.LastIndexOf("\\") + 1);
												// ReSharper restore StringLastIndexOfIsCultureSpecific.1
											}

											// In addition to PowerShell commands, we're also going to return a list of available files in the search directory
											// (the current directory if there's no backslash in the token, or the text preceding the backslash if there is 
											// one)
											IEnumerable<string> childItems = _executeHelper(
												"get-childitem -Filter '" + match + "*'" + (!String.IsNullOrEmpty(searchPath)
													                                            ? " -Path '" + searchPath + "'"
													                                            : "")).Select(
														                                            i => String.IsNullOrEmpty(searchPath)
															                                                 ? ".\\" + i.ToString()
															                                                 : searchPath + i.ToString());

											// Union the matching files and the matching commands to get the full list of Intellisense candidates
											intellisenseCandidates =
												(((currentToken.Kind == TokenKind.Generic || currentToken.Kind == TokenKind.Identifier) && currentToken.TokenFlags == TokenFlags.CommandName)
													 ? _intellisenseCommands.Where(c => c.ToLower().StartsWith(currentToken.Text.ToLower()))
													 : new List<string>()).Union(childItems).Select(
														 c => c.Contains(" ")
															      ? "'" + c + "'"
															      : c).OrderBy(c => c).ToList();
										}

											// If the grammar token is a command parameter
										else if (currentToken.Kind == TokenKind.Parameter ||
										         (currentToken.Kind == TokenKind.Generic && (currentToken.TokenFlags & TokenFlags.BinaryPrecedenceAdd) == TokenFlags.BinaryPrecedenceAdd))
										{
											List<Token> tokenList = tokens.ToList();

											if (tokenList.IndexOf(currentToken) > 0)
											{
												// Get the token representing the name of the command for this parameter
												int commandTokenIndex = tokenList.FindLastIndex(
													tokenList.IndexOf(currentToken) - 1, tokenList.IndexOf(currentToken), t => t.TokenFlags == TokenFlags.CommandName);

												if (commandTokenIndex != -1)
												{
													Token commandToken = tokens[commandTokenIndex];

													// Get the list of parameters for the command if we don't have it already
													if (!_intellisenseParameters.ContainsKey(commandToken.Text.ToLower()))
													{
														_intellisenseParameters[commandToken.Text.ToLower()] = new List<string>();

														// Run the "Get-Command" cmdlet and look at its Parameters property
														Collection<PSObject> command = _executeHelper("get-command " + commandToken.Text);

														if (command != null)
														{
															_intellisenseParameters[commandToken.Text.ToLower()].AddRange(
																from parameter in (command[0].Properties["Parameters"].Value as Dictionary<string, ParameterMetadata>).Keys
																select "-" + parameter);
														}
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

							// Cycle to the next Intellisense candidate
							if (intellisenseStartLocation != null && intellisenseCandidates.Count > 0)
							{
								intellisenseCandidatesIndex += ShiftKeyDown
									                               ? -1
									                               : 1;

								if (intellisenseCandidatesIndex >= intellisenseCandidates.Count)
									intellisenseCandidatesIndex = 0;

								else if (intellisenseCandidatesIndex < 0)
									intellisenseCandidatesIndex = intellisenseCandidates.Count - 1;

								string command = intellisenseCandidates[intellisenseCandidatesIndex.Value];

								// Remove the current token from the line and replace it with the current Intellisense candidate
								_currentInputLine.Remove(intellisenseStartLocation.Value, currentIntellisenseCommand.Length);
								_currentInputLine.Insert(intellisenseStartLocation.Value, command);

								// Move the cursor to the start of the prompt and re-write the current line, appending spaces if the current Intellisense
								// candidate was shorter than the previous one
								RawUI.CursorPosition = promptStart;
								Write(
									_currentInputLine + (currentIntellisenseCommand.Length > command.Length
										                     ? new string(' ', currentIntellisenseCommand.Length - command.Length)
										                     : ""));
								promptEnd = RawUI.CursorPosition;

								// Reposition the cursor properly to end of the Intellisense candidate
								RawUI.CursorPosition = new Coordinates(
									(promptStart.X + intellisenseStartLocation.Value + command.Length) % RawUI.BufferSize.Width,
									promptStart.Y + Convert.ToInt32(Math.Floor((promptStart.X + intellisenseStartLocation.Value + command.Length) / (double) RawUI.BufferSize.Width)));

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

						// Handle the BACKSPACE character
						if (currentByte == 8)
						{
							Coordinates currentPosition = GetCurrentCursorPosition(promptStart, insertPosition);

							// If the cursor is somewhere other than the very start of the prompt
							if (insertPosition > 0)
							{
								// Remove the preceding character from _currentInputLine
								_currentInputLine.Remove(insertPosition - 1, 1);
								insertPosition--;

								currentPosition = currentPosition.X > 0
									                  ? new Coordinates(currentPosition.X - 1, currentPosition.Y)
									                  : new Coordinates(RawUI.BufferSize.Width, currentPosition.Y - 1);

								// Move the cursor to the current input position and re-emit the rest of the line (minus the character that we removed) as well
								// as a trailing space to cover the previous end character
								RawUI.CursorPosition = currentPosition;
								Write(_currentInputLine.ToString(insertPosition, _currentInputLine.Length - insertPosition));
								Write(" ");
								RawUI.CursorPosition = currentPosition;

								promptEnd = promptEnd.X > 1
									            ? new Coordinates(promptEnd.X - 1, promptEnd.Y)
									            : new Coordinates(RawUI.BufferSize.Width, promptEnd.Y - 1);
							}
						}

							// In ANSI, the ESCAPE character means we're starting a special processing sequence
						else if (currentByte == 27)
							inEscapeSequence = true;

							// If we're in an escape sequence and we see a "[" or a "~", simply continue on to the next character in the stream
						else if (currentByte == 91 && inEscapeSequence)
						{
						}

						else if (currentByte == 126 && inEscapeSequence)
						{
						}

							// ^7 translates to the HOME key
						else if (currentByte == 55 && inEscapeSequence)
						{
						    insertPosition = 0;
                            RawUI.CursorPosition = GetCurrentCursorPosition(promptStart, insertPosition);
						}

							// ^8 translates to the END key
						else if (currentByte == 56 && inEscapeSequence)
						{
						    insertPosition = _currentInputLine.Length;
                            RawUI.CursorPosition = GetCurrentCursorPosition(promptStart, insertPosition);
						}

							// ^D translates to the left arrow, so we move the cursor backwards
						else if (currentByte == 68 && inEscapeSequence)
						{
						    Coordinates currentPosition = GetCurrentCursorPosition(promptStart, insertPosition);

                            if (RawUI.CursorPosition != promptStart)
							{
								RawUI.CursorPosition = currentPosition.X > 0
									                       ? new Coordinates(currentPosition.X - 1, currentPosition.Y)
									                       : new Coordinates(RawUI.BufferSize.Width, currentPosition.Y - 1);
								insertPosition--;
							}

							inEscapeSequence = false;
						}

							// ^D translates to the right arrow, so we move the cursor forwards
						else if (currentByte == 67 && inEscapeSequence)
						{
						    Coordinates currentPosition = GetCurrentCursorPosition(promptStart, insertPosition);

                            if (RawUI.CursorPosition != promptEnd)
							{
								RawUI.CursorPosition = currentPosition.X < RawUI.BufferSize.Width
									                       ? new Coordinates(currentPosition.X + 1, currentPosition.Y)
									                       : new Coordinates(0, currentPosition.Y + 1);
								insertPosition++;
							}

							inEscapeSequence = false;
						}

							// ^D translates to the DELETE key, so we remove the current character
						else if (currentByte == 51 && inEscapeSequence)
						{
						    Coordinates currentPosition = GetCurrentCursorPosition(promptStart, insertPosition);

                            // If the cursor is somewhere other than the very start of the prompt
                            if (RawUI.CursorPosition != promptEnd)
							{
								// Remove the current character from _currentInputLine
								_currentInputLine.Remove(insertPosition, 1);

								if (insertPosition < _currentInputLine.Length)
								{
									// Re-emit the rest of the line (minus the character that we removed) as well as a trailing space to cover the previous end 
									// character
									Write(_currentInputLine.ToString(insertPosition, _currentInputLine.Length - insertPosition));
									Write(" ");
									RawUI.CursorPosition = currentPosition;

									promptEnd = promptEnd.X > 0
										            ? new Coordinates(promptEnd.X - 1, promptEnd.Y)
										            : new Coordinates(RawUI.BufferSize.Width, promptEnd.Y - 1);
								}
							}

							inEscapeSequence = false;
						}

							// ^A translates to the up arrow, so we move to the previous (if any) entry in the history buffer
						else if (currentByte == 65 && inEscapeSequence)
						{
							// If there are any preceding items in the history buffer
							if (AtCommandPrompt && _upCommandHistory.Count > 0)
							{
								// Save the current history command
								if (!String.IsNullOrEmpty(_currentHistoryCommand))
									_downCommandHistory.Push(_currentHistoryCommand);

								_currentHistoryCommand = _upCommandHistory.Pop();

								// Wipe out the current line entered by the user
								RawUI.CursorPosition = promptStart;
								Write(new string(' ', _currentInputLine.Length));

								// Move the cursor back to the start and write the history command
								RawUI.CursorPosition = promptStart;
								Write(_currentHistoryCommand);
								promptEnd = RawUI.CursorPosition;
								_currentInputLine = new StringBuilder(_currentHistoryCommand);
								insertPosition = _currentInputLine.Length;
							}

							inEscapeSequence = false;
						}

							// ^A translates to the down arrow, so we move to the previous (if any) entry in the history buffer
						else if (currentByte == 66 && inEscapeSequence)
						{
							// If there are any following items in the history buffer
							if (AtCommandPrompt && _downCommandHistory.Count > 0)
							{
								// Save the current history command
								if (!String.IsNullOrEmpty(_currentHistoryCommand))
									_upCommandHistory.Push(_currentHistoryCommand);

								_currentHistoryCommand = _downCommandHistory.Pop();

								// Wipe out the current line entered by the user
								RawUI.CursorPosition = promptStart;
								Write(new string(' ', _currentInputLine.Length));

								// Move the cursor back to the start and write the history command
								RawUI.CursorPosition = promptStart;
								Write(_currentHistoryCommand);
								promptEnd = RawUI.CursorPosition;
								_currentInputLine = new StringBuilder(_currentHistoryCommand);
								insertPosition = _currentInputLine.Length;
							}

							inEscapeSequence = false;
						}

							// Handle the carriage return character; write a new line and exit the loop
						else if (currentByte == 13)
						{
							WriteLine();
							foundCarriageReturn = true;

							break;
						}

							// Otherwise, if it's an ASCII character, write it to the console and add it to _currentInputLine
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
								Coordinates currentPosition = GetCurrentCursorPosition(promptStart, insertPosition);
								Write(_currentInputLine.ToString(insertPosition, _currentInputLine.Length - insertPosition));
								RawUI.CursorPosition = currentPosition;
							}

							promptEnd = promptEnd.X < RawUI.BufferSize.Width
								            ? new Coordinates(promptEnd.X + 1, promptEnd.Y)
								            : new Coordinates(0, promptEnd.Y + 1);
						}

						else
							inEscapeSequence = false;
					}
				}

			    _currentInputLineMaxLength = Math.Max(_currentInputLineMaxLength, _currentInputLine.Length);

				// If we're still waiting on a carriage return, sleep
				if (!foundCarriageReturn)
					Thread.Sleep(50);
			}

			_inputSemaphore.Set();
		}

	    protected Coordinates GetCurrentCursorPosition(Coordinates promptStart, int insertPosition)
	    {
	        int promptYLength = (int) Math.Floor((_currentInputLineMaxLength + promptStart.X) / (decimal) RawUI.BufferSize.Width);

	        Coordinates currentCursorPosition = new Coordinates(
	            (promptStart.X + insertPosition) % RawUI.BufferSize.Width,
	            promptStart.Y + (int)Math.Floor((promptStart.X + insertPosition) / (decimal)RawUI.BufferSize.Width));

	        if (promptStart.Y + promptYLength > RawUI.BufferSize.Height - 1)
	        {
	            currentCursorPosition.Y -= promptYLength;
	        }

	        return currentCursorPosition;
	    }

		/// <summary>
		/// Reads characters entered by the user until a newline (carriage return) is encountered and returns the characters as a secure string.
		/// </summary>
		/// <returns>A <see cref="SecureString"/> object containing the text entered by the user.</returns>
		public override SecureString ReadLineAsSecureString()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Writes characters to the output display of the host.
		/// </summary>
		/// <param name="value">The characters to be written.</param>
		public override void Write(string value)
		{
			// Replace newlines in the value with the ANSI newline control code
			byte[] buffer = Encoding.UTF8.GetBytes(value.Replace("\n", "\x001BE"));
			_connection.Receive(buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Writes characters to the output display of the host with possible foreground and background colors. 
		/// </summary>
		/// <param name="foregroundColor">The color of the characters.</param>
		/// <param name="backgroundColor">The backgound color to use.</param>
		/// <param name="value">The characters to be written.</param>
		public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
		{
			RawUI.ForegroundColor = foregroundColor;
			RawUI.BackgroundColor = backgroundColor;
			Write(value);

			// Set the foreground and background colors back to their defaults
			(RawUI as PowerShellRawUi).RestoreForegroundColor();
			(RawUI as PowerShellRawUi).RestoreBackgroundColor();
		}

		/// <summary>
		/// Writes a line of characters to the output display of the host with foreground and background colors and appends a newline (carriage return). 
		/// </summary>
		/// <param name="foregroundColor">The foreground color of the display. </param>
		/// <param name="backgroundColor">The background color of the display. </param>
		/// <param name="value">The line to be written.</param>
		public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
		{
			Write(foregroundColor, backgroundColor, value + "\n");
		}

		/// <summary>
		/// Writes a debug message to the output display of the host.
		/// </summary>
		/// <param name="message">The debug message that is displayed.</param>
		public override void WriteDebugLine(string message)
		{
			WriteLine(ConsoleColor.DarkYellow, ConsoleColor.Black, String.Format(CultureInfo.CurrentCulture, "DEBUG: {0}", message));
		}

		/// <summary>
		/// Writes an error message to the output display of the host.
		/// </summary>
		/// <param name="value">The error message that is displayed.</param>
		public override void WriteErrorLine(string value)
		{
			WriteLine(ConsoleColor.Red, ConsoleColor.Black, value);
		}

		/// <summary>
		/// Writes a newline character (carriage return) to the output display of the host. 
		/// </summary>
		public override void WriteLine()
		{
			WriteLine("");
		}

		/// <summary>
		/// Writes a line of characters to the output display of the host and appends a newline character(carriage return). 
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
			_progressLabel.Text = String.IsNullOrEmpty(record.Activity)
				                      ? ""
				                      : record.Activity + (String.IsNullOrEmpty(record.CurrentOperation)
					                                           ? ""
					                                           : ": ") + record.CurrentOperation;
			_progressBar.Value = record.PercentComplete;

			// If we've completed, leave it in its current state for one second and then clear the progress data
			if (_progressBar.Value == 100)
			{
				_timer.Enabled = true;
				_timer.Interval = 1000;
				_timer.Elapsed += _timer_Elapsed;
				_timer.Start();
			}
		}

		/// <summary>
		/// Handler method that's called when <see cref="_timer"/> ticks.  Clears the progress data from the UI and stops the timer.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="_timer"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			_progressLabel.Text = "";
			_progressBar.Value = 0;

			_timer.Elapsed -= _timer_Elapsed;
			_timer.Stop();
		}

		/// <summary>
		/// Writes a verbose message to the output display of the host.
		/// </summary>
		/// <param name="message">The verbose message that is displayed.</param>
		public override void WriteVerboseLine(string message)
		{
			WriteLine(ConsoleColor.Green, ConsoleColor.Black, String.Format(CultureInfo.CurrentCulture, "VERBOSE: {0}", message));
		}

		/// <summary>
		/// Writes a warning message to the output display of the host.
		/// </summary>
		/// <param name="message">The warning message that is displayed.</param>
		public override void WriteWarningLine(string message)
		{
			WriteLine(ConsoleColor.Yellow, ConsoleColor.Black, String.Format(CultureInfo.CurrentCulture, "WARNING: {0}", message));
		}

		/// <summary>
		/// Parse a string containing a hotkey character. Take a string of the form "Yes to &amp;all" and returns a two-dimensional array split out as ["A", 
		/// "Yes to all"].
		/// </summary>
		/// <param name="input">The string to process</param>
		/// <returns>A two dimensional array containing the parsed components.</returns>
		protected static string[] GetHotkeyAndLabel(string input)
		{
			string[] result = new string[]
				                  {
					                  String.Empty, String.Empty
				                  };
			string[] fragments = input.Split('&');

			if (fragments.Length == 2)
			{
				if (fragments[1].Length > 0)
					result[0] = fragments[1][0].ToString().ToUpper(CultureInfo.CurrentCulture);

				result[1] = (fragments[0] + fragments[1]).Trim();
			}

			else
				result[1] = input;

			return result;
		}

		/// <summary>
		/// This is a worker function that splits out the accelerator keys from the menu and builds a two dimensional array with the first access containing
		/// the accelerator and the second containing the label string with the &amp; removed.
		/// </summary>
		/// <param name="choices">The choice collection to process.</param>
		/// <returns>A two dimensional array containing the accelerator characters and the cleaned-up labels</returns>
		protected static string[,] BuildHotkeysAndPlainLabels(Collection<ChoiceDescription> choices)
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

		/// <summary>
		/// Stop the currently-executing pipeline and any input that goes along with it.
		/// </summary>
		public void StopCurrentPipeline()
		{
			// Abort the actual pipeline thread first
			try
			{
				_readingInputThread.Abort();
			}

			catch (Exception)
			{
			}

			// Then abort the input thread
			ReadingInput = false;
			EndInput();
		}

		/// <summary>
		/// Provides a set of choices that enable the user to choose a one or more options from a set of options. 
		/// </summary>
		/// <param name="caption">Text that proceeds (a title) the choices.</param>
		/// <param name="message">A message that describes the choice.</param>
		/// <param name="choices">A collection of ChoiceDescription objects that describe each choice.</param>
		/// <param name="defaultChoices">The index of the label in the <paramref name="choices"/> parameter collection. To indicate no default choice, set to 
		/// -1.</param>
		/// <returns>The index of the Choices parameter collection element that corresponds to the option that is selected by the user.</returns>
		public Collection<int> PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, IEnumerable<int> defaultChoices)
		{
			// Write the caption and message strings in blue.
			WriteLine(ConsoleColor.Blue, ConsoleColor.Black, caption + "\n" + message + "\n");

			// Convert the choice collection into something that is easier to work with
			string[,] promptData = BuildHotkeysAndPlainLabels(choices);

			// Format the overall choice prompt string to display.
			StringBuilder promptText = new StringBuilder();

			for (int i = 0; i < choices.Count; i++)
				promptText.Append(String.Format(CultureInfo.CurrentCulture, "|{0}> {1} ", promptData[0, i], promptData[1, i]));

			Collection<int> defaultResults = new Collection<int>();
			List<int> defaultChoicesList = defaultChoices.ToList();

			if (defaultChoices != null)
			{
				int countDefaults = 0;

				foreach (int defaultChoice in defaultChoicesList)
				{
					++countDefaults;
					defaultResults.Add(defaultChoice);
				}

				if (countDefaults != 0)
				{
					promptText.Append(
						countDefaults == 1
							? "[Default choice is "
							: "[Default choices are ");

					foreach (int defaultChoice in defaultChoicesList)
						promptText.AppendFormat(CultureInfo.CurrentCulture, "\"{0}\",", promptData[0, defaultChoice]);

					promptText.Remove(promptText.Length - 1, 1);
					promptText.Append("]");
				}
			}

			WriteLine(ConsoleColor.Cyan, ConsoleColor.Black, promptText.ToString());

			// Read prompts until a match is made, the default is chosen, or the loop is interrupted with Ctrl-C.
			Collection<int> results = new Collection<int>();

			while (true)
			{
				string prompt = string.Format(CultureInfo.CurrentCulture, "Choice[{0}]: ", results.Count);
				Write(ConsoleColor.Cyan, ConsoleColor.Black, prompt);
				string data = ReadLine().Trim().ToUpper(CultureInfo.CurrentCulture);

				// If the choice string was empty, no more choices have been made.  If there were no choices made, return the defaults
				if (data.Length == 0)
				{
					return (results.Count == 0)
						       ? defaultResults
						       : results;
				}

				bool showError = true;

				// See if the selection matched and return the corresponding index if it did
				for (int i = 0; i < choices.Count; i++)
				{
					if (promptData[0, i] == data)
					{
						results.Add(i);
						showError = false;
					}
				}

				if (showError)
					WriteErrorLine("Invalid choice: " + data);
			}
		}

	    public bool ShiftKeyDown
	    {
	        get;
	        set;
	    }
	}
}