using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using System.Windows.Forms;
using Poderosa;
using Poderosa.Config;
using Poderosa.Connection;
using Poderosa.ConnectionParam;
using Poderosa.Terminal;
using WalburySoftware;
using Shell = System.Management.Automation.PowerShell;

namespace EasyConnect.Protocols.PowerShell
{
	/// <summary>
	/// UI that displays a PowerShell console via the <see cref="TerminalControl"/> class.
	/// </summary>
	public partial class PowerShellConnectionForm : BaseConnectionForm<PowerShellConnection>
	{
		/// <summary>
		/// Holds a reference to the runspace for this interpreter.
		/// </summary>
		internal Runspace Runspace;

		/// <summary>
		/// Holds a reference to the currently executing pipeline so that it can be stopped by the Ctrl-C handler.
		/// </summary>
		protected Shell _currentPowerShell;

		/// <summary>
		/// Semaphore controlling access to the PowerShell execution pipeline.
		/// </summary>
		protected object _executionLock = new object();

		/// <summary>
		/// Thread reading PowerShell command prompt input from the user.
		/// </summary>
		protected Thread _inputThread = null;

		/// <summary>
		/// Semaphore controlling access to <see cref="_currentPowerShell"/>.
		/// </summary>
		protected object _instanceLock = new object();

		/// <summary>
		/// Holds a reference to  the PSHost object for this interpreter.
		/// </summary>
		protected PowerShellHost _powerShellHost;

		/// <summary>
		/// Default constructor.  Turns off the warning that the terminal displays when it encounters an unknown terminal control code.
		/// </summary>
		public PowerShellConnectionForm()
		{
			InitializeComponent();

			GEnv.Options.WarningOption = WarningOption.Ignore;
			Shown += PowerShellConnectionForm_Shown;
		}

		/// <summary>
		/// Control instance that hosts the actual PowerShell prompt UI.
		/// </summary>
		protected override Control ConnectionWindow
		{
			get
			{
				return _terminal;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the connection window should close.
		/// </summary>
		public bool ShouldExit
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the exit code that the connection window uses when closing.
		/// </summary>
		public int ExitCode
		{
			get;
			set;
		}

		/// <summary>
		/// Handler method that's called when the connection window is shown, either initially or after the user switches to another tab and then back to this
		/// one.  It's necessary to call <see cref="Connections.BringToActivationOrderTop"/> so that shortcut keys entered by the user are sent to the correct
		/// window.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void PowerShellConnectionForm_Shown(object sender, EventArgs e)
		{
			if (IsConnected)
				GEnv.Connections.BringToActivationOrderTop(_terminal.TerminalPane.ConnectionTag);
		}

		/// <summary>
		/// Handler method that's called when the connection is established.  Adds this connection to <see cref="Connections"/> and sends it to the top of the
		/// activation queue via a call to <see cref="Connections.BringToActivationOrderTop"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		protected override void OnConnected(object sender, EventArgs e)
		{
			GEnv.Connections.Add(_terminal.TerminalPane.ConnectionTag);
			GEnv.Connections.BringToActivationOrderTop(_terminal.TerminalPane.ConnectionTag);

			base.OnConnected(sender, e);
		}

		/// <summary>
		/// Wires up a PowerShell runspace created via <see cref="RunspaceFactory.CreateRunspace()"/> to the terminal to display the PowerShell to the user.
		/// </summary>
		public override void Connect()
		{
			GEnv.Options.Font = Connection.Font;

			_progressBar.Value = 0;
			_progressLabel.Text = "";

			// This is not strictly a network connection:  we're relaying information that we receive from the runspace to the terminal over a local stream
			// (a StreamConnection in this case)
			TerminalParam terminalParam = TCPTerminalParam.Fake;
			terminalParam.TerminalType = TerminalType.XTerm;
			terminalParam.RenderProfile = new RenderProfile();
			terminalParam.Encoding = EncodingType.UTF8;

			StreamConnection connection = new StreamConnection(terminalParam)
				                              {
					                              Capture = false
				                              };

			ConnectionTag connectionTag = new ConnectionTag(connection);
			connectionTag.Receiver.Listen();

			// Attach the new "connection" to the terminal control
			_terminal.TerminalPane.FakeVisible = true;
			_terminal.TerminalPane.Attach(connectionTag);
			_terminal.TerminalPane.Focus();
			_terminal.TerminalPane.SendShiftTab = true;
			_terminal.SetPaneColors(Connection.TextColor, Connection.BackgroundColor);

			try
			{
				_powerShellHost = new PowerShellHost(this, _terminal, ExecuteQuiet, _progressBar, _progressLabel);

				// Create the host and runspace instances for this interpreter.  If we're connecting to the local host, don't bother with the connection info.
				// ReSharper disable StringCompareIsCultureSpecific.3
				if (String.Compare(Connection.Host, "localhost", true) != 0 && Connection.Host != "127.0.0.1" &&
					String.Compare(Connection.Host, Environment.MachineName, true) != 0)
				// ReSharper restore StringCompareIsCultureSpecific.3
				{
					WSManConnectionInfo connectionInfo = new WSManConnectionInfo
					{
						ComputerName = Connection.Host
					};

					if (!String.IsNullOrEmpty(Connection.InheritedUsername))
						connectionInfo.Credential = new PSCredential(Connection.InheritedUsername, Connection.InheritedPassword);

					Runspace = RunspaceFactory.CreateRunspace(_powerShellHost, connectionInfo);
				}

				else
					Runspace = RunspaceFactory.CreateRunspace(_powerShellHost);

				Runspace.Open();
			}

			catch (Exception e)
			{
				OnConnectionLost(this, new ErrorEventArgs(e));
				return;
			}

			// Start capturing input from the prompt via the input loop
			_inputThread = new Thread(InputLoop)
				               {
					               Name = "PowerShellConnectionForm Input Thread"
				               };
			_inputThread.Start();

			ParentForm.Closing += ParentForm_Closing;

			OnConnected(this, null);
		}

		/// <summary>
		/// Event handler that's called when the connection window is closing.  Clean up all of our threads, exit the PowerShell instance, and dispose of
		/// <see cref="_currentPowerShell"/>.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		protected void ParentForm_Closing(object sender, CancelEventArgs e)
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
				ExecuteQuiet("exit");
			}

			finally
			{
				if (_currentPowerShell != null)
				{
					_currentPowerShell.Dispose();
					_currentPowerShell = null;
				}

				_powerShellHost.Exit();
			}
		}

