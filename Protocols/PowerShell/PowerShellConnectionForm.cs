using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Granados.SSHC;
using Poderosa;
using Poderosa.Communication;
using Poderosa.Config;
using Poderosa.Connection;
using Poderosa.ConnectionParam;
using Poderosa.LocalShell;
using Poderosa.Terminal;
using Shell = System.Management.Automation.PowerShell;
using ThreadState = System.Threading.ThreadState;

namespace EasyConnect.Protocols.PowerShell
{
	public partial class PowerShellConnectionForm : BaseConnectionForm<PowerShellConnection>
	{
		protected Thread _inputThread = null;

		public PowerShellConnectionForm()
		{
			InitializeComponent();

			GEnv.Options.WarningOption = WarningOption.Ignore;
			Shown += PowerShellConnectionForm_Shown;
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

		protected override Control ConnectionWindow
		{
			get
			{
				return _terminal;
			}
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

		public override void Connect()
		{
			GEnv.Options.Font = Connection.Font;

			TerminalParam terminalParam = TCPTerminalParam.Fake;
			terminalParam.TerminalType = TerminalType.XTerm;
			terminalParam.RenderProfile = new RenderProfile();
			terminalParam.Encoding = EncodingType.UTF8;
			terminalParam.LocalEcho = true;

			StreamConnection connection = new StreamConnection(terminalParam);
			connection.Capture = false;

			ConnectionTag connectionTag = new ConnectionTag(connection);
			connectionTag.Receiver.Listen();

			_terminal.TerminalPane.FakeVisible = true;
			_terminal.TerminalPane.Attach(connectionTag);
			_terminal.TerminalPane.Focus();
			_terminal.SetPaneColors(Connection.TextColor, Connection.BackgroundColor);

			_inputThread = new Thread(new ThreadStart(InputLoop));
			_inputThread.Start();

			OnConnected(this, null);
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			if (_inputThread != null && _inputThread.ThreadState == ThreadState.Running)
				_inputThread.Abort();
		}

		/// <summary>
		/// Holds a reference to the runspace for this interpeter.
		/// </summary>
		internal Runspace myRunSpace;

		/// <summary>
		/// Indicator to tell the host application that it should exit.
		/// </summary>
		private bool shouldExit;

		/// <summary>
		/// The exit code that the host application will use to exit.
		/// </summary>
		private int exitCode;

		/// <summary>
		/// Holds a reference to  the PSHost object for this interpreter.
		/// </summary>
		private PowerShellHost _powerShellHost;

		/// <summary>
		/// Holds a reference to the currently executing pipeline so that 
		/// it can be stopped by the control-C handler.
		/// </summary>
		private Shell currentPowerShell;

		/// <summary>
		/// Used to serialize access to instance data.
		/// </summary>
		private object instanceLock = new object();

		/// <summary>
		/// Gets or sets a value indicating whether the host applcation
		/// should exit.
		/// </summary>
		public bool ShouldExit
		{
			get { return this.shouldExit; }
			set { this.shouldExit = value; }
		}

		/// <summary>
		/// Gets or sets the exit code that the host application will use 
		/// when exiting.
		/// </summary>
		public int ExitCode
		{
			get { return this.exitCode; }
			set { this.exitCode = value; }
		}

		/// <summary>
		/// A helper class that builds and executes a pipeline that writes to the
		/// default output path. Any exceptions that are thrown are just passed to
		/// the caller. Since all output goes to the default 
		/// outter, this method does not return anything.
		/// </summary>
		/// <param name="cmd">The script to run.</param>
		/// <param name="input">Any input arguments to pass to the script. 
		/// If null then nothing is passed in.</param>
		private void executeHelper(string cmd, object input)
		{
			// Ignore empty command lines.
			if (String.IsNullOrEmpty(cmd))
			{
				return;
			}

			// Create the pipeline object and make it available to the
			// ctrl-C handle through the currentPowerShell instance
			// variable.
			lock (this.instanceLock)
			{
				this.currentPowerShell = Shell.Create();
			}

			// Create a pipeline for this execution, and then place the 
			// result in the currentPowerShell variable so it is available 
			// to be stopped.
			try
			{
				this.currentPowerShell.Runspace = this.myRunSpace;
				this.currentPowerShell.AddScript(cmd);

				// Add the default outputter to the end of the pipe and then 
				// call the MergeMyResults method to merge the output and 
				// error streams from the pipeline. This will result in the 
				// output being written using the PSHost and PSHostUserInterface 
				// classes instead of returning objects to the host application.
				this.currentPowerShell.AddCommand("out-default");
				this.currentPowerShell.Commands.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);

				// If there is any input pass it in, otherwise just invoke the
				// the pipeline.
				if (input != null)
				{
					this.currentPowerShell.Invoke(new object[] { input });
				}
				else
				{
					this.currentPowerShell.Invoke();
				}
			}
			finally
			{
				// Dispose the PowerShell object and set currentPowerShell to null. 
				// It is locked because currentPowerShell may be accessed by the 
				// ctrl-C handler.
				lock (this.instanceLock)
				{
					this.currentPowerShell.Dispose();
					this.currentPowerShell = null;
				}
			}
		}

		/// <summary>
		/// To display an exception using the display formatter, 
		/// run a second pipeline passing in the error record.
		/// The runtime will bind this to the $input variable,
		/// which is why $input is being piped to the Out-String
		/// cmdlet. The WriteErrorLine method is called to make sure 
		/// the error gets displayed in the correct error color.
		/// </summary>
		/// <param name="e">The exception to display.</param>
		private void ReportException(Exception e)
		{
			if (e != null)
			{
				object error;
				IContainsErrorRecord icer = e as IContainsErrorRecord;
				if (icer != null)
				{
					error = icer.ErrorRecord;
				}
				else
				{
					error = (object)new ErrorRecord(e, "Host.ReportException", ErrorCategory.NotSpecified, null);
				}

				lock (this.instanceLock)
				{
					this.currentPowerShell = Shell.Create();
				}

				this.currentPowerShell.Runspace = this.myRunSpace;

				try
				{
					this.currentPowerShell.AddScript("$input").AddCommand("out-string");

					// Do not merge errors, this function will swallow errors.
					Collection<PSObject> result;
					PSDataCollection<object> inputCollection = new PSDataCollection<object>();
					inputCollection.Add(error);
					inputCollection.Complete();
					result = this.currentPowerShell.Invoke(inputCollection);

					if (result.Count > 0)
					{
						string str = result[0].BaseObject as string;
						if (!string.IsNullOrEmpty(str))
						{
							// Remove \r\n, which is added by the Out-String cmdlet.    
							this._powerShellHost.UI.WriteErrorLine(str.Substring(0, str.Length - 2));
						}
					}
				}
				finally
				{
					// Dispose of the pipeline and set it to null, locking it  because 
					// currentPowerShell may be accessed by the ctrl-C handler.
					lock (this.instanceLock)
					{
						this.currentPowerShell.Dispose();
						this.currentPowerShell = null;
					}
				}
			}
		}

		/// <summary>
		/// Basic script execution routine. Any runtime exceptions are
		/// caught and passed back to the Windows PowerShell engine to 
		/// display.
		/// </summary>
		/// <param name="cmd">Script to run.</param>
		private void Execute(string cmd)
		{
			try
			{
				// Execute the command with no input.
				this.executeHelper(cmd, null);
			}
			catch (RuntimeException rte)
			{
				this.ReportException(rte);
			}
		}

		/// <summary>
		/// Method used to handle control-C's from the user. It calls the
		/// pipeline Stop() method to stop execution. If any exceptions occur
		/// they are printed to the console but otherwise ignored.
		/// </summary>
		/// <param name="sender">See sender property documentation of  
		/// ConsoleCancelEventHandler.</param>
		/// <param name="e">See e property documentation of 
		/// ConsoleCancelEventHandler.</param>
		private void HandleControlC(object sender, ConsoleCancelEventArgs e)
		{
			try
			{
				lock (this.instanceLock)
				{
					if (this.currentPowerShell != null && this.currentPowerShell.InvocationStateInfo.State == PSInvocationState.Running)
					{
						this.currentPowerShell.Stop();
					}
				}

				e.Cancel = true;
			}
			catch (Exception exception)
			{
				this._powerShellHost.UI.WriteErrorLine(exception.ToString());
			}
		}

		/// <summary>
		/// Implements the basic listener loop. It sets up the ctrl-C handler, then
		/// reads a command from the user, executes it and repeats until the ShouldExit
		/// flag is set.
		/// </summary>
		public void InputLoop()
		{
			// Create the host and runspace instances for this interpreter. Note 
			// that this application doesn't support console files so only the 
			// default snap-ins will be available.
			this._powerShellHost = new PowerShellHost(this, _terminal);
			this.myRunSpace = RunspaceFactory.CreateRunspace(this._powerShellHost);
			this.myRunSpace.Open();

			try
			{
				try
				{
					lock (this.instanceLock)
					{
						this.currentPowerShell = Shell.Create();
						this.currentPowerShell.Runspace = this.myRunSpace;

						currentPowerShell.AddScript("cd $HOME");
						currentPowerShell.Invoke();
					}
				}

				finally
				{
					currentPowerShell.Dispose();
					currentPowerShell = null;
				}

				// Read commands to execute until ShouldExit is set by
				// the user calling "exit".
				while (!this.ShouldExit)
				{
					string prompt;
					string currentDirectory;

					try
					{
						lock (this.instanceLock)
						{
							this.currentPowerShell = Shell.Create();
							this.currentPowerShell.Runspace = this.myRunSpace;

							currentPowerShell.AddCommand("pwd");
							currentDirectory = currentPowerShell.Invoke()[0].ToString();
						}
					}

					finally
					{
						currentPowerShell.Dispose();
						currentPowerShell = null;
					}

					if (this._powerShellHost.IsRunspacePushed)
					{
						prompt = string.Format("\n[{0}] PS {1}> ", this.myRunSpace.ConnectionInfo.ComputerName, currentDirectory);
					}
					else
					{
						prompt = "\nPS " + currentDirectory + "> ";
					}

					this._powerShellHost.UI.Write(prompt);
					string cmd = _powerShellHost.UI.ReadLine();
					this.Execute(cmd);
				}
			}
			finally
			{
				// Dispose of the pipeline line and set it to null, locked because currentPowerShell
				// may be accessed by the ctrl-C handler...
				lock (this.instanceLock)
				{
					if (this.currentPowerShell != null)
					{
						this.currentPowerShell.Dispose();
						this.currentPowerShell = null;
					}
				}
			}

			OnLoggedOff(this, null);
		}
	}
}
