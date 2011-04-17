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

        public EditHostEntryForm(IServiceProvider serviceProvider, string[] addresses)
            : base(serviceProvider)
        {
            InitializeComponent();

            addressLabel.Text = Resources.HostEntryForm_AddressLabel;
            hostnameLabel.Text = Resources.HostEntryForm_HostnameLabel;
            commentLabel.Text = Resources.HostEntryForm_CommentLabel;
            enabledCheckBox.Text = Resources.HostEntryForm_EnabledLabel;

            if (addresses != null)
            {
                addressValues = addresses;
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
            if (AddressChanged)
            {
                hostEntry.Address = addressComboBox.Text;
            }

            if (HostnameChanged)
            {
                hostEntry.Hostname = hostnameTextBox.Text;
            }

            if (CommentChanged)
            {
                hostEntry.Comment = commentTextBox.Text;
            }

            if (EnabledChanged)
            {
                hostEntry.Enabled = enabledCheckBox.Checked;
            }
        }

        private void BindToForm()
        {
            AddressChanged = false;
            HostnameChanged = false;
            CommentChanged = false;
            EnabledChanged = false;

            addressComboBox.Text = ((editableFields & HostEntryField.Address) != 0)
                ? hostEntry.Address
                : Resources.FieldHasMultipleValues;

            hostnameTextBox.Text = ((editableFields & HostEntryField.Hostname) != 0)
                ? hostEntry.Hostname
                : Resources.FieldHasMultipleValues;

            commentTextBox.Text = ((editableFields & HostEntryField.Comment) != 0)
                ? hostEntry.Comment
                : Resources.FieldHasMultipleValues;


            if ((editableFields & HostEntryField.Enabled) != 0)
            {
                enabledCheckBox.Checked = hostEntry.Enabled;
            }
            else
            {
                enabledCheckBox.CheckState = CheckState.Indeterminate;
            }

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

            AddressChanged = (address != Resources.FieldHasMultipleValues);
            HostnameChanged = (hostname != Resources.FieldHasMultipleValues);
            CommentChanged = (commentTextBox.Text != Resources.FieldHasMultipleValues);
            EnabledChanged = (enabledCheckBox.CheckState != CheckState.Indeterminate);

            canAccept = (IsAddressValid(address)) &&
                (IsHostnameValid(hostname));

            this.UpdateTaskForm();
        }

        private bool IsHostnameValid(string hostname)
        {
            return hostname == Resources.FieldHasMultipleValues ||
                Regex.IsMatch(hostname, @"^[\w\.\-]+$");
        }

        private bool IsAddressValid(string address)
        {
            return address == Resources.FieldHasMultipleValues || 
                Regex.IsMatch(address, @"^[^\s]+$");
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
            set { editableFields = value; }
        }

        public Model.HostEntryField FieldChanges
        {
            get
            {
                var fields = Model.HostEntryField.None;

                if (AddressChanged) fields |= HostEntryField.Address;
                if (HostnameChanged) fields |= HostEntryField.Hostname;
                if (CommentChanged) fields |= HostEntryField.Comment;
                if (EnabledChanged) fields |= HostEntryField.Enabled;

                return fields;
            }
        }

        private bool AddressChanged { get; set; }
        private bool HostnameChanged { get; set; }
        private bool CommentChanged { get; set; }
        private new bool EnabledChanged { get; set; }
    }
}
