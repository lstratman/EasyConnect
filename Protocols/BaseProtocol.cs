using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EasyConnect.Protocols
{
    public abstract class BaseProtocol<TConnection, TOptionsForm, TConnectionForm> : IProtocol 
        where TConnection : IConnection
        where TOptionsForm : Form, IOptionsForm<TConnection>, new()
        where TConnectionForm : BaseConnectionForm, IConnectionForm<TConnection>, new()
    {
        public abstract string ProtocolPrefix
        {
            get;
        }

        public abstract string ProtocolTitle
        {
            get;
        }

        public abstract Icon ProtocolIcon
        {
            get;
        }

        public Type ConnectionType
        {
            get
            {
                return typeof (TConnection);
            }
        }

        public virtual Form GetOptionsForm()
        {
            return new TOptionsForm();
        }

        public Form GetOptionsForm(IConnection connection)
        {
            return GetOptionsForm((TConnection) connection);
        }

        public Form GetOptionsFormInDefaultsMode()
        {
            TConnection defaults = (TConnection)ConnectionFactory.GetDefaults(GetType());

            return GetOptionsFormInDefaultsMode(defaults);
        }

        public BaseConnectionForm CreateConnectionForm(IConnection connection, Panel containerPanel)
        {
            TConnectionForm connectionForm = new TConnectionForm
                                                   {
                                                       Location = new Point(0, 0),
                                                       Dock = DockStyle.Fill,
                                                       FormBorderStyle = FormBorderStyle.None,
                                                       TopLevel = false,
                                                       Connection = (TConnection)connection
                                                   };

            containerPanel.Controls.Add(connectionForm);
            connectionForm.Show();

            return connectionForm;
        }

        public virtual Form GetOptionsForm(TConnection connection)
        {
            return new TOptionsForm
                       {
                           Connection = connection
                       };
        }

        public virtual Form GetOptionsFormInDefaultsMode(TConnection connection)
        {
            return new TOptionsForm
                       {
                           Connection = connection,
                           DefaultsMode = true
                       };
        }
    }
}
