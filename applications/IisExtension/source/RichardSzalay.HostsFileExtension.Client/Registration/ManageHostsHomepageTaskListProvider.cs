using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Client.Extensions;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Management.Server;
using System.Diagnostics;
using System.Timers;
using System.Collections;
using System.ComponentModel;
using Microsoft.Web.Management.Client.Win32;
using System.Net;
using RichardSzalay.HostsFileExtension.Service;
using RichardSzalay.HostsFileExtension.Client.Controller;
using RichardSzalay.HostsFileExtension.Client.View;
using RichardSzalay.HostsFileExtension.Client.Model;
using RichardSzalay.HostsFileExtension.Client.Services;
using RichardSzalay.HostsFileExtension.Client.Properties;
using System.Windows.Forms;

namespace RichardSzalay.HostsFileExtension.Client.Registration
{
    /// <summary>
    /// Provides integration with the website "homepage", allowing us to add task links on the right
    /// </summary>
    /// <remarks>
    /// Registered in ManageHostsModule
    /// </remarks>
    public class ManageHostsHomepageTaskListProvider : IHomepageTaskListProvider
    {
        private Module module;

        public ManageHostsHomepageTaskListProvider(Module module)
        {
            this.module = module;

            taskList = new TestTaskList(this);
        }

        private TaskList taskList;

        private string currentSiteName;
        private Connection connection;

        private IManageHostsControllerFactory controllerFactory;
        private IManageHostsController controller;
        private IManagementUIService uiService;
        private HierarchyService hierarchyService;
        private INavigationService navigationService;

        private bool initialized = false;
        private bool isDirty = true;
        private IEnumerable<HostEntryViewModel> hostEntries;

        private void Initialize(IServiceProvider serviceProvider)
        {
            if (!initialized)
            {
                this.serviceProvider = serviceProvider;

                connection = (Connection)serviceProvider.GetService(typeof(Connection));
                uiService = (IManagementUIService)serviceProvider.GetService(typeof(IManagementUIService));

                controllerFactory = (IManageHostsControllerFactory)serviceProvider.GetService(typeof(IManageHostsControllerFactory));

                isDirty = true;

                HierarchyService hs = (HierarchyService)serviceProvider.GetService(typeof(HierarchyService));
                INavigationService navService = (INavigationService)serviceProvider.GetService(typeof(INavigationService));

                hs.InfoRefreshed += (s, e) =>
                    {
                        isDirty = true;
                    };

                navService.NavigationPerformed += (s, e) =>
                    {
                        isDirty = true;
                    };

                initialized = true;
            }
        }

        public TaskList GetTaskList(IServiceProvider serviceProvider, ModulePageInfo selectedModulePage)
        {
            this.Initialize(serviceProvider);

            if (isDirty)
            {
                if (connection.ConfigurationPath.SiteName != currentSiteName)
                {
                    controller = controllerFactory.Create(connection, module);
                }

                var proxy = (ManageHostsFileModuleProxy)connection
                    .CreateProxy(this.module, typeof(ManageHostsFileModuleProxy));

                this.addresses = proxy.GetServerAddresses();

                hierarchyService = (HierarchyService)serviceProvider.GetService(typeof(HierarchyService));
                navigationService = (INavigationService)serviceProvider.GetService(typeof(INavigationService));

                Trace.WriteLine("ManageHostsHomepageTaskListProvider.GetTaskList (" + connection.ConfigurationPath.SiteName + ")");

                bool isSite = (connection.ConfigurationPath.PathType == ConfigurationPathType.Site);

                if (isSite)
                {
                    isDirty = false;

                    this.RefreshHostEntries();

                    this.alternateAddresses = new HostEntrySelectionOptionsStrategy(hostEntries)
                        .GetOptions(hostEntries).AlternateAddresses;

                    this.hasEnabledBindingEntries = hostEntries.Any(x => x.HostEntry.Enabled);
                }
            }

            return taskList;
        }

        private void RefreshHostEntries()
        {
            var allEntries = controller.GetHostEntryModels(connection);
            this.bindings = controller.GetSiteBindings(connection).ToArray();

            this.hostEntries = allEntries
                .Where(x => bindings.Any(b => b.Host == x.HostEntry.Hostname))
                .ToList();
        }

        private SiteBinding[] bindings;
        private string[] addresses;
        private IServiceProvider serviceProvider;
        private ICollection<string> alternateAddresses;
        private bool hasEnabledBindingEntries;


