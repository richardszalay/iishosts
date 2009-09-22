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
using RichardSzalay.HostsFileExtension.Properties;
using RichardSzalay.HostsFileExtension.Model;

namespace RichardSzalay.HostsFileExtension.Registration
{
    class ManageHostsHomepageTaskListProvider : IHomepageTaskListProvider
    {
        private Module module;

        public ManageHostsHomepageTaskListProvider(Module module)
        {
            this.module = module;

            taskList = new TestTaskList(this);
        }

        private TaskList taskList;

        private Connection connection;
        private INavigationService navService;
        private IManagementUIService uiService;

        private ManageHostsFileModuleProxy hostsFileProxy;

        private IEnumerable<HostEntryViewModel> hostEntryModels;

        private bool canFixEntries = false;

        private bool updating = false;

        public TaskList GetTaskList(IServiceProvider serviceProvider, ModulePageInfo selectedModulePage)
        {
            connection = (Connection)serviceProvider.GetService(typeof(Connection));
            navService = (INavigationService)serviceProvider.GetService(typeof(INavigationService));
            uiService = (IManagementUIService)serviceProvider.GetService(typeof(IManagementUIService));
            
            IBindingInfoProvider bindingInfoProvider = (IBindingInfoProvider)serviceProvider.GetService(typeof(IBindingInfoProvider));
            IBulkHostResolver bulkHostResolver = (IBulkHostResolver)serviceProvider.GetService(typeof(IBulkHostResolver));
            IAddressProvider addressProvider = (IAddressProvider)serviceProvider.GetService(typeof(IAddressProvider));

            if (IsSiteConnection(connection) && !canFixEntries && !updating)
            {
                var bindings = bindingInfoProvider.GetBindings(connection);

                IAsyncResult result = null;

                AsyncCallback callback = new AsyncCallback(r =>
                    {
                        IPHostEntry[] localEntries = bulkHostResolver.EndGetHostEntries(result);

                        SiteHostEntryViewModelStrategy strategy = new SiteHostEntryViewModelStrategy(
                            bindings.Select(b => new SiteBinding(b)).ToArray(),
                            addressProvider.GetAddresses()
                            );

                        hostsFileProxy = (ManageHostsFileModuleProxy)connection.CreateProxy(module, typeof(ManageHostsFileModuleProxy));

                        var hostEntries = hostsFileProxy
                            .GetEntries()
                            .Where(e => bindings.Any(b => b.Host == e.Hostname));

                        hostEntryModels = strategy.GetEntryModels(hostEntries, localEntries);

                        canFixEntries = hostEntryModels.Any(m => m.Conflicted || m.HostEntry.IsNew);

                        updating = false;

                        uiService.Update();
                    });

                ISynchronizeInvoke syncInvoke = (ISynchronizeInvoke)navService.CurrentItem.Page;

                updating = true;

                result = bulkHostResolver.BeginGetHostEntries(
                    bindings.Select(b => b.Host),
                    syncInvoke,
                    TimeSpan.FromMilliseconds(1000),
                    callback,
                    null
                    );
            }

            return taskList;
        }

        private void FixEntries()
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

            this.hostsFileProxy.AddEntries(newEntries);

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

            this.hostsFileProxy.EditEntries(originalConflictedModels, conflictedEntries);

            uiService.Update();
        }

        private static bool IsSiteConnection(Connection connection)
        {
            return (connection.ConfigurationPath.PathType == ConfigurationPathType.Site);
        }

        private class TestTaskList : TaskList
        {
            private ManageHostsHomepageTaskListProvider owner;

            public TestTaskList(ManageHostsHomepageTaskListProvider owner)
            {
                this.owner = owner;
            }

            public void FixEntries()
            {
                this.owner.FixEntries();
            }

            public override System.Collections.ICollection GetTaskItems()
            {
                ArrayList list = new ArrayList();

                if (owner.canFixEntries)
                {
                    list.Add(CreateFixEntriesTaskItem());

                    // HACK
                    owner.canFixEntries = false;
                }

                return list;
            }

            private TaskItem CreateFixEntriesTaskItem()
            {
                return new MessageTaskItem(
                    MessageTaskItemType.Warning,
                    Resources.FixEntriesDescription,
                    "",
                    Resources.FixEntriesDescription,
                    "FixEntries",
                    null
                );
            }
        }
    }
}
