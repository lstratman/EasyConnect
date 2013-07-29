using System;

namespace EasyConnect
{
	/// <summary>
	/// Remoting methods used to tell an existing application process to open one or more history items in its window.
	/// </summary>
	public class HistoryMethods : MarshalByRefObject
	{
		/// <summary>
		/// Open the history entry matching <paramref name="historyGuid"/> in a new tab.
		/// </summary>
		/// <param name="historyGuid">Identifier for the history entry that we should open.</param>
		public void OpenToHistoryGuid(Guid historyGuid)
		{
			if (MainForm.ActiveInstance == null)
				return;

			MainForm.ActiveInstance.Invoke(MainForm.ConnectToHistoryMethod, historyGuid);
		}
	}
}