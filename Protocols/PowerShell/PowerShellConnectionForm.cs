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
	public partial class PowerShellConnectionForm : BaseConnectionForm<PowerShellConnection>
	{
		public PowerShellConnectionForm()
		{
			InitializeComponent();
		}

		protected override Control ConnectionWindow
		{
			get
			{
				return this;
			}
		}

		public override void Connect()
		{
		}
	}
}
