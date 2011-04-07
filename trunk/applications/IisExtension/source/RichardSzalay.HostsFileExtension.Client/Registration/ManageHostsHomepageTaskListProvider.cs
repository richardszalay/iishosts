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

        private IEnumerable<HostEntryViewModel> hostEntryModels;

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

                addresses = ((IAddressProvider)serviceProvider.GetService(typeof(IAddressProvider))).GetAddresses();

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

                hierarchyService = (HierarchyService)serviceProvider.GetService(typeof(HierarchyService));
                navigationService = (INavigationService)serviceProvider.GetService(typeof(INavigationService));

                Trace.WriteLine("ManageHostsHomepageTaskListProvider.GetTaskList (" + connection.ConfigurationPath.SiteName + ")");

                bool isSite = (connection.ConfigurationPath.PathType == ConfigurationPathType.Site);

                if (isSite)
                {
                    isDirty = false;

                    this.hostEntries = controller.GetHostEntryModels(connection);
                    this.bindings = controller.GetSiteBindings(connection).ToArray();
                }
            }

            return taskList;
        }

        private SiteBinding[] bindings;
        private string[] addresses;
        private IServiceProvider serviceProvider;


        private class TestTaskList : TaskList
        {
            private ManageHostsHomepageTaskListProvider owner;

            public TestTaskList(ManageHostsHomepageTaskListProvider owner)
            {
                this.owner = owner;
                this.Expanded = true;
            }

            public void AddToHostsFile(string hostname)
            {
                this.owner.AddToHostsFile(hostname);
            }

            public void RemoveFromHostsFile(string hostname)
            {
                this.owner.RemoveFromHostsFile(hostname);
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

                foreach (var binding in owner.bindings)
                {
                    var isAlreadyInHostFile = owner.hostEntries.Any(x => x.HostEntry.Enabled &&
                        String.Equals(x.HostEntry.Hostname, binding.Host, StringComparison.InvariantCultureIgnoreCase));

                    if (isAlreadyInHostFile)
                    {
                        taskItem.Items.Add(new MethodTaskItem("RemoveFromHostsFile", "Remove " + binding.Host,
                            "Tasks", "", null, binding.Host)); 
                    }
                    else
                    {
                        taskItem.Items.Add(new MethodTaskItem("AddToHostsFile", "Add " + binding.Host,
                            "Tasks", "", null, binding.Host));
                    }
                }

                taskItem.Items.Add(new MethodTaskItem("GoToHostsView", "Edit Hosts", "Tasks"));

                return taskItem;
            }

            public bool Expanded { get; set; }
        }

        private void AddToHostsFile(string hostname)
        {
            var existingEntry = this.hostEntries
                .FirstOrDefault(x => x.HostEntry.Hostname == hostname);

            var binding = this.bindings.FirstOrDefault(x => x.Host == hostname);

            string address = addresses[0];

            if (binding != null && !binding.IsAnyAddress)
            {
                address = binding.Address;
            }

            var proxy = (ManageHostsFileModuleProxy)connection
                .CreateProxy(this.module, typeof(ManageHostsFileModuleProxy));

            if (existingEntry != null)
            {
                if (!existingEntry.HostEntry.Enabled)
                {
                    var newEntry = existingEntry.HostEntry.Clone();
                    newEntry.Enabled = true;
                    newEntry.Address = address;

                    proxy.EditEntries(
                        new [] { existingEntry.HostEntry }, 
                        new [] { newEntry });
                }
            }
            else
            {
                var newEntry = new HostEntry(hostname, addresses.First(), null);

                proxy.AddEntries(new[] { newEntry });
            }

            isDirty = true;

            uiService.Update();
        }

        private void RemoveFromHostsFile(string hostname)
        {
            isDirty = true;

            var existingEntry = this.hostEntries
                .FirstOrDefault(x => x.HostEntry.Hostname == hostname && x.HostEntry.Enabled);

            var proxy = (ManageHostsFileModuleProxy)connection
                .CreateProxy(this.module, typeof(ManageHostsFileModuleProxy));

            if (existingEntry != null)
            {
                var newEntry = existingEntry.HostEntry.Clone();
                newEntry.Enabled = false;
                proxy.EditEntries(new [] { existingEntry.HostEntry }, new [] { newEntry });
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