		/// <summary>
		/// Handles the use of Ctrl+C to abort the currently-executing PowerShell command.
		/// </summary>
		/// <param name="msg">UI message associated with the key press.</param>
		/// <param name="keyData">Data containing the keys that were pressed.</param>
		/// <returns>The return value of base.ProcessCmdKey.</returns>
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == (Keys.Control | Keys.C))
			{
				try
				{
					lock (_instanceLock)
					{
						_powerShellHost.StopCurrentPipeline();

						if (_currentPowerShell != null && _currentPowerShell.InvocationStateInfo.State == PSInvocationState.Running)
							_currentPowerShell.Stop();

						_powerShellHost.UI.WriteLine("");
					}
				}

				catch (Exception exception)
				{
					_powerShellHost.UI.WriteErrorLine(exception.ToString());
				}
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		/// <summary>
		/// A helper class that builds and executes a pipeline that writes to the default output path, depending on the value of <paramref name="quiet"/>. Any 
		/// exceptions that are thrown are just passed to the caller.
		/// </summary>
		/// <param name="command">The script to run.</param>
		/// <param name="input">Any input arguments to pass to the script. If null then nothing is passed in.</param>
		/// <param name="quiet">Whether or not the results of the call should be written to the console.</param>
		/// <returns>The results of the call to _currentPowerShell.<see cref="PowerShell.Invoke()"/>.</returns>
		protected Collection<PSObject> ExecuteHelper(string command, object input, bool quiet = false)
		{
			// Ignore empty command lines.
			if (String.IsNullOrEmpty(command))
				return null;

			lock (_executionLock)
			{
				// Create the pipeline object and make it available to the Ctrl-C handle through the _currentPowerShell instance variable.
				lock (_instanceLock)
				{
					_currentPowerShell = Shell.Create();
				}

				// Create a pipeline for this execution, and then place the result in the _currentPowerShell variable so it is available to be stopped.
				try
				{
					_currentPowerShell.Runspace = Runspace;
					_currentPowerShell.AddScript(command);

					if (!quiet)
					{
						// Add the default outputter to the end of the pipe and then call the MergeMyResults method to merge the output and error streams from 
						// the pipeline. This will result in the output being written using the PSHost and PSHostUserInterface classes instead of returning 
						// objects to the host application.
						_currentPowerShell.AddCommand("out-default");
						_currentPowerShell.Commands.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
					}

					// If there is any input pass it in, otherwise just invoke the the pipeline.
					if (input != null)
					{
						return _currentPowerShell.Invoke(
							new object[]
								{
									input
								});
					}

					else
						return _currentPowerShell.Invoke();
				}

				finally
				{
					// Dispose the PowerShell object and set _currentPowerShell to null. It is locked because _currentPowerShell may be accessed by the Ctrl-C 
					// handler.
					lock (_instanceLock)
					{
					    if (_currentPowerShell != null)
					    {
                            _currentPowerShell.Dispose();
                            _currentPowerShell = null;
					    }
					}
				}
			}
		}

		/// <summary>
		/// To display an exception using the display formatter, run a second pipeline passing in the error record. The runtime will bind this to the $input 
		/// variable, which is why $input is being piped to the Out-String cmdlet. The WriteErrorLine method is called to make sure the error gets displayed in 
		/// the correct error color.
		/// </summary>
		/// <param name="e">The exception to display.</param>
		private void ReportException(Exception e)
		{
			if (e != null)
			{
				IContainsErrorRecord errorRecord = e as IContainsErrorRecord;
				object error = errorRecord != null
					               ? errorRecord.ErrorRecord
					               : new ErrorRecord(e, "Host.ReportException", ErrorCategory.NotSpecified, null);

				lock (_instanceLock)
				{
					_currentPowerShell = Shell.Create();
				}

				_currentPowerShell.Runspace = Runspace;

				try
				{
					_currentPowerShell.AddScript("$input").AddCommand("out-string");

					// Do not merge errors, this function will swallow errors.
					Collection<PSObject> result;
					PSDataCollection<object> inputCollection = new PSDataCollection<object>
						                                           {
							                                           error
						                                           };

					inputCollection.Complete();

					lock (_executionLock)
					{
						result = _currentPowerShell.Invoke(inputCollection);
					}

					if (result.Count > 0)
					{
						string output = result[0].BaseObject as string;

						// Remove \r\n, which is added by the Out-String cmdlet.
						if (!string.IsNullOrEmpty(output))
							_powerShellHost.UI.WriteErrorLine(output.Substring(0, output.Length - 2));
					}
				}

				finally
				{
					// Dispose of the pipeline and set it to null, locking it because _currentPowerShell may be accessed by the Ctrl-C handler.
					lock (_instanceLock)
					{
					    if (_currentPowerShell != null)
					    {
					        _currentPowerShell.Dispose();
					        _currentPowerShell = null;
					    }
					}
				}
			}
		}

		/// <summary>
		/// Basic script execution routine. Any runtime exceptions are caught and passed back to the Windows PowerShell engine to display.
		/// </summary>
		/// <param name="command">Script to run.</param>
		/// <returns>The results of the call to <see cref="ExecuteHelper"/>.</returns>
		protected Collection<PSObject> Execute(string command)
		{
			try
			{
				// Execute the command with no input.
				return ExecuteHelper(command, null);
			}

			catch (RuntimeException e)
			{
				ReportException(e);
				return null;
			}
		}

		/// <summary>
		/// Script execution routine that runs a command, but suppresses any output that would be displayed.
		/// </summary>
		/// <param name="command">Script to run.</param>
		/// <returns>The results of the call to <see cref="ExecuteHelper"/>.</returns>
		protected Collection<PSObject> ExecuteQuiet(string command)
		{
			try
			{
				// Execute the command with no input.
				return ExecuteHelper(command, null, true);
			}

			catch (RuntimeException)
			{
				return null;
			}
		}

		/// <summary>
		/// Implements the basic listener loop. It reads a command from the user, executes it and repeats until the ShouldExit flag is set.
		/// </summary>
		public void InputLoop()
		{
			try
			{
				// Go to the user's home directory
				ExecuteQuiet("cd $HOME");

				// Read commands to execute until ShouldExit is set by the user calling "exit".
				while (!ShouldExit)
				{
					string prompt;
					string currentDirectory = ExecuteQuiet("pwd")[0].ToString();

					// Display the prompt containing the current directory to the user, prefixed by the name of the remote machine if we are remoted into one
					if (_powerShellHost.IsRunspacePushed)
						prompt = string.Format("[{0}] PS {1}> ", Runspace.ConnectionInfo.ComputerName, currentDirectory);

					else
						prompt = "PS " + currentDirectory + "> ";

					_powerShellHost.UI.Write(prompt);

					// Read the command from the prompt and run it
					_powerShellHost.AtCommandPrompt = true;
					string cmd = _powerShellHost.UI.ReadLine();
					_powerShellHost.AtCommandPrompt = false;
					_powerShellHost.AddToCommandHistory(cmd);

					Execute(cmd);
				}
			}

			finally
			{
				// Dispose of the pipeline line and set it to null, locked because _currentPowerShell may be accessed by the Ctrl-C handler
				lock (_instanceLock)
				{
					if (_currentPowerShell != null)
					{
						_currentPowerShell.Dispose();
						_currentPowerShell = null;
					}
				}
			}

			OnLoggedOff(this, null);
		}
	}
}