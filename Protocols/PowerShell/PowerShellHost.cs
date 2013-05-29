using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Threading;
using WalburySoftware;

namespace EasyConnect.Protocols.PowerShell
{
	/// <summary>
	/// Implementation of the PSHost abstract class for wiring up a PowerShell session with a <see cref="TerminalControl"/> instance.
	/// </summary>
	public class PowerShellHost : PSHost, IHostSupportsInteractiveSession
	{
		/// <summary>
		/// The identifier of this PSHost implementation.
		/// </summary>
		protected static Guid _instanceId = Guid.NewGuid();

		/// <summary>
		/// A reference to the runspace used to start an interactive session.
		/// </summary>
		public Runspace PushedRunspace = null;

		/// <summary>
		/// The culture information of the thread that created this object.
		/// </summary>
		protected CultureInfo _originalCultureInfo = Thread.CurrentThread.CurrentCulture;

		/// <summary>
		/// The UI culture information of the thread that created this object.
		/// </summary>
		protected CultureInfo _originalUiCultureInfo = Thread.CurrentThread.CurrentUICulture;

		/// <summary>
		/// A reference to the PSHost implementation.
		/// </summary>
		protected PowerShellConnectionForm _parentForm;

		/// <summary>
		/// A reference to the implementation of the PSHostUserInterface class.
		/// </summary>
		protected PowerShellHostUi _powerShellHostUi;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="parentForm">The connection form that will display the PowerShell console.</param>
		/// <param name="terminal">Terminal control that will display the PowerShell console.</param>
		/// <param name="executeHelper">Method used to execute PowerShell commands within the current session.</param>
		public PowerShellHost(PowerShellConnectionForm parentForm, TerminalControl terminal, Func<string, Collection<PSObject>> executeHelper)
		{
			_parentForm = parentForm;
			_powerShellHostUi = new PowerShellHostUi(terminal, executeHelper);
		}

		/// <summary>
		/// Flag indicating whether the console is currently reading input from the user.
		/// </summary>
		public bool ReadingInput
		{
			get
			{
				return _powerShellHostUi.ReadingInput;
			}
		}

		/// <summary>
		/// Flag indicating whether the console is currently at the command prompt reading a command to run from the user.
		/// </summary>
		public bool AtCommandPrompt
		{
			get
			{
				return (UI as PowerShellHostUi).AtCommandPrompt;
			}

			set
			{
				(UI as PowerShellHostUi).AtCommandPrompt = value;
			}
		}

		/// <summary>
		/// Gets the culture information to use. This implementation returns a snapshot of the culture information of the thread that created this object.
		/// </summary>
		public override CultureInfo CurrentCulture
		{
			get
			{
				return _originalCultureInfo;
			}
		}

		/// <summary>
		/// Gets the UI culture information to use. This implementation returns a snapshot of the UI culture information of the thread that created this 
		/// object.
		/// </summary>
		public override CultureInfo CurrentUICulture
		{
			get
			{
				return _originalUiCultureInfo;
			}
		}

		/// <summary>
		/// Gets an identifier for this host. This implementation always returns the GUID allocated at instantiation time.
		/// </summary>
		public override Guid InstanceId
		{
			get
			{
				return _instanceId;
			}
		}

		/// <summary>
		/// Gets a string that contains the name of this host implementation.
		/// </summary>
		public override string Name
		{
			get
			{
				return "EasyConnect";
			}
		}

		/// <summary>
		/// Gets an instance of the implementation of the <see cref="PSHostUserInterface"/> class for this application.
		/// </summary>
		public override PSHostUserInterface UI
		{
			get
			{
				return _powerShellHostUi;
			}
		}

		/// <summary>
		/// Gets the version object for this application. Typically this should match the version resource in the application.
		/// </summary>
		public override Version Version
		{
			get
			{
				return Assembly.GetExecutingAssembly().GetName().Version;
			}
		}

		/// <summary>
		/// Gets a value indicating whether a request to open a PSSession has been made.
		/// </summary>
		public bool IsRunspacePushed
		{
			get
			{
				return PushedRunspace != null;
			}
		}

		/// <summary>
		/// Gets or sets the runspace used by the PSSession.
		/// </summary>
		public Runspace Runspace
		{
			get
			{
				return _parentForm.Runspace;
			}

			internal set
			{
				_parentForm.Runspace = value;
			}
		}

		/// <summary>
		/// Exit the currently-running PowerShell session.
		/// </summary>
		public void Exit()
		{
			_powerShellHostUi.EndInput();
		}

		/// <summary>
		/// Stop the currently-running PowerShell pipeline.
		/// </summary>
		public void StopCurrentPipeline()
		{
			if (ReadingInput)
				_powerShellHostUi.StopCurrentPipeline();
		}

		/// <summary>
		/// Add a command executed by the user to the history list.
		/// </summary>
		/// <param name="command">Command to add to the history.</param>
		public void AddToCommandHistory(string command)
		{
			(UI as PowerShellHostUi).AddToCommandHistory(command);
		}

		/// <summary>
		/// This API Instructs the host to interrupt the currently running  pipeline and start a new nested input loop.
		/// </summary>
		public override void EnterNestedPrompt()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// This API instructs the host to exit the currently running input loop.
		/// </summary>
		public override void ExitNestedPrompt()
		{
			_powerShellHostUi.EndInput();
		}

		/// <summary>
		/// This API is called before an external application process is started. Typically it is used to save state so that the parent can restore state that 
		/// has been modified by a child process (after the child exits).
		/// </summary>
		public override void NotifyBeginApplication()
		{
		}

		/// <summary>
		/// This API is called after an external application process finishes. Typically it is used to restore state that a child process has altered.
		/// </summary>
		public override void NotifyEndApplication()
		{
		}

		/// <summary>
		/// Indicate to the connection window that an exit has been requested. Pass the exit code that the connection window should use when exiting.
		/// </summary>
		/// <param name="exitCode">The exit code that the connection window should use.</param>
		public override void SetShouldExit(int exitCode)
		{
			_parentForm.ShouldExit = true;
			_parentForm.ExitCode = exitCode;
		}

		/// <summary>
		/// Requests to close a <see cref="PSSession"/>.
		/// </summary>
		public void PopRunspace()
		{
			Runspace = PushedRunspace;
			PushedRunspace = null;
		}

		/// <summary>
		/// Requests to open a <see cref="PSSession"/>.
		/// </summary>
		/// <param name="runspace">Runspace to use.</param>
		public void PushRunspace(Runspace runspace)
		{
			PushedRunspace = Runspace;
			Runspace = runspace;
		}
	}
}