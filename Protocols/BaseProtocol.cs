using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EasyConnect.Protocols
{
    public abstract class BaseProtocol<TConnection, TOptionsForm, TConnectionForm> 
        where TConnection : IConnection
        where TOptionsForm : Form, IOptionsForm<TConnection>, new()
        where TConnectionForm : Form, IConnectionForm<TConnection>, new()
    {
        public abstract string ProtocolPrefix
        {
            get;
        }

        public abstract string ProtocolTitle
        {
            get;
        }

        public virtual Form GetOptionsForm()
        {
            return new TOptionsForm();
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

        public virtual TConnectionForm CreateConnectionForm(TConnection connection, Form parentWindow)
        {
            TConnectionForm connectionForm = new TConnectionForm
                                                 {
                                                     MdiParent = parentWindow
                                                 };

            return connectionForm;
        }
    }
}
