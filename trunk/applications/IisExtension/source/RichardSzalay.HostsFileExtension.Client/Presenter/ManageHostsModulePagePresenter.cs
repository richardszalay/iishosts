using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RichardSzalay.HostsFileExtension.Client.View;
using System.Drawing;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Management.Client.Win32;
using RichardSzalay.HostsFileExtension.Client.Properties;
using System.Collections;
using System.Resources;
using RichardSzalay.HostsFileExtension.Service;
using Microsoft.Web.Management.Server;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;
using RichardSzalay.HostsFileExtension.Client.Controller;
using RichardSzalay.HostsFileExtension.Client.Model;
using RichardSzalay.HostsFileExtension.Client.Services;
using RichardSzalay.HostsFileExtension.Messages;

namespace RichardSzalay.HostsFileExtension.Presenter
{
    public class ManageHostsModulePagePresenter
    {
        private IManageHostsModulePage view;

        private IEnumerable<HostEntryViewModel> hostEntryModels;
        
        private bool hasAlreadyResolvedEntries = false;

        private ManageHostsFileModuleProxy proxy;

        private IManageHostsControllerFactory controllerFactory;

        private IHostEntrySelectionOptionsStrategy selectionOptionsStrategy;

        private string filter;

        private Connection connection;
        private HostEntrySelectionOptions options;
        private IManagementUIService uiService;
        private TipSelector tipSelector;
        private string currentTip;
        private string[] addresses;
        private IServiceProvider serviceProvider;

        public ManageHostsModulePagePresenter(IManageHostsModulePage view)
        {
            this.view = view;
            this.view.Initialized += new EventHandler(view_Initialized);
            this.view.Refreshing += new EventHandler(view_Refreshing);
            this.view.ListItemDoubleClick += new EventHandler(view_ListItemDoubleClick);
            this.view.SearchFilterChanged += new EventHandler(view_SearchFilterChanged);
            this.view.DeleteSelected += new EventHandler(view_DeleteSelected);
            this.view.EditSelected += new EventHandler(view_EditSelected);
            this.view.SelectionChanged += new EventHandler(view_SelectionChanged);
        }

        void view_EditSelected(object sender, EventArgs e)
        {
            this.EditSelectedEntry();
        }

        void view_SelectionChanged(object sender, EventArgs e)
        {
            this.options = selectionOptionsStrategy.GetOptions(this.SelectedEntries);
            uiService.Update();
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
            this.controllerFactory = (IManageHostsControllerFactory)view.GetService(typeof(IManageHostsControllerFactory));

            this.connection = (Connection)view.GetService(typeof(Connection));

            this.serviceProvider = (IServiceProvider)connection;

            this.proxy = view.CreateProxy<ManageHostsFileModuleProxy>();

            this.uiService = (IManagementUIService)view.GetService(typeof(IManagementUIService));

            view.SetTaskList(new ManageHostsTaskList(this));

            this.tipSelector = new TipSelector();
            this.RefreshTip();

            this.UpdateData();
        }

