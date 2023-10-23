using System;
using System.Windows.Forms;

namespace Standalone_Multicast
{
    public partial class frmMulticast : Form
    {
        private GeniePlugin.Interfaces.IHost _host;

        public frmMulticast(ref GeniePlugin.Interfaces.IHost host)
        {
            InitializeComponent();

            _host = host;
        }
        private void btnOk_Click(object sender, EventArgs e)
        {
            //if (cboSort.Text == "Bottom")
            //    _host.SendText("#var CircleCalc.Sort 1");
            //else
            //    _host.SendText("#var CircleCalc.Sort 0");
            //_host.SendText("#var save");
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
