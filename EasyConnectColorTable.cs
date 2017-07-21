using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasyConnect
{
    public class EasyConnectColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected
        {
            get
            {
                return Color.FromArgb(66, 139, 202);
            }
        }

        public override Color MenuItemSelectedGradientBegin
        {
            get
            {
                return Color.FromArgb(66, 139, 202);
            }
        }

        public override Color MenuItemSelectedGradientEnd
        {
            get
            {
                return Color.FromArgb(66, 139, 202);
            }
        }

        public override Color ToolStripDropDownBackground
        {
            get
            {
                return Color.FromArgb(242, 242, 242);
            }
        }

        public override Color ImageMarginGradientBegin
        {
            get
            {
                return Color.FromArgb(242, 242, 242);
            }
        }

        public override Color ImageMarginGradientEnd
        {
            get
            {
                return Color.FromArgb(242, 242, 242);
            }
        }

        public override Color ImageMarginGradientMiddle
        {
            get
            {
                return Color.FromArgb(242, 242, 242);
            }
        }
    }
}
