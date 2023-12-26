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
            if (chkDebug.Checked)
                _host.SendText("#var Multicast.Debug 1");
            else
                _host.SendText("#var Multicast.Debug 0");

            bool _changed = false;
            if (txtAddress.Text != _host.get_Variable("Multicast.Address"))
            {
                _changed = true;
                _host.SendText("#var Multicast.Address " + txtAddress.Text);
            }

            if (txtPort.Text != _host.get_Variable("Multicast.Port"))
            {
                _changed = true;
                _host.SendText("#var Multicast.Port " + txtPort.Text);
            }

            if (_changed)
                _host.SendText("#echo Multicast address and/or port was changed. Please reload the Multicast plugin!");

            _host.SendText("#var save");
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
