using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EasyConnect.Protocols
{
    public abstract class BaseConnectionPanel : Control, IConnectionPanel
    {
        protected Panel _containerPanel = null;

        public abstract event EventHandler Connected;

        public abstract void Connect();

        public Panel ContainerPanel
        {
            get
            {
                return _containerPanel;
            }

            set
            {
                _containerPanel = value;
                Parent = value;
            }
        }

        public Form ParentForm
        {
            get;
            set;
        }
    }
}
