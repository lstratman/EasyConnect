using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyConnect
{
    public class BookmarksMethods
    {
        public void OpenToBookmarkGuids(Guid[] bookmarkGuids)
        {
            if (MainForm.ActiveInstance == null)
                return;

            MainForm.ActiveInstance.Invoke(MainForm.ConnectToBookmarksMethod, bookmarkGuids);
        }
    }
}
