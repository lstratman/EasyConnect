using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UltraRDC
{
    public class HistoryMethods : MarshalByRefObject
    {
        public void OpenToHistoryGuid(Guid historyGuid)
        {
            if (MainForm.ActiveInstance == null)
                return;

            MainForm.ActiveInstance.Invoke(MainForm.ConnectToHistoryMethod, historyGuid);
        }
    }
}
