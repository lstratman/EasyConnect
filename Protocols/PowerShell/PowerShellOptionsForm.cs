using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EasyConnect.Protocols.PowerShell
{
	public partial class PowerShellOptionsForm : Form, IOptionsForm<PowerShellConnection>
	{
		public PowerShellOptionsForm()
		{
			InitializeComponent();
		}

		IConnection IOptionsForm.Connection
		{
			get
			{
				return Connection;
			}

			set
			{
				Connection = (PowerShellConnection)value;
			}
		}

		public PowerShellConnection Connection
		{
			get;
			set;
		}

		public bool DefaultsMode
		{
			get;
			set;
		}
	}
}
