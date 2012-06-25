using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EasyConnect.Protocols
{
    public abstract class BaseProtocol<TConnection, TOptionsForm, TConnectionPanel> : IProtocol 
        where TConnection : IConnection
        where TOptionsForm : Form, IOptionsForm<TConnection>, new()
        where TConnectionPanel : BaseConnectionPanel, IConnectionPanel<TConnection>, new()
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

        public Form GetOptionsFormInDefaultsMode(IConnection connection)
        {
            return GetOptionsFormInDefaultsMode((TConnection) connection);
        }

        public BaseConnectionPanel CreateConnectionPanel(IConnection connection, Panel containerPanel, Form parentForm)
        {
            TConnectionPanel connectionPanel = new TConnectionPanel
                                                   {
                                                       Parent = containerPanel,
                                                       Location = new Point(0, 0),
                                                       Size = containerPanel.Size,
                                                       Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Right,
                                                       ContainerPanel = containerPanel,
                                                       ParentForm = parentForm,
                                                       Connection = (TConnection)connection
                                                   };

            return connectionPanel;
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
