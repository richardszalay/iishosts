using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Client.Win32;
using System.Windows.Forms;
using Microsoft.Web.Management.Client;
using RichardSzalay.HostsFileExtension.Client.Properties;
using System.Drawing;
using System.Resources;
using System.IO;
using System.Collections;
using RichardSzalay.HostsFileExtension.Presenter;
using Microsoft.Web.Management.Server;
using RichardSzalay.HostsFileExtension.Client.View;
using RichardSzalay.HostsFileExtension.Client.Model;

namespace RichardSzalay.HostsFileExtension.Client.View
{
    /// <summary>
    /// GUI for managing entire hosts file
    /// </summary>
    public class ManageHostsModulePage : ModuleListPage, IManageHostsModulePage
    {
        public event EventHandler Refreshing;
        public event EventHandler Initialized;
        public event EventHandler ListItemDoubleClick;
        public event EventHandler SearchFilterChanged;
        public event EventHandler DeleteSelected;
        public event EventHandler EditSelected;
        public event EventHandler SelectionChanged;

        private ColumnHeader addressColumnHeader;
        private ColumnHeader hostnameColumnHeader;
        private ColumnHeader commentColumnHeader;
        private float oldListViewWidth = 0f;

        private TaskList taskList;

        public ManageHostsModulePage()
        {
            new ManageHostsModulePagePresenter(this);
        }

        private void CreateUserInterface()
        {
            oldListViewWidth = ListView.Width;

            hostnameColumnHeader = new ColumnHeader
            {
                Text = Resources.HostsPageHostNameColumn,
                Width = (int) ((float) ListView.Width*0.3f)
            };

            addressColumnHeader = new ColumnHeader
            {
                Text = Resources.HostsPageAddressColumn,
                Width = (int)((float)ListView.Width * 0.2f)
            };

            commentColumnHeader = new ColumnHeader
            {
                Text = Resources.HostsPageCommentColumn,
                Width = (int) ((float) ListView.Width*0.5f)
            };

            ListView.Columns.Add(hostnameColumnHeader);
            ListView.Columns.Add(addressColumnHeader);
            ListView.Columns.Add(commentColumnHeader);

            ListView.MultiSelect = true;
            ListView.SelectedIndexChanged += new EventHandler(ListView_SelectedIndexChanged);
            ListView.DoubleClick += new EventHandler(ListView_DoubleClick);

            ListView.KeyDown += new KeyEventHandler(ListView_KeyDown);
        }

        void ListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                this.OnDeleteSelected(e);
            }

            if (e.KeyCode == Keys.Enter)
            {
                this.OnEditSelected(e);
            }

            if (e.KeyCode == Keys.A && e.Control)
            {
                this.ListView.BeginUpdate();
                this.ListView.SelectedIndices.Clear();

                foreach (ListViewItem item in ListView.Items)
                {
                    item.Selected = true;
                }

                this.ListView.EndUpdate();
            }
        }

        private void OnDeleteSelected(EventArgs e)
        {
            var handler = this.DeleteSelected;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnEditSelected(EventArgs e)
        {
            var handler = this.EditSelected;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        void ListView_DoubleClick(object sender, EventArgs e)
        {
            this.OnListItemDoubleClick(EventArgs.Empty);
        }

        void ListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            //this.Update();
            this.OnSelectionChanged(e);
        }

        private void OnSelectionChanged(EventArgs e)
        {
            var handler = this.SelectionChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);

            this.ReorderListViewColumns();
        }

        protected virtual void OnListItemDoubleClick(EventArgs e)
        {
            EventHandler handler = this.ListItemDoubleClick;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void ReorderListViewColumns()
        {
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

        public string SearchFilter { get; private set; }

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
                    new ModuleListPageSearchField(Resources.AllSearchField, Resources.AllSearchField)
                };
            }
        }

        protected override void  OnSearch(ModuleListPageSearchOptions options)
        {
            this.SearchFilter = options.Text;

            this.OnSearchFilterChanged(EventArgs.Empty);
        }

        private void OnSearchFilterChanged(EventArgs e)
        {
            EventHandler handler = this.SearchFilterChanged;

            if (handler != null)
            {
                handler(this, e);
            }
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

        public void SetHostEntries(IEnumerable<HostEntryViewModel> hostEntries)
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

        private ListViewItem CreateListViewItem(HostEntryViewModel model)
        {
            ListViewItem item = new ListViewItem();
            item.Text = model.HostEntry.Address;
            item.SubItems.Add(model.HostEntry.Comment);
            item.SubItems.Add(model.HostEntry.Hostname);
            item.Tag = model;
            item.Font = GetFont(model, item.Font);
            item.ForeColor = GetFontColor(model, item.ForeColor);
            item.UseItemStyleForSubItems = true;

            return item;
        }

        private Color GetFontColor(HostEntryViewModel model, Color defaultColor)
        {
            if (model.Conflicted)
            {
                return model.HostEntry.Enabled
                    ? Color.Red
                    : Color.Salmon;
            }
            else
            {
                return model.HostEntry.Enabled
                    ? defaultColor
                    : Color.SlateGray;
            }
        }

        private Font GetFont(HostEntryViewModel model, Font prototype)
        {
            return model.HostEntry.IsNew
                ? new Font(prototype, FontStyle.Italic)
                : prototype;
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
            get { return false; }
        }

        public IEnumerable<HostEntryViewModel> SelectedEntries
        {
            get
            {
                return this.ListView.SelectedItems
                    .OfType<ListViewItem>()
                    .Select(c => (HostEntryViewModel)c.Tag);
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

        public ConfigurationPathType ConfigurationPathType
        {
            get { return Connection.ConfigurationPath.PathType; }
        }

        Module IManageHostsModulePage.Module
        {
            get { return this.Module; }
        }

        #region IServiceProvider Members

        public new object GetService(Type serviceType)
        {
            return this.ServiceProvider.GetService(serviceType);
        }

        #endregion

        Connection IManageHostsModulePage.Connection
        {
            get { return base.Connection; }
        }
    }
}
