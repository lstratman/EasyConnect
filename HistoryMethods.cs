using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyConnect
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
