using System;

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