using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RichardSzalay.HostsFileExtension.View;
using System.Drawing;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Management.Client.Win32;
using RichardSzalay.HostsFileExtension.Properties;
using System.Collections;
using System.Resources;
using RichardSzalay.HostsFileExtension.Service;
using Microsoft.Web.Management.Server;
using Microsoft.Web.Administration;
using Binding = Microsoft.Web.Administration.Binding;
using System.Windows.Forms;

namespace RichardSzalay.HostsFileExtension.Presenter
{
    public class ManageHostsModulePagePresenter
    {
        private IManageHostsModulePage view;
        private IEnumerable<HostEntry> hostEntries;

        private ManageHostsFileModuleProxy proxy;

        private IAddressProvider addressProvider;
        private IBindingInfoProvider bindingInfoProvider;
        

        private string filter;

        public ManageHostsModulePagePresenter(IManageHostsModulePage view)
        {
            this.view = view;
            this.view.Initialized += new EventHandler(view_Initialized);
            this.view.Refreshing += new EventHandler(view_Refreshing);
            this.view.ListItemDoubleClick += new EventHandler(view_ListItemDoubleClick);
            this.view.SearchFilterChanged += new EventHandler(view_SearchFilterChanged);
        }

        void view_SearchFilterChanged(object sender, EventArgs e)
        {
            this.DisplayEntries();
        }

        void view_ListItemDoubleClick(object sender, EventArgs e)
        {
            IList<HostEntry> hostEntries = this.view.SelectedEntries.ToList();

            if (hostEntries.Count == 1)
            {
                this.EditSelectedEntry();
            }
        }

        void view_Refreshing(object sender, EventArgs e)
        {
            this.UpdateData();
        }

        void view_Initialized(object sender, EventArgs e)
        {
            this.addressProvider = (IAddressProvider)view.GetService(typeof(IAddressProvider));
            this.bindingInfoProvider = (IBindingInfoProvider)view.GetService(typeof(IBindingInfoProvider));

            this.proxy = view.CreateProxy<ManageHostsFileModuleProxy>();

            view.SetTaskList(new ManageHostsTaskList(this));

            this.UpdateData();
        }

        public bool HasChanges
        {
            get
            {
                return false;
            }
        }

        private void UpdateData()
        {
            this.hostEntries = this.proxy.GetEntries();

            if (view.ConfigurationPathType == ConfigurationPathType.Site)
            {
                this.hostEntries = this.FilterSiteEntries(this.hostEntries);
            }

            DisplayEntries();
        }

        private IEnumerable<HostEntry> FilterSiteEntries(IEnumerable<HostEntry> allEntries)
        {
            const string AnyAddress = "*";

            string defaultAddress = this.addressProvider.GetAddresses()[0];

            List<HostEntry> newEntries = new List<HostEntry>();

            IEnumerable<Binding> bindings = this.bindingInfoProvider.GetBindings(view.Connection)
                .Where(c => c.Protocol.StartsWith("http") || c.Protocol == "tcp");

            IList<string> siteHosts = bindings.Select(c => c.Host).Distinct().ToList();
            IList<string> invalidHosts = new List<string>(siteHosts);
            
            IList<string> addedHosts = new List<string>();

            foreach(Binding binding in bindings)
            {
                string host = binding.Host;
                string address = GetBindingAddress(binding);
                bool isAnyAddress = (address == AnyAddress);

                if (isAnyAddress)
                {
                    address = defaultAddress;
                }

                bool alreadyAdded = addedHosts.Contains(binding.Host);

                if (!alreadyAdded)
                {
                    IEnumerable<HostEntry> matchingHosts = allEntries.Where(c => c.Hostname == host);

                    bool hostAlreadyHasEntry = (matchingHosts.Count() > 0);

                    if (hostAlreadyHasEntry)
                    {
                        if (!isAnyAddress)
                        {
                            bool existingEntryHasSameAddress =
                                matchingHosts.Where(c => c.Address == address).Count() != 0;

                            if (existingEntryHasSameAddress)
                            {
                                invalidHosts.Remove(host);
                            }
                        }
                    }
                    else
                    {
                        newEntries.Add(new HostEntry(host, address, null));
                        invalidHosts.Remove(host);
                    }
                }
            }

            return allEntries
                .Where(c => siteHosts.Contains(c.Hostname))
                .Union(newEntries);
        }

        private string GetBindingAddress(Binding binding)
        {
            return binding.BindingInformation.Split(':')[0];
        }

        private IEnumerable<HostEntry> SelectedEntries
        {
            get { return view.SelectedEntries; }
        }

        private bool CanApplyChanges
        {
            get { return true; }
        }

        private void AddTask()
        {
            string defaultAddress = addressProvider.GetAddresses()[0];

            using (var form = new EditHostEntryForm(view.ServiceProvider, addressProvider))
            {
                form.Text = Resources.AddHostEntryDialogTitle;

                HostEntry selectedEntry = view.SelectedEntries.FirstOrDefault();

                // TODO: Fix this default value
                form.HostEntry = (selectedEntry != null && selectedEntry.IsNew)
                    ? selectedEntry
                    : new HostEntry("", defaultAddress, null);

                DialogResult result = view.ShowDialog(form);

                if (result == DialogResult.OK)
                {
                    this.proxy.AddEntries(new List<HostEntry>() { form.HostEntry });
                    this.UpdateData();
                }
            }
        }

        private void AddMissingTask()
        {
            DialogResult result = MessageBox.Show(view, Resources.AddMissingConfirmation, Resources.AddMissingConfirmationTitle,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                IList<HostEntry> entries = this.hostEntries
                    .Where(c => c.IsNew)
                    .ToList();

                this.proxy.AddEntries(entries);

                this.UpdateData();
            }
        }

        public virtual void EditSelectedEntry()
        {
            using (var form = new EditHostEntryForm(view.ServiceProvider, addressProvider))
            {
                form.Text = Resources.EditHostEntryDialogTitle;

                HostEntry entry = this.view.SelectedEntries.First();
                HostEntry originalEntry = entry.Clone();

                form.HostEntry = entry;

                DialogResult result = view.ShowDialog(form);

                if (result == DialogResult.OK)
                {
                    this.proxy.EditEntries(
                        new List<HostEntry>() { originalEntry },
                        new List<HostEntry>() { form.HostEntry }
                        );

                    this.UpdateData();
                }
            }
        }

        private void DeleteSelected()
        {
            DialogResult result = MessageBox.Show(view, Resources.DeleteEntriesConfirmation, Resources.DeleteEntriesConfirmationTitle,
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                IList<HostEntry> entries = this.view.SelectedEntries.ToList();

                this.proxy.DeleteEntries(entries);

                this.UpdateData();
            }
        }

        private void EnableSelectedEntries()
        {
            DialogResult result = MessageBox.Show(view, Resources.EnableEntriesConfirmation, Resources.EnableEntriesConfirmationTitle,
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                IList<HostEntry> originalEntries = this.view.SelectedEntries.ToList();
                IList<HostEntry> changedEntries = CloneEntries(originalEntries);

                foreach (HostEntry entry in changedEntries)
                {
                    entry.Enabled = true;
                }

                this.proxy.EditEntries(originalEntries, changedEntries);

                this.UpdateData();
            }
        }

        private void DisableSelectedEntries()
        {
            DialogResult result = MessageBox.Show(view, Resources.DisableEntriesConfirmation, Resources.DisableEntriesConfirmationTitle,
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                IList<HostEntry> originalEntries = this.view.SelectedEntries.ToList();
                IList<HostEntry> changedEntries = CloneEntries(originalEntries);

                foreach (HostEntry entry in changedEntries)
                {
                    entry.Enabled = false;
                }

                this.proxy.EditEntries(originalEntries, changedEntries);

                this.UpdateData();
            }
        }

        private IList<HostEntry> CloneEntries(IList<HostEntry> input)
        {
            List<HostEntry> output = new List<HostEntry>(input.Count);

            for (int i = 0; i < input.Count; i++)
            {
                output.Add(input[i].Clone());
            }

            return output;
        }

        private void CancelChanges()
        {
            this.UpdateData();
        }

        public void FilterEntries(string text)
        {
            this.filter = text;

            this.DisplayEntries();
        }

        private void DisplayEntries()
        {
            var entries = hostEntries;

            string filter = view.SearchFilter;

            if (!String.IsNullOrEmpty(filter))
            {
                entries = entries.Where(c =>
                    c.Address.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                    c.Hostname.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                    (c.Comment ?? "").IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) != -1
                    );
            }

            view.SetHostEntries(entries);
        }

        private class ManageHostsTaskList : TaskList
        {
            private const string ActionsCategory = "Actions";
            private const string EnableCategory = "Enable";
            private const string ChangesCategory = "Changes";

            private ResourceManager iisUIResources = new ResourceManager("Microsoft.Web.Management.Resources", typeof(ModulePage).Assembly);

            private ManageHostsModulePagePresenter owner;

            private bool appliedChanges = false;

            public ManageHostsTaskList(ManageHostsModulePagePresenter owner)
            {
                this.owner = owner;
            }

