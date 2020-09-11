using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel.Design;

namespace EasyConnect.Common
{
    [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]
    public partial class MaterialCard : UserControl
    {
        public MaterialCard()
        {
            InitializeComponent();
        }

        private void MaterialCard_Resize(object sender, EventArgs e)
        {
            _panelLeft.Height = Height - _panelTopLeft.Height - _panelBottomLeft.Height;
            _panelRight.Height = _panelLeft.Height;
            _panelRight.Location = new Point(Width - _panelRight.Width, _panelRight.Location.Y);
            _panelTop.Width = Width - _panelTopLeft.Width - _panelTopRight.Width;
            _panelBottom.Width = _panelTop.Width;
            _panelBottom.Location = new Point(_panelBottom.Location.X, Height - _panelBottom.Height);
            _panelBottomLeft.Location = new Point(_panelBottomLeft.Location.X, Height - _panelBottomLeft.Height);
            _panelBottomRight.Location = new Point(Width - _panelBottomRight.Width, Height - _panelBottomRight.Height);
            _panelTopRight.Location = new Point(Width - _panelTopRight.Width, _panelTopRight.Location.Y);
        }
    }
}
