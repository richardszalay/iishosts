using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Web.Management.Client.Win32;
using System.Net;
using System.Text.RegularExpressions;
using RichardSzalay.HostsFileExtension.Properties;

namespace RichardSzalay.HostsFileExtension.View
{
    public partial class EditHostEntryForm : TaskForm
    {
        private bool hasChanges = false;
        private bool canAccept = false;

        private const int MaxTabIndex = 7;

        public EditHostEntryForm()
            : this(null, null)
        {
        }

        public EditHostEntryForm(IServiceProvider serviceProvider, IAddressProvider addressProvider)
            : base(serviceProvider)
        {
            InitializeComponent();

            addressLabel.Text = Resources.HostEntryForm_AddressLabel;
            hostnameLabel.Text = Resources.HostEntryForm_HostnameLabel;
            commentLabel.Text = Resources.HostEntryForm_CommentLabel;
            enabledCheckBox.Text = Resources.HostEntryForm_EnabledLabel;

            if (addressProvider != null)
            {
                addressComboBox.Items.Clear();
                addressComboBox.Items.AddRange(addressProvider.GetAddresses());
            }

            ((Control)base.AcceptButton).TabIndex = MaxTabIndex + 1;
            ((Control)base.CancelButton).TabIndex = MaxTabIndex + 2;

            hostnameTextBox.Focus();
        }

        protected override void OnAccept()
        {
            BindFromForm();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BindFromForm()
        {
            hostEntry.Address = addressComboBox.Text;
            hostEntry.Hostname = hostnameTextBox.Text;
            hostEntry.Comment = commentTextBox.Text;
            hostEntry.Enabled = enabledCheckBox.Checked;
        }

        private void BindToForm()
        {
            addressComboBox.Text = hostEntry.Address;
            hostnameTextBox.Text = hostEntry.Hostname;
            commentTextBox.Text = hostEntry.Comment;
            enabledCheckBox.Checked = hostEntry.Enabled;

            hasChanges = false;
        }

        protected override bool CanAccept
        {
            get { return canAccept && hasChanges; }
        }

        private void OnTextBoxTextChanged(object sender, EventArgs e)
        {
            this.hasChanges = true;
            this.UpdateUIState();
        }

        private void OnCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            this.hasChanges = true;
            this.UpdateUIState();
        }

        private void UpdateUIState()
        {
            string address = addressComboBox.Text;
            string hostname = hostnameTextBox.Text;

            canAccept = IsAddressValid(address) &&
                IsHostnameValid(hostname);

            this.UpdateTaskForm();
        }

        private bool IsHostnameValid(string hostname)
        {
            return Regex.IsMatch(hostname, @"^[\w\.\-]+$");
        }

        private bool IsAddressValid(string address)
        {
            return Regex.IsMatch(address, @"^[^\s]+$");
        }

        private HostEntry hostEntry;

        public HostEntry HostEntry
        {
            get { return hostEntry; }
            set
            {
                hostEntry = value;

                this.BindToForm();
            }
        }
    }
}
