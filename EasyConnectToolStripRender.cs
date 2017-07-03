using System.Drawing;
using System.Windows.Forms;

namespace EasyConnect
{
    public class EasyConnectToolStripRender : ToolStripProfessionalRenderer
    {
        public EasyConnectToolStripRender() : base(new EasyConnectColorTable())
        {
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            if (e.Item.Selected)
            {
                e.TextColor = Color.White;
            }

            base.OnRenderItemText(e);
        }
    }
}