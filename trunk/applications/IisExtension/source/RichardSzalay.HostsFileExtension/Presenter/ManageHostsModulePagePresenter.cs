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
using RichardSzalay.HostsFileExtension.Model;
using RichardSzalay.HostsFileExtension.Extensions;
using System.Net;
using RichardSzalay.HostsFileExtension.Controller;
using System.Diagnostics;

namespace RichardSzalay.HostsFileExtension.Presenter
{
    public class ManageHostsModulePagePresenter
    {
        private IManageHostsModulePage view;

        private IEnumerable<HostEntryViewModel> hostEntryModels;
        
        private bool hasAlreadyResolvedEntries = false;

        private ManageHostsFileModuleProxy proxy;

        private IManageHostsControllerFactory controllerFactory;

        private IAddressProvider addressProvider;
        private IBindingInfoProvider bindingInfoProvider;
        private IBulkHostResolver bulkHostResolver;

        private string filter;

        public ManageHostsModulePagePresenter(IManageHostsModulePage view)
        {
            this.view = view;
            this.view.Initialized += new EventHandler(view_Initialized);
            this.view.Refreshing += new EventHandler(view_Refreshing);
            this.view.ListItemDoubleClick += new EventHandler(view_ListItemDoubleClick);
            this.view.SearchFilterChanged += new EventHandler(view_SearchFilterChanged);
            this.view.DeleteSelected += new EventHandler(view_DeleteSelected);
        }

        void view_DeleteSelected(object sender, EventArgs e)
        {
            this.DeleteSelected();
        }

        void view_SearchFilterChanged(object sender, EventArgs e)
        {
            this.DisplayEntries();
        }

        void view_ListItemDoubleClick(object sender, EventArgs e)
        {
            int hostEntryCount = this.view.SelectedEntries.Count();

            if (hostEntryCount == 1)
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
            this.bulkHostResolver = (IBulkHostResolver)view.GetService(typeof(IBulkHostResolver));
            this.addressProvider = (IAddressProvider)view.GetService(typeof(IAddressProvider));
            this.bindingInfoProvider = (IBindingInfoProvider)view.GetService(typeof(IBindingInfoProvider));

            this.controllerFactory = (IManageHostsControllerFactory)view.GetService(typeof(IManageHostsControllerFactory));

            this.proxy = view.CreateProxy<ManageHostsFileModuleProxy>();

            view.SetTaskList(new ManageHostsTaskList(this));

            this.UpdateData();
        }

        private bool IsSiteView
        {
            get
            {
                return view.ConfigurationPathType == ConfigurationPathType.Site;
            }
        }

        public bool HasChanges
        {
            get
            {
                return false;
            }
        }

        public bool CanFindMissingEntries
        {
            get { return !hasAlreadyResolvedEntries; }
        }

        public bool HasConflictedEntries
        {
            get { return hostEntryModels.Where(c => c.Conflicted).Count() > 0; }
        }

        private void UpdateData()
        {
            Trace.WriteLine("ManageHostsModulePagePresenter.UpdateData");

            Connection connection = view.Connection;

            IManageHostsController controller = controllerFactory.Create(connection, this.view.Module);

            bool displayDefaultEntries = true;

            if (IsSiteView)
            {
                controller.ResolveBindings(connection, () =>
                {
                    displayDefaultEntries = false;

                    Trace.WriteLine("ManageHostsModulePagePresenter.UpdateData => ResolveBindings complete");

                    hostEntryModels = controller.GetHostEntryModels(connection, true);

                    DisplayEntries();
                });
            }

            if (displayDefaultEntries)
            {
                hostEntryModels = controller.GetHostEntryModels(connection, false);

                DisplayEntries();
            }
        }

        private IEnumerable<HostEntryViewModel> SelectedEntries
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

                HostEntry selectedEntry = view.SelectedEntries.Select(c => c.HostEntry).FirstOrDefault();

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

        private void FixEntriesTask()
        {
            IEnumerable<HostEntryViewModel> newModels = this.hostEntryModels
                    .Where(model => model.HostEntry.IsNew);

            foreach (HostEntryViewModel newModel in newModels)
            {
                if (newModel.Conflicted)
                {
                    newModel.HostEntry.Address = newModel.PreferredAddress;
                }
            }

            List<HostEntry> newEntries = newModels.Select(m => m.HostEntry).ToList();

            this.proxy.AddEntries(newEntries);

            IEnumerable<HostEntryViewModel> conflictedModels = this.hostEntryModels
                .Where(model => model.Conflicted && !model.HostEntry.IsNew);

            List<HostEntry> originalConflictedModels = conflictedModels
                .Select(m => m.HostEntry.Clone())
                .ToList();

            foreach (HostEntryViewModel conflictedModel in conflictedModels)
            {
                conflictedModel.HostEntry.Address = conflictedModel.PreferredAddress;
            }

            List<HostEntry> conflictedEntries = conflictedModels.Select(m => m.HostEntry).ToList();

            this.proxy.EditEntries(originalConflictedModels, conflictedEntries);

            this.UpdateData();
        }

        public virtual void EditSelectedEntry()
        {
            using (var form = new EditHostEntryForm(view.ServiceProvider, addressProvider))
            {
                form.Text = Resources.EditHostEntryDialogTitle;

                HostEntry entry = this.view.SelectedEntries.First().HostEntry;
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
                IList<HostEntry> entries = this.view.SelectedEntries.Select(e => e.HostEntry).ToList();

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
                IList<HostEntry> originalEntries = this.view.SelectedEntries.Select(e => e.HostEntry).ToList();
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
                IList<HostEntry> originalEntries = this.view.SelectedEntries.Select(c => c.HostEntry).ToList();
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
            Trace.WriteLine("ManageHostsModulePagePresenter.DisplayEntries");

            var entries = hostEntryModels;

            string filter = view.SearchFilter;

            if (!String.IsNullOrEmpty(filter))
            {
                entries = entries.Where(model =>
                    model.HostEntry.Address.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                    model.HostEntry.Hostname.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                    (model.HostEntry.Comment ?? "").IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) != -1
                    );
            }

            view.Invoke(new Action(() =>
                {
                    view.SetHostEntries(entries);
                }), null);
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

                IList<HostEntry> selectedEntries = owner.SelectedEntries.Select(c => c.HostEntry).ToList();

                bool hasSelection = (selectedEntries.Count > 0);
                bool multipleSelected = (selectedEntries.Count > 1);

                //bool selectedItem

                list.Add(CreateAddTask());

                bool hasFixableEntries = owner.hostEntryModels
                    .Any(c => c.Conflicted || c.HostEntry.IsNew);

                bool showFixEntriesTask = hasFixableEntries;

                if (showFixEntriesTask)
                {
                    list.Add(CreateFixEntriesTask());
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

            public void FixEntriesTask()
            {
                this.owner.FixEntriesTask();
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

            private TaskItem CreateFixEntriesTask()
            {
                TaskItem addTask = new MessageTaskItem(
                                MessageTaskItemType.Warning,
                                 Resources.FixEntriesDescription,
                                 ActionsCategory,
                                 Resources.FixEntriesDescription,
                                 "FixEntriesTask",
                                 null
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
