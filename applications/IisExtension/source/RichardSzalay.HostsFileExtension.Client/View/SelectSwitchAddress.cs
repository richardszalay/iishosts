using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Web.Management.Client.Win32;
using RichardSzalay.HostsFileExtension.Client.Properties;

namespace RichardSzalay.HostsFileExtension.Client.View
{
    public partial class SelectSwitchAddress : TaskForm
    {
        private const int MaxTabIndex = 7;

        private readonly string[] addresses;

        public SelectSwitchAddress()
            : this(null, null)
        {
        }

        public SelectSwitchAddress(IServiceProvider serviceProvider, string[] addresses)
            : base(serviceProvider)
        {
            InitializeComponent();

            this.addresses = addresses;

            this.Text = Resources.SwitchAddressDialogTitle;
            addressLabel.Text = Resources.HostEntryForm_AddressLabel;

            ((Control)base.AcceptButton).TabIndex = MaxTabIndex + 1;
            ((Control)base.CancelButton).TabIndex = MaxTabIndex + 2;

            addressComboBox.Items.AddRange(this.addresses);
        }

        protected override void OnAccept()
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void addressComboBox_TextChanged(object sender, EventArgs e)
        {
            ((Control)base.AcceptButton).Enabled = (addressComboBox.Text.Length > 0);
        }

        public string Address { get { return addressComboBox.Text; } }
    }
}
