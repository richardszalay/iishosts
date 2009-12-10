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
using RichardSzalay.HostsFileExtension.Controller;

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

        private string currentSiteName;
        private Connection connection;

        private IManageHostsControllerFactory controllerFactory;
        private IManageHostsController controller;
        private IManagementUIService uiService;
        private HierarchyService hierarchyService;
        private INavigationService navigationService;

        private IEnumerable<HostEntryViewModel> hostEntryModels;

        private bool canFixEntries = false;
        
        private bool initialized = false;
        private bool isDirty = true;

        private void Initialize(IServiceProvider serviceProvider)
        {
            if (!initialized)
            {
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

                hierarchyService = (HierarchyService)serviceProvider.GetService(typeof(HierarchyService));
                navigationService = (INavigationService)serviceProvider.GetService(typeof(INavigationService));

                Trace.WriteLine("ManageHostsHomepageTaskListProvider.GetTaskList (" + connection.ConfigurationPath.SiteName + ")");

                bool isSite = (connection.ConfigurationPath.PathType == ConfigurationPathType.Site);

                bool cancelled = false;

                HierarchyInfoEventHandler infoRefreshedHandler = null;
                NavigationEventHandler navigationHandler = null;

                Action cancelAsync = () =>
                    {
                        Trace.WriteLine("Cancelling");

                        cancelled = true;
                        hierarchyService.InfoUpdated -= infoRefreshedHandler;
                        navigationService.NavigationPerformed -= navigationHandler;
                    };

                infoRefreshedHandler = (s, e) => cancelAsync();
                navigationHandler = (s, e) => cancelAsync();

                hierarchyService.InfoUpdated += infoRefreshedHandler;
                navigationService.NavigationPerformed += navigationHandler;

                if (isSite)
                {
                    isDirty = false;

                    controller.ResolveBindings(connection, () =>
                        {
                            if (isDirty || cancelled)
                            {
                                Trace.WriteLine("Cancelled");

                                return;
                            }

                            hierarchyService.InfoUpdated -= infoRefreshedHandler;
                            navigationService.NavigationPerformed -= navigationHandler;

                            Trace.WriteLine("ManageHostsHomepageTaskListProvider.GetTaskList => ResolveBindings complete");

                            hostEntryModels = controller.GetHostEntryModels(connection, true);

                            bool newCanFixEntriesValue = hostEntryModels.Any(m => m.Conflicted || m.HostEntry.IsNew);

                            if (newCanFixEntriesValue != canFixEntries)
                            {
                                Trace.WriteLine("ManageHostsHomepageTaskListProvider.GetTaskList => Updating UI");

                                canFixEntries = newCanFixEntriesValue;

                                ISynchronizeInvoke syncInvoke = (ISynchronizeInvoke)navigationService.CurrentItem.Page;

                                syncInvoke.Invoke(new Action(() => uiService.Update()), null);
                            }
                        });
                }
            }

            return taskList;
        }

        private void FixEntries()
        {
            this.controller.FixEntries(hostEntryModels);

            isDirty = true;

            uiService.Update();
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
