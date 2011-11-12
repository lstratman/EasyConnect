using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyConnect
{
    public class FavoritesFolder
    {
        protected List<FavoritesFolder> _childFolders = new List<FavoritesFolder>();
        protected List<RDCConnection> _favorites = new List<RDCConnection>();

        public string Name
        {
            get;
            set;
        }

        public List<FavoritesFolder> ChildFolders
        {
            get
            {
                return _childFolders;
            }
        }

        public List<RDCConnection> Favorites
        {
            get
            {
                return _favorites;
            }
        }
    }
}