            public override ICollection GetTaskItems()
            {
                ArrayList list = new ArrayList();

                if (owner.HasChanges && appliedChanges)
                {
                    appliedChanges = false;
                }

                IList<HostEntry> selectedEntries = owner.SelectedEntries.ToList();

                bool hasSelection = (selectedEntries.Count > 0);
                bool multipleSelected = (selectedEntries.Count > 1);

                //bool selectedItem

                list.Add(CreateAddTask());

                bool hasNewEntries = owner.hostEntries.Where(c => c.IsNew).Count() > 0;
                bool showAddMissingTask = hasNewEntries;

                if (showAddMissingTask)
                {
                    list.Add(CreateAddMissingTask());
                }

                if (selectedEntries.Count > 0)
                {
                    HostEntry firstSelectedEntry = selectedEntries[0];

                    bool firstEntryEnabled = firstSelectedEntry.Enabled;
                    bool firstEntryNew = firstSelectedEntry.IsNew;
                    
                    bool showEditTask = !(multipleSelected || firstEntryNew);
                    bool showDeleteTask = !firstEntryNew;
                    bool showDisableTask = !firstEntryNew && (multipleSelected || firstEntryEnabled);
                    bool showEnableTask = !firstEntryNew && (multipleSelected || !showDisableTask);

                    if (showEditTask)
                    {
                        list.Add(CreateEditTask());
                    }

                    if (showDeleteTask)
                    {
                        list.Add(CreateDeleteTask());
                    }

                    if (showEnableTask)
                    {
                        list.Add(CreateEnableTask());
                    }

                    if (showDisableTask)
                    {
                        list.Add(CreateDisableTask());
                    }
                }

                return list;
            }

            private MethodTaskItem CreateTaskItem(string methodName, string category)
            {
                string resourcePrefix = methodName;

                Image taskImage = GetIisImageResource(resourcePrefix);

                return new MethodTaskItem(
                    methodName,
                    iisUIResources.GetString(resourcePrefix + "Task"),
                    category,
                    iisUIResources.GetString(resourcePrefix + "Description"),
                    taskImage
                    );
            }

            private Image GetIisImageResource(string key)
            {
                return (Image)iisUIResources.GetObject(key);
            }

            public void AddTask()
            {
                this.owner.AddTask();
            }

            public void AddMissingTask()
            {
                this.owner.AddMissingTask();
            }

            public void EditTask()
            {
                this.owner.EditSelectedEntry();
            }

            public void DeleteSelected()
            {
                this.owner.DeleteSelected();
            }

            public void EnableSelected()
            {
                this.owner.EnableSelectedEntries();
            }

            public void DisableSelected()
            {
                this.owner.DisableSelectedEntries();
            }

            #region Task Definitions

            private TaskItem CreateAddTask()
            {
                TaskItem addTask = new MethodTaskItem(
                                "AddTask",
                                 Resources.AddHostEntryTask,
                                 ActionsCategory,
                                 Resources.AddHostEntryDescription
                                 );
                return addTask;
            }

            private TaskItem CreateAddMissingTask()
            {
                TaskItem addTask = new MethodTaskItem(
                                "AddMissingTask",
                                 Resources.AddMissingHostEntriesTask,
                                 ActionsCategory,
                                 Resources.AddMissingHostEntriesDescriptin
                                 );
                return addTask;
            }


            private TaskItem CreateEditTask()
            {
                TaskItem editTask = new MethodTaskItem(
                                "EditTask",
                                 Resources.EditHostEntryTask,
                                 ActionsCategory,
                                 Resources.EditHostEntryDescription
                                 );
                return editTask;
            }

            private TaskItem CreateDeleteTask()
            {
                TaskItem deleteTask = new MethodTaskItem(
                                "DeleteSelected",
                                 Resources.DeleteHostEntryTask,
                                 ActionsCategory,
                                 Resources.DeleteHostEntryDescription,
                                 GetIisImageResource("DeleteImage")
                                 );
                return deleteTask;
            }

            private TaskItem CreateEnableTask()
            {
                TaskItem enableTask = new MethodTaskItem(
                                "EnableSelected",
                                 Resources.EnableHostEntriesTask,
                                 EnableCategory,
                                 Resources.EnableHostEntriesDescription
                                 );

                return enableTask;
            }

            private TaskItem CreateDisableTask()
            {
                TaskItem enableTask = new MethodTaskItem(
                                "DisableSelected",
                                 Resources.DisableHostEntriesTask,
                                 EnableCategory,
                                 Resources.DisableHostEntriesDescription
                                 );
                return enableTask;
            }

            #endregion
        }
    }
}