        private class TestTaskList : TaskList
        {
            private ManageHostsHomepageTaskListProvider owner;

            public TestTaskList(ManageHostsHomepageTaskListProvider owner)
            {
                this.owner = owner;
                this.Expanded = true;
            }

            public void SwitchBindingsAddress(string address)
            {
                this.owner.SwitchBindingsAddress(address);
            }

            public void SwitchBindingsAddressToManual()
            {
                this.owner.SwitchBindingsAddressToManual();
            }

            public void DisableAllBindingEntries()
            {
                this.owner.DisableAllBindingEntries();
            }

            public void GoToHostsView()
            {
                this.owner.GoToHostsView();
            }

            public override System.Collections.ICollection GetTaskItems()
            {
                ArrayList list = new ArrayList();

                if (owner.bindings != null && owner.bindings.Length > 0)
                {
                    list.Add(CreateHostChangesGroup());
                }

                return list;
            }

            private TaskItem CreateHostChangesGroup()
            {
                GroupTaskItem taskItem = new GroupTaskItem("Expanded", "Hosts File", "", true);

                if (owner.alternateAddresses != null)
                {
                    foreach (var alternateAddress in owner.alternateAddresses)
                    {
                        taskItem.Items.Add(new MethodTaskItem("SwitchBindingsAddress",
                            String.Format(Resources.SwitchBindingsAddressTask, alternateAddress),
                            "Tasks",
                            String.Format(Resources.SwitchBindingAddressDescription, alternateAddress),
                            null, alternateAddress));
                    }
                }

                taskItem.Items.Add(new MethodTaskItem("SwitchBindingsAddressToManual",
                        Resources.SwitchBindingsAddressesToManualTask,
                        "Tasks"));
                
                if (owner.hasEnabledBindingEntries)
                {
                    taskItem.Items.Add(new MethodTaskItem("DisableAllBindingEntries",
                        Resources.DisableBindingEntriesTask,
                        "Tasks", Resources.DisableBindingEntriesDescription));
                }

                taskItem.Items.Add(new MethodTaskItem("GoToHostsView", Resources.EditHostsTask, "Tasks"));

                return taskItem;
            }

            public bool Expanded { get; set; }
        }

        private void SwitchBindingsAddress(string address)
        {
            var proxy = (ManageHostsFileModuleProxy)connection
                .CreateProxy(this.module, typeof(ManageHostsFileModuleProxy));

            var hosts = bindings.Select(x => x.Host).Distinct().ToList();

            var entriesToAdd = hosts
                .Where(h => !hostEntries.Any(entry => entry.HostEntry.Hostname == h && 
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
                .Select(host => hostEntries.FirstOrDefault(m => m.HostEntry.Hostname == host && m.HostEntry.Address == address))
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

            var entriesToDisableBefore = hostEntries
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

            proxy.EditEntries(
                entriesToDisableBefore.Concat(entriesToEnableBefore).ToList(),
                entriesToDisableAfter.Concat(entriesToEnableAfter).ToList()
            );

            isDirty = true;

            uiService.Update();
        }

        private void SwitchBindingsAddressToManual()
        {
            using (var form = new SelectSwitchAddress(serviceProvider, addresses))
            {
                var result = uiService.ShowDialog(form);

                if (result == DialogResult.OK)
                {
                    SwitchBindingsAddress(form.Address);
                }
            }
        }

        private void DisableAllBindingEntries()
        {
            isDirty = true;

            var enabledEntries = this.hostEntries
                .Select(m => m.HostEntry)
                .Where(x => x.Enabled && bindings.Any(b => b.Host == x.Hostname))
                .ToList();

            var proxy = (ManageHostsFileModuleProxy)connection
                .CreateProxy(this.module, typeof(ManageHostsFileModuleProxy));

            if (enabledEntries.Count > 0)
            {
                var disabledEntries = enabledEntries
                    .Select(entry =>
                        {
                            var newEntry = entry.Clone();
                            newEntry.Enabled = false;

                            return newEntry;
                        })
                    .ToList();

                proxy.EditEntries(enabledEntries, disabledEntries);
            }

            uiService.Update();
        }

        private void GoToHostsView()
        {
            navigationService.Navigate(connection, connection.ConfigurationPath,
                typeof(ManageHostsModulePage), null);            
        }
    }
}
