using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Management.Client.Win32;
using Microsoft.Web.Management.Server;
using RichardSzalay.HostsFileExtension.Client.Properties;
using RichardSzalay.HostsFileExtension.Service;
using RichardSzalay.HostsFileExtension.Client.Services;
using RichardSzalay.HostsFileExtension.Client.View;
using System.Windows.Forms;

namespace RichardSzalay.HostsFileExtension.Client.Registration
{
    partial class ManageHostsHomepageTaskListProvider
    {
        public class ManageHostsHomepageTaskList : TaskList
        {
            private const string HostsTaskCategory = "Hosts";
            private readonly Connection connection;
            private readonly ManageHostsHomepageTaskListProvider owner;
            private readonly IServiceProvider serviceProvider;
            private readonly ManageHostsModule module;
            
            private SiteBinding[] bindings;
            private List<Model.HostEntryViewModel> hostEntries;
            private string[] enabledHostEntryAddresses;
            private ICollection<string> alternateAddresses;
            private bool hasEnabledBindingEntries;
            private string[] addresses;

            public ManageHostsHomepageTaskList(ManageHostsHomepageTaskListProvider owner, IServiceProvider serviceProvider)
            {
                this.owner = owner;
                this.module = owner.module;
                this.serviceProvider = serviceProvider;
                this.connection = (Connection)serviceProvider.GetService(typeof(Connection));

                this.Expanded = true;
            }

            #region Task Handlers

            public void SwitchBindingsAddress(string address)
            {
                var hosts = bindings.Select(x => x.Host).Distinct().ToList();

                this.owner.SwitchBindingsAddress(hosts, address);

                UIService.Update();
            }

            public void SwitchBindingsAddressToManual()
            {
                using (var form = new SelectSwitchAddress(serviceProvider, addresses))
                {
                    var result = UIService.ShowDialog(form);

                    if (result == DialogResult.OK)
                    {
                        var hosts = bindings.Select(x => x.Host).Distinct().ToList();

                        owner.SwitchBindingsAddress(hosts, form.Address);
                    }
                }
            }

            public void DisableAllBindingEntries()
            {
                var enabledEntries = this.hostEntries
                    .Select(m => m.HostEntry)
                    .Where(x => x.Enabled && bindings.Any(b => b.Host == x.Hostname))
                    .ToList();

                this.owner.DisableEntries(enabledEntries);
            }

            public void GoToHostsView()
            {
                this.owner.GoToHostsView();
            }

            #endregion

            private IManagementUIService UIService
            {
                get { return (IManagementUIService)serviceProvider.GetService(typeof(IManagementUIService)); }
            }

            public override System.Collections.ICollection GetTaskItems()
            {
                var list = new ArrayList();

                var currentConnection = (Connection)serviceProvider.GetService(typeof(Connection));

                var isSite = (currentConnection.ConfigurationPath.PathType == ConfigurationPathType.Site);

                if (isSite)
                {
                    this.EnsureHostInfo();

                    if (bindings.Length > 0)
                    {
                        list.Add(CreateHostChangesGroup());
                    }
                }

                return list;
            }

            private void EnsureHostInfo()
            {
                if (hostEntries == null)
                {
                    var proxy = this.module.ServiceProxy;

                    this.bindings = proxy.GetSiteBindings(connection.ConfigurationPath.SiteName).ToArray();

                    this.addresses = proxy.GetServerAddresses();

                    var strategy = new GlobalHostEntryViewModelStrategy();

                    var allEntries = strategy.GetEntryModels(proxy.GetEntries())
                        .ToList();

                    this.hostEntries = allEntries
                        .Where(x => bindings.Any(b => b.Host == x.HostEntry.Hostname))
                        .ToList();

                    this.enabledHostEntryAddresses = hostEntries
                        .Where(e => e.HostEntry.Enabled)
                        .Select(e => e.HostEntry.Address)
                        .Distinct()
                        .ToArray();

                    this.alternateAddresses = new HostEntrySelectionOptionsStrategy(hostEntries)
                        .GetOptions(hostEntries).AlternateAddresses;

                    this.hasEnabledBindingEntries = hostEntries.Any(x => x.HostEntry.Enabled);
                }
            }

            private TaskItem CreateHostChangesGroup()
            {
                GroupTaskItem taskItem = new GroupTaskItem("Expanded", Resources.SiteTaskListHostFileTitle, HostsTaskCategory, true);

                string currentAddressValue = FormatCurrentAddress(this.enabledHostEntryAddresses);

                TextTaskItem item = new TextTaskItem(currentAddressValue, HostsTaskCategory, true);
                taskItem.Items.Add(item);

                if (this.alternateAddresses != null)
                {
                    foreach (var alternateAddress in this.alternateAddresses)
                    {
                        taskItem.Items.Add(new MethodTaskItem("SwitchBindingsAddress",
                            String.Format(Resources.SwitchBindingsAddressTask, alternateAddress),
                            HostsTaskCategory,
                            String.Format(Resources.SwitchBindingAddressDescription, alternateAddress),
                            null, alternateAddress));
                    }
                }

                taskItem.Items.Add(new MethodTaskItem("SwitchBindingsAddressToManual",
                        Resources.SwitchBindingsAddressesToManualTask,
                        HostsTaskCategory));

                if (this.hasEnabledBindingEntries)
                {
                    taskItem.Items.Add(new MethodTaskItem("DisableAllBindingEntries",
                        Resources.DisableBindingEntriesTask,
                        HostsTaskCategory, Resources.DisableBindingEntriesDescription));
                }

                taskItem.Items.Add(new MethodTaskItem("GoToHostsView", Resources.EditHostsTask, HostsTaskCategory));

                return taskItem;
            }

            private string FormatCurrentAddress(string[] addresses)
            {
                if (addresses.Length == 0)
                {
                    return Resources.SiteCurrentAddressNone;
                }
                
                if (addresses.Length > 1)
                {
                    return String.Format(Resources.SiteCurrentAddressMultiple,
                                         addresses[0], addresses.Length - 1);
                }
                
                return addresses[0];
            }

            // Required as the task group "binds" its state to this property
            public bool Expanded { get; set; }
        }
    }
}
