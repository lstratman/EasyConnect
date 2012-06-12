using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EasyConnect.Protocols
{
    public interface IProtocol
    {
        string ProtocolPrefix
        {
            get;
        }

        string ProtocolTitle
        {
            get;
        }

        Type ConnectionType
        {
            get;
        }

        Form GetOptionsForm();

        Form GetOptionsForm(IConnection connection);

        Form GetOptionsFormInDefaultsMode(IConnection connection);

        BaseConnectionPanel CreateConnectionPanel(IConnection connection, Panel containerPanel, Form parentForm);
    }
}
