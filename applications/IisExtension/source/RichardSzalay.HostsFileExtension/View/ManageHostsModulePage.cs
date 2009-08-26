using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Client.Win32;
using Microsoft.Web.Administration;
using System.Windows.Forms;
using Microsoft.Web.Management.Client;
using RichardSzalay.HostsFileExtension.Properties;
using RichardSzalay.HostsFileExtension.Presenter;

namespace RichardSzalay.HostsFileExtension.View
{
    /// <summary>
    /// GUI for managing entire hosts file
    /// </summary>
    public class ManageHostsModulePage : ModuleListPage, IManageHostsModulePage
    {
        public event EventHandler Refreshing;

        private ColumnHeader enabledColumnHeader;
        private ColumnHeader addressColumnHeader;
        private ColumnHeader hostnameColumnHeader;
        

        public ManageHostsModulePage()
        {
            ManageHostsModulePagePresenter presenter = new ManageHostsModulePagePresenter(this);
        }

        private void CreateUserInterface()
        {
            ListView.CheckBoxes = true;
            ListView.ItemCheck += new ItemCheckEventHandler(ListView_ItemCheck);

            enabledColumnHeader = new ColumnHeader();
            enabledColumnHeader.Text = "Enabled";

            addressColumnHeader = new ColumnHeader();
            addressColumnHeader.Text = "Address";

            hostnameColumnHeader = new ColumnHeader();
            hostnameColumnHeader.Text = "Host name";

            ListView.Columns.Add(enabledColumnHeader);
            ListView.Columns.Add(addressColumnHeader);
            ListView.Columns.Add(hostnameColumnHeader);
        }

        void ListView_ItemCheck(object sender, ItemCheckEventArgs e)
        {
        }

        protected override bool CanRefresh
        {
            get { return true; }
        }

        protected override void Refresh()
        {
           base.Refresh();
        }

        protected override void InitializeListPage()
        {
            this.CreateUserInterface();

            OnInitialized();
        }

        #region IManageHostsModulePage Members

        public event EventHandler Initialized;

        protected virtual void OnInitialized()
        {
            EventHandler handler = Initialized;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public ListView.ListViewItemCollection HostEntries
        {
            get
            {
                return ListView.Items;
            }
        }

        #endregion
    }
}
