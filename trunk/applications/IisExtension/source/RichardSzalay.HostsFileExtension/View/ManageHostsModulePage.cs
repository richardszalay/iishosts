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
using System.Drawing;
using System.Resources;
using System.IO;
using System.Collections;

namespace RichardSzalay.HostsFileExtension.View
{
    /// <summary>
    /// GUI for managing entire hosts file
    /// </summary>
    public class ManageHostsModulePage : ModuleListPage, IManageHostsModulePage
    {
        public event EventHandler Refreshing;
        public event EventHandler Initialized;

        private ColumnHeader addressColumnHeader;
        private ColumnHeader hostnameColumnHeader;
        private ColumnHeader commentColumnHeader;
        private float oldListViewWidth = 0f;

        private TaskList taskList;

        private ManageHostsModulePagePresenter presenter;

        public ManageHostsModulePage()
        {
            presenter = new ManageHostsModulePagePresenter(this);
        }

        private void CreateUserInterface()
        {
            oldListViewWidth = ListView.Width;

            addressColumnHeader = new ColumnHeader();
            addressColumnHeader.Text = "Address";
            addressColumnHeader.Width = (int)((float)ListView.Width * 0.2f);

            hostnameColumnHeader = new ColumnHeader();
            hostnameColumnHeader.Text = "Host name";
            hostnameColumnHeader.Width = (int)((float)ListView.Width * 0.3f);

            commentColumnHeader = new ColumnHeader();
            commentColumnHeader.Text = "Comment";
            commentColumnHeader.Width = (int)((float)ListView.Width * 0.5f);

            ListView.Columns.Add(addressColumnHeader);
            ListView.Columns.Add(hostnameColumnHeader);
            ListView.Columns.Add(commentColumnHeader);

            ListView.MultiSelect = true;
            ListView.SelectedIndexChanged += new EventHandler(ListView_SelectedIndexChanged);
            ListView.DoubleClick += new EventHandler(ListView_DoubleClick);
        }

        void ListView_DoubleClick(object sender, EventArgs e)
        {
            if (this.ListView.SelectedIndices.Count == 1)
            {
                this.presenter.EditSelectedEntry();
            }
        }

        void ListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Update();
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);

            if (ListView.Width != 0)
            {
                float totalWidth = oldListViewWidth;

                float viewWidth = (float)ListView.Width;

                addressColumnHeader.Width = (int)((float)addressColumnHeader.Width / totalWidth * viewWidth);
                hostnameColumnHeader.Width = (int)((float)hostnameColumnHeader.Width / totalWidth * viewWidth);
                commentColumnHeader.Width = (int)((float)commentColumnHeader.Width / totalWidth * viewWidth);

                oldListViewWidth = (float)ListView.Width;
            }
        }

        protected override bool CanSearch
        {
            get { return true; }
        }

        protected override bool CanInstantSearch
        {
            get { return true;  }
        }

        protected override ModuleListPageSearchField[] SearchFields
        {
            get
            {
                return new ModuleListPageSearchField[]
                {
                    new ModuleListPageSearchField("Address", "Address"),
                    new ModuleListPageSearchField("Hostname", "Hostname")
                };
            }
        }

        protected override void  OnSearch(ModuleListPageSearchOptions options)
        {
            presenter.FilterEntries(options.Text);
        }



        protected override bool CanRefresh
        {
            get { return true; }
        }

        protected override void Refresh()
        {
            this.OnRefreshing(EventArgs.Empty);
        }

        protected override void InitializeListPage()
        {
            this.CreateUserInterface();

            this.OnInitialized(EventArgs.Empty);
        }

        #region IManageHostsModulePage Members

        protected virtual void OnInitialized(EventArgs e)
        {
            EventHandler handler = Initialized;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnRefreshing(EventArgs e)
        {
            EventHandler handler = Refreshing;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void SetHostEntries(IEnumerable<HostEntry> hostEntries)
        {
            int[] selectedIndicies = this.ListView.SelectedIndices.OfType<int>().ToArray();

            ListView.ListViewItemCollection items = ListView.Items;

            items.Clear();

            var newEntries = hostEntries.Select(c => CreateListViewItem(c));

            foreach (ListViewItem item in newEntries)
            {
                items.Add(item);
            }

            for (int i = 0; i < selectedIndicies.Length; i++)
            {
                int selectedIndex = selectedIndicies[i];

                if (selectedIndex < items.Count)
                {
                    items[selectedIndex].Selected = true;
                }
            }

            this.Update();
        }

        private ListViewItem CreateListViewItem(HostEntry entry)
        {
            ListViewItem item = new ListViewItem();
            item.Text = entry.Address;
            item.SubItems.Add(entry.Hostname);
            item.SubItems.Add(entry.Comment);
            item.Tag = entry;
            item.ForeColor = GetFontColor(entry, item.ForeColor);

            return item;
        }

        private Color GetFontColor(HostEntry entry, Color defaultColor)
        {
            return entry.Enabled
                ? defaultColor
                : Color.LightGray;
        }

        #endregion

        protected override TaskListCollection Tasks
        {
            get
            {
                TaskListCollection tasks = base.Tasks;

                tasks.Add(this.taskList);

                return tasks;
            }
        }

        private bool CanApplyChanges
        {
            get
            {
                return presenter.HasChanges;
            }
        }

        public IEnumerable<HostEntry> SelectedEntries
        {
            get
            {
                return this.ListView.SelectedItems
                    .OfType<ListViewItem>()
                    .Select(c => (HostEntry)c.Tag);
            }
        }

        public void SetTaskList(TaskList taskList)
        {
            this.taskList = taskList;
        }

        IServiceProvider IManageHostsModulePage.ServiceProvider
        {
            get { return base.ServiceProvider; }
        }

        DialogResult IManageHostsModulePage.ShowDialog(DialogForm form)
        {
            return base.ShowDialog(form);
        }

        T IManageHostsModulePage.CreateProxy<T>()
        {
            return (T)base.CreateProxy(typeof(T));
        }
    }
}
