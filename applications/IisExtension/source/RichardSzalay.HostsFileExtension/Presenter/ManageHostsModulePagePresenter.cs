using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RichardSzalay.HostsFileExtension.View;
using System.Drawing;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Management.Client.Win32;
using RichardSzalay.HostsFileExtension.Properties;
using System.Collections;
using System.Resources;

namespace RichardSzalay.HostsFileExtension.Presenter
{
    public class ManageHostsModulePagePresenter
    {
        private IManageHostsModulePage view;
        private HostsFile hostsFile;

        private string filter;

        public ManageHostsModulePagePresenter(IManageHostsModulePage view)
        {
            this.view = view;
            this.view.Initialized += new EventHandler(view_HandleCreated);
            this.view.Refreshing += new EventHandler(view_Refreshing);
        }

        void view_Refreshing(object sender, EventArgs e)
        {
            this.UpdateData();
        }

        void view_HandleCreated(object sender, EventArgs e)
        {
            view.SetTaskList(new ManageHostsTaskList(this));

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

            DisplayEntries();
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
            using (var form = new EditHostEntryForm(view.ServiceProvider, new DnsAddressProvider()))
            {
                form.Text = Resources.AddHostEntryDialogTitle;

                // TODO: Fix this default value
                form.HostEntry = new HostEntry("", "127.0.0.1", null);

                DialogResult result = view.ShowDialog(form);

                if (result == DialogResult.OK)
                {
                    this.hostsFile.AddEntry(form.HostEntry);

                    this.ApplyChanges();
                }
            }
        }

        public void EditSelectedEntry()
        {
            using (var form = new EditHostEntryForm(view.ServiceProvider, new DnsAddressProvider()))
            {
                form.Text = Resources.EditHostEntryDialogTitle;

                form.HostEntry = this.view.SelectedEntries.First();

                DialogResult result = view.ShowDialog(form);

                if (result == DialogResult.OK)
                {
                    this.ApplyChanges();
                }
            }
        }

        private void DeleteSelected()
        {
            DialogResult result = MessageBox.Show(view, Resources.DeleteEntriesConfirmation, Resources.DeleteEntriesConfirmationTitle,
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                foreach (HostEntry entry in this.view.SelectedEntries.ToList())
                {
                    hostsFile.DeleteEntry(entry);
                }

                this.ApplyChanges();
            }
        }

        private void EnableSelectedEntries()
        {
            DialogResult result = MessageBox.Show(view, Resources.EnableEntriesConfirmation, Resources.EnableEntriesConfirmationTitle,
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                foreach (HostEntry entry in this.view.SelectedEntries)
                {
                    entry.Enabled = true;
                }

                this.ApplyChanges();
            }
        }

        private void DisableSelectedEntries()
        {
            DialogResult result = MessageBox.Show(view, Resources.DisableEntriesConfirmation, Resources.DisableEntriesConfirmationTitle,
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                foreach (HostEntry entry in this.view.SelectedEntries)
                {
                    entry.Enabled = false;
                }

                this.ApplyChanges();
            }
        }

        private void ApplyChanges()
        {
            hostsFile.Save();

            this.UpdateData();
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
            var entries = this.hostsFile.Entries;

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

                list.Add(CreateAddTask());

                if (selectedEntries.Count > 0)
                {
                    HostEntry firstSelectedEntry = selectedEntries[0];

                    bool firstEntryEnabled = firstSelectedEntry.Enabled;

                    bool showDisableTask = multipleSelected || firstEntryEnabled;
                    bool showEnableTask = multipleSelected || !showDisableTask;

                    if (!multipleSelected)
                    {
                        list.Add(CreateEditTask());
                    }

                    list.Add(CreateDeleteTask());

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
