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
using RichardSzalay.HostsFileExtension.Client.Properties;
using RichardSzalay.HostsFileExtension.Client.Model;

namespace RichardSzalay.HostsFileExtension.Client.View
{
    public partial class EditHostEntryForm : TaskForm
    {
        private bool hasChanges = false;
        private bool canAccept = false;

        private const int MaxTabIndex = 7;

        private string[] addressValues;

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
                addressValues = addressProvider.GetAddresses();
                addressComboBox.Items.Clear();
                addressComboBox.Items.AddRange(addressValues);
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
            if (addressComboBox.Enabled)
            {
                hostEntry.Address = addressComboBox.Text;
            }

            if (hostnameTextBox.Enabled)
            {
                hostEntry.Hostname = hostnameTextBox.Text;
            }

            if (commentTextBox.Enabled)
            {
                hostEntry.Comment = commentTextBox.Text;
            }

            if (enabledCheckBox.Checked)
            {
                hostEntry.Enabled = enabledCheckBox.Checked;
            }
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

        private void OnFormFieldChanged(object sender, EventArgs e)
        {
            this.hasChanges = true;
            this.UpdateUIState();
        }

        private void UpdateUIState()
        {
            string address = addressComboBox.Text;
            string hostname = hostnameTextBox.Text;

            canAccept = (!addressComboBox.Enabled || IsAddressValid(address)) &&
                (!hostnameTextBox.Enabled || IsHostnameValid(hostname));

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
        private Model.HostEntryField editableFields;

        public HostEntry HostEntry
        {
            get { return hostEntry; }
            set
            {
                hostEntry = value;

                this.BindToForm();
            }
        }

        public Model.HostEntryField EditableFields
        {
            get { return editableFields; }
            set
            {
                editableFields = value;

                addressComboBox.Enabled = ((value & HostEntryField.Address) != 0);
                hostnameTextBox.Enabled = ((value & HostEntryField.Hostname) != 0);
                commentTextBox.Enabled = ((value & HostEntryField.Comment) != 0);
                enabledCheckBox.Enabled = ((value & HostEntryField.Enabled) != 0);

                if (addressComboBox.Enabled)
                {
                    addressComboBox.Items.Clear();
                    addressComboBox.Items.AddRange(addressValues);
                }
                else
                {
                    addressComboBox.Items.Clear();
                    addressComboBox.Text = Resources.FieldHasMultipleValues;
                }
               
                if (!hostnameTextBox.Enabled) hostnameTextBox.Text = Resources.FieldHasMultipleValues;
                if (!commentTextBox.Enabled) commentTextBox.Text = Resources.FieldHasMultipleValues;
            }
        }
    }
}
