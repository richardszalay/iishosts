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

            view.SetHostEntries(hostsFile.Entries);
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
            using (var form = new EditHostEntryForm(view.ServiceProvider))
            {
                form.HostEntry = new HostEntry("", "127.0.0.1", null);

                DialogResult result = view.ShowDialog(form);

                if (result == DialogResult.OK)
                {
                    this.hostsFile.AddEntry(form.HostEntry);

                    view.SetHostEntries(hostsFile.Entries);
                }
            }
        }

        private void EditTask()
        {
            using (var form = new EditHostEntryForm(view.ServiceProvider))
            {
                form.HostEntry = this.view.SelectedEntries.First();

                DialogResult result = view.ShowDialog(form);

                if (result == DialogResult.OK)
                {
                    view.SetHostEntries(hostsFile.Entries);
                }
            }
        }

        private void DeleteSelected()
        {
            foreach (HostEntry entry in this.view.SelectedEntries.ToList())
            {
                hostsFile.DeleteEntry(entry);
            }

            view.SetHostEntries(hostsFile.Entries);
        }

        private void EnableSelected()
        {
            foreach (HostEntry entry in this.view.SelectedEntries)
            {
                entry.Enabled = true;
            }

            view.SetHostEntries(hostsFile.Entries);
        }

        private void DisableSelected()
        {
            foreach (HostEntry entry in this.view.SelectedEntries)
            {
                entry.Enabled = false;
            }

            view.SetHostEntries(hostsFile.Entries);
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

        private class ManageHostsTaskList : TaskList
        {
            private const string ActionsCategory = "Actions";
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

                    bool showDisableTask = firstEntryEnabled;
                    bool showEnableTask = !showDisableTask;

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

                list.Add(CreateApplyChangesTask());
                list.Add(CreateCancelChangesTask());

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
                this.owner.EditTask();
            }

            public void DeleteSelected()
            {
                this.owner.DeleteSelected();
            }

            public void EnableSelected()
            {
                this.owner.EnableSelected();
            }

            public void DisableSelected()
            {
                this.owner.DisableSelected();
            }

            public void ApplyChanges()
            {
                this.owner.ApplyChanges();
            }

            public void CancelChanges()
            {
                this.owner.CancelChanges();
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
                                 ActionsCategory,
                                 Resources.EnableHostEntriesDescription
                                 );

                return enableTask;
            }

            private TaskItem CreateDisableTask()
            {
                TaskItem enableTask = new MethodTaskItem(
                                "DisableSelected",
                                 Resources.DisableHostEntriesTask,
                                 ActionsCategory,
                                 Resources.DisableHostEntriesDescription
                                 );
                return enableTask;
            }

            private TaskItem CreateApplyChangesTask()
            {
                TaskItem applyChangesTask = CreateTaskItem("ApplyChanges", ChangesCategory);
                applyChangesTask.Enabled = ((this.owner.HasChanges && this.owner.CanApplyChanges));

                return applyChangesTask;
            }

            private TaskItem CreateCancelChangesTask()
            {
                TaskItem cancelChangesTask = CreateTaskItem("CancelChanges", ChangesCategory);
                cancelChangesTask.Enabled = (this.owner.HasChanges);

                return cancelChangesTask;
            }

            #endregion
        }
    }
}
