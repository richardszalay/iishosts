using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RichardSzalay.HostsFileExtension.View;

namespace RichardSzalay.HostsFileExtension.Presenter
{
    public class ManageHostsModulePagePresenter
    {
        private IManageHostsModulePage view;
        private HostsFile hostsFile;

        public ManageHostsModulePagePresenter(IManageHostsModulePage view)
        {
            this.view = view;
            this.view.Initialized += new EventHandler(view_HandleCreated);
        }

        void view_HandleCreated(object sender, EventArgs e)
        {
            this.UpdateData();
        }

        public bool HasChanges
        {
            get
            {
                return hostsFile.IsDirty;
            }
        }

        private void UpdateData()
        {
            hostsFile = new HostsFile();

            view.HostEntries.Clear();

            var newEntries = hostsFile.Entries.Select(c =>
                {
                    ListViewItem item = new ListViewItem();
                    item.SubItems.Add(c.Address);
                    item.SubItems.Add(c.Hostname);
                    item.Checked = c.Enabled;
                    item.Tag = c;

                    return item;
                });

            foreach(ListViewItem item in newEntries)
            {
                view.HostEntries.Add(item);
            }
        }

        
    }
}
