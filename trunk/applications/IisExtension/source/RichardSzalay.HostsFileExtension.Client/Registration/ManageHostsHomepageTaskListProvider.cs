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
    public partial class ManageHostsHomepageTaskListProvider : HomepageExtension
    {
        private ManageHostsModule module;
        private TaskList taskList;
        private ManagementConfigurationPath lastConfigPath;
        private IServiceProvider serviceProvider;

        public ManageHostsHomepageTaskListProvider(ManageHostsModule module)
        {
            this.module = module;
        }

        protected override TaskList  GetTaskList(IServiceProvider serviceProvider, ModulePageInfo selectedModulePage)
        {
            this.serviceProvider = serviceProvider;

            Connection connection = (Connection)serviceProvider.GetService(typeof(Connection));

            if (connection.ConfigurationPath != lastConfigPath || taskList == null)
            {
                //string siteName = connection.ConfigurationPath.SiteName;
                //var proxy = (ManageHostsFileModuleProxy)connection.CreateProxy(module, typeof(ManageHostsFileModuleProxy));
                //proxy.GetSiteBindings(siteName);

                taskList = new ManageHostsHomepageTaskList(this, serviceProvider);
                lastConfigPath = connection.ConfigurationPath;
            }

            return taskList;
        }

        protected override void OnRefresh()
        {
            this.taskList = null;
            this.lastConfigPath = null;

            base.OnRefresh();
        }

        private void SwitchBindingsAddress(IEnumerable<string> hosts, string address)
        {
            Connection connection = (Connection)serviceProvider.GetService(typeof(Connection));

            var proxy = module.ServiceProxy;

            var hostEntries = proxy.GetEntries();

            var entriesToAdd = hosts
                .Where(h => !hostEntries.Any(entry => entry.Hostname == h &&
                                            entry.Address == address))
                .Select(host => new HostEntry(host, address, null))
                .ToList();

            if (entriesToAdd.Count > 0)
            {
                proxy.AddEntries(entriesToAdd);
            }

            var entriesToEnableBefore = hosts
                .Select(host => hostEntries.FirstOrDefault(m => m.Hostname == host && m.Address == address))
                .Where(m => m != null)
                .ToList();

            var entriesToEnableAfter = entriesToEnableBefore
                .Select(e =>
                {
                    var newEntry = e.Clone();
                    newEntry.Enabled = true;
                    return newEntry;
                }).ToList();

            var entriesToDisableBefore = hostEntries
                .Where(m => hosts.Any(h => h == m.Hostname) &&
                            !entriesToEnableBefore.Contains(m))
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

            OnRefresh();
            UIService.Update();
        }

        private void DisableEntries(IList<HostEntry> enabledEntries)
        {
            Connection connection = (Connection)serviceProvider.GetService(typeof(Connection));

            var proxy = module.ServiceProxy;

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

            OnRefresh();
            UIService.Update();
        }

        private void GoToHostsView()
        {
            var connection = (Connection)serviceProvider.GetService(typeof(Connection));
            var navigationService = (INavigationService) serviceProvider.GetService(typeof (INavigationService));

            navigationService.Navigate(connection, connection.ConfigurationPath,
                typeof(ManageHostsModulePage), null);            
        }

        private IManagementUIService UIService
        {
            get { return (IManagementUIService)serviceProvider.GetService(typeof(IManagementUIService)); }
        } 
    }
}