        private bool IsSiteView
        {
            get
            {
                return view.ConfigurationPathType == ConfigurationPathType.Site;
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

            hostEntryModels = controller.GetHostEntryModels(connection);

            this.selectionOptionsStrategy = new HostEntrySelectionOptionsStrategy(hostEntryModels);

            this.addresses = proxy.GetServerAddresses();

            this.options = selectionOptionsStrategy.GetOptions(this.SelectedEntries);

            DisplayEntries();
        }

        private void RefreshTip()
        {
            this.currentTip = tipSelector.SelectTip();
            this.uiService.Update();
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
            string defaultAddress = addresses[0];

            using (var form = new EditHostEntryForm(view.ServiceProvider, addresses))
            {
                form.Text = Resources.AddHostEntryDialogTitle;

                HostEntry selectedEntry = view.SelectedEntries.Select(c => c.HostEntry).FirstOrDefault();

                form.EditableFields = HostEntryField.All;
                form.HostEntry = new HostEntry("", defaultAddress, null);

                DialogResult result = view.ShowDialog(form);

                if (result == DialogResult.OK)
                {
                    this.proxy.AddEntries(new List<HostEntry>() { form.HostEntry });
                    this.UpdateData();
                }
            }
        }

        public virtual void EditSelectedEntry()
        {
            using (var form = new EditHostEntryForm(view.ServiceProvider, addresses))
            {
                form.Text = Resources.EditHostEntryDialogTitle;

                HostEntry templateEntry = this.view.SelectedEntries.First().HostEntry;
                HostEntry editingEntry = templateEntry.Clone();

                form.EditableFields = options.EditableFields;
                form.HostEntry = editingEntry;

                DialogResult result = view.ShowDialog(form);

                if (result == DialogResult.OK)
                {
                    var originalEntries = SelectedEntries.Select(m => m.HostEntry).ToList();
                    var editedEntries = SelectedEntries.Select(entry =>
                        {
                            var newEntry = entry.HostEntry.Clone();
                            this.ApplyChanges(newEntry, editingEntry, form.FieldChanges);

                            return newEntry;
                        })
                        .ToList();

                    try
                    {
                        this.proxy.EditEntries(originalEntries, editedEntries);
                    }
                    catch (HostsFileServiceException ex)
                    {
                        uiService.ShowError(ex, null, Resources.ActionFailedTitle, false);
                        return;
                    }

                    this.UpdateData();
                }
            }
        }

        private void ApplyChanges(HostEntry applyTo, HostEntry applyFrom, HostEntryField fields)
        {
            if ((fields & HostEntryField.Address) != 0)
            {
                applyTo.Address = applyFrom.Address;
            }

            if ((fields & HostEntryField.Hostname) != 0)
            {
                applyTo.Hostname = applyFrom.Hostname;
            }

            if ((fields & HostEntryField.Enabled) != 0)
            {
                applyTo.Enabled = applyFrom.Enabled;
            }

            if ((fields & HostEntryField.Comment) != 0)
            {
                applyTo.Comment = applyFrom.Comment;
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

        private void SwitchAddress(string address)
        {
            var hosts = SelectedEntries.Select(x => x.HostEntry.Hostname).Distinct();

            var entriesToAdd = hosts
                .Where(h => !hostEntryModels.Any(entry => entry.HostEntry.Hostname == h &&
                            entry.HostEntry.Address == address))
                .Select(host =>
                {
                    return new HostEntry(host, address, null);
                })
                .ToList();

            if (entriesToAdd.Count > 0)
            {
                proxy.AddEntries(entriesToAdd);
            }

            var entriesToEnableBefore = hosts
                .Select(host => hostEntryModels.FirstOrDefault(m => m.HostEntry.Hostname == host && m.HostEntry.Address == address))
                .Where(m => m != null)
                .Select(m => m.HostEntry)
                .ToList();

            var entriesToEnableAfter = entriesToEnableBefore
                .Select(e =>
                    {
                        var newEntry = e.Clone();
                        newEntry.Enabled = true;
                        return newEntry;
                    }).ToList();

            var entriesToDisableBefore = hostEntryModels
                .Where(m => hosts.Any(h => h == m.HostEntry.Hostname) &&
                            !entriesToEnableBefore.Contains(m.HostEntry))
                .Select(m => m.HostEntry)
                .ToList();

            var entriesToDisableAfter = entriesToDisableBefore
                .Select(e =>
                {
                    var newEntry = e.Clone();
                    newEntry.Enabled = false;
                    return newEntry;
                }).ToList();

            this.proxy.EditEntries(
                entriesToDisableBefore.Concat(entriesToEnableBefore).ToList(),
                entriesToDisableAfter.Concat(entriesToEnableAfter).ToList()
            );

            this.UpdateData();
        }

        private void SwitchAddressManual()
        {
            using (var form = new SelectSwitchAddress(serviceProvider, addresses))
            {
                var result = uiService.ShowDialog(form);

                if (result == DialogResult.OK)
                {
                    SwitchAddress(form.Address);
                }
            }
        }

        private void OpenInNotepad()
        {
            string notepadPath = 
                Environment.ExpandEnvironmentVariables(@"%systemroot%\notepad.exe");

            string hostsPath = 
                Environment.ExpandEnvironmentVariables(@"%systemroot%\system32\drivers\etc\hosts");

            var psi = new ProcessStartInfo(notepadPath, hostsPath);

            Process.Start(psi);
        }

        private HostEntry GetEnabledAlternateHostEntry(HostEntry entry)
        {
            return hostEntryModels
                .Where(x => x.HostEntry.Enabled &&
                            x.HostEntry.Hostname == entry.Hostname)
                .Select(x => x.HostEntry)
                .FirstOrDefault() ?? entry;
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

            public bool ShowingSwitchAddresses { get; set; }

            private ResourceManager iisUIResources = new ResourceManager("Microsoft.Web.Management.Resources", typeof(ModulePage).Assembly);

            private ManageHostsModulePagePresenter owner;

            public ManageHostsTaskList(ManageHostsModulePagePresenter owner)
            {
                this.owner = owner;
                this.ShowingSwitchAddresses = true;
            }

            public override ICollection GetTaskItems()
            {
                ArrayList list = new ArrayList();

                list.Add(CreateAddTask());

                var options = owner.options;

                if (options.CanEdit)
                {
                    list.Add(CreateEditTask());
                }

                if (options.CanDelete)
                {
                    list.Add(CreateDeleteTask());
                }

                if (options.CanEnable)
                {
                    list.Add(CreateEnableTask());
                }

                if (options.CanDisable)
                {
                    list.Add(CreateDisableTask());
                }

                if (options.CanSwitchAddress)
                {
                    list.Add(CreateSwitchAddressTask(options.AlternateAddresses));
                }

                bool showOpenInNotepadTask = owner.connection.IsLocalConnection;

                if (showOpenInNotepadTask)
                {
                    list.Add(CreateOpenInNotepadTask());
                }

                if (owner.HasConflictedEntries)
                {
                    list.Add(CreateConflictedEntriesTask());
                }
                else
                {
                    list.Add(CreateTipTask());
                }

                return list;
            }

            private Image GetIisImageResource(string key)
            {
                return (Image)iisUIResources.GetObject(key);
            }

            #region Task proxy methods

            public void AddTask()
            {
                this.owner.AddTask();
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

            public void SwitchAddress(string address)
            {
                this.owner.SwitchAddress(address);
            }

            public void SwitchAddressManual()
            {
                this.owner.SwitchAddressManual();
            }

            public void OpenInNotepad()
            {
                this.owner.OpenInNotepad();
            }

            #endregion


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

            private TaskItem CreateConflictedEntriesTask()
            {
                TaskItem addTask = new MessageTaskItem(
                                MessageTaskItemType.Warning,
                                 Resources.ConflictedEntriesDescription,
                                 ActionsCategory,
                                 Resources.ConflictedEntriesDescription);
                return addTask;
            }

            private TaskItem CreateTipTask()
            {
                return new MessageTaskItem(
                    MessageTaskItemType.Information,
                    owner.currentTip,
                    ActionsCategory,
                    owner.currentTip);
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

            private TaskItem CreateSwitchAddressTask(ICollection<string> otherAddresses)
            {
                GroupTaskItem group = new GroupTaskItem("ShowingSwitchAddresses",
                    Resources.SwitchAddressTask, ChangesCategory, true);

                foreach (string otherAddress in otherAddresses)
                {
                    var task = new MethodTaskItem(
                        "SwitchAddress",
                        otherAddress,
                        ChangesCategory,
                        String.Format(Resources.SwitchAddressDescription, otherAddress),
                        null,
                        otherAddress
                        );

                    group.Items.Add(task);
                }

                group.Items.Add(new MethodTaskItem(
                    "SwitchAddressManual",
                    Resources.SwitchBindingsAddressesToManualTask,
                    ChangesCategory));

                return group;
            }

            private TaskItem CreateOpenInNotepadTask()
            {
                TaskItem task = new MethodTaskItem(
                            "OpenInNotepad",
                             Resources.OpenInNotepadTask,
                             ActionsCategory,
                             Resources.OpenInNotepadDescription
                             );

                return task;
            }

            #endregion
        }
    }
}
