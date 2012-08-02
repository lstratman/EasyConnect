using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Poderosa
{
    public partial class EditDisplayDialog : Form
    {
        public EditDisplayDialog()
        {
            InitializeComponent();
            Forms.DisplayOptionPanel panel = new Poderosa.Forms.DisplayOptionPanel();
            panel.Dock = DockStyle.Fill;
            
            this.Controls.Add(panel);
        }
    }
}