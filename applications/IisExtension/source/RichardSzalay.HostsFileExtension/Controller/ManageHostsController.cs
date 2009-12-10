using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RichardSzalay.HostsFileExtension.Model;
using Microsoft.Web.Management.Client;
using System.Net;
using Microsoft.Web.Management.Server;
using Microsoft.Web.Administration;
using RichardSzalay.HostsFileExtension.Service;
using System.ComponentModel;
using System.Threading;
using RichardSzalay.HostsFileExtension.Threading;

namespace RichardSzalay.HostsFileExtension.Controller
{
    public class ManageHostsController : IManageHostsController
    {
        private static readonly string[] ValidBindingProtocols = new string[]
        {
            "http", "https", "tcp"
        };

        private ITaskManager taskManager;

        private Module module;

        private INavigationService navService;
        private Connection connection;
        private IBindingInfoProvider bindingInfoProvider;
        private IBulkHostResolver bulkHostResolver;
        private IAddressProvider addressProvider;

        private ManageHostsFileModuleProxy hostsFileProxy;

        private IPHostEntry[] resolvedIPHostEntries;

        private IEnumerable<SiteBinding> siteBindings;

        bool isSite = false;

        private readonly object lockObject = new object();

        public ManageHostsController(IServiceProvider serviceProvider, Module module)
        {
            this.module = module;

            this.navService = (INavigationService)serviceProvider.GetService(typeof(INavigationService));

            this.connection = (Connection)serviceProvider.GetService(typeof(Connection));
            this.bindingInfoProvider = (IBindingInfoProvider)serviceProvider.GetService(typeof(IBindingInfoProvider));
            this.bulkHostResolver = (IBulkHostResolver)serviceProvider.GetService(typeof(IBulkHostResolver));
            this.addressProvider = (IAddressProvider)serviceProvider.GetService(typeof(IAddressProvider));

            this.taskManager = (ITaskManager)serviceProvider.GetService(typeof(ITaskManager));

            this.isSite = (connection.ConfigurationPath.PathType == ConfigurationPathType.Site);
            this.hostsFileProxy = (ManageHostsFileModuleProxy)connection.CreateProxy(module, typeof(ManageHostsFileModuleProxy));
        }

        public IEnumerable<HostEntryViewModel> GetHostEntryModels(Connection connection, bool useResolvedBindings)
        {
            IEnumerable<HostEntry> hostEntries = this.hostsFileProxy.GetEntries();

            if (isSite)
            {
                if (siteBindings == null)
                {
                    this.UpdateSiteBindings();
                }

                hostEntries = hostEntries.Where(e => siteBindings.Any(b => b.Host == e.Hostname));
            }

            IHostEntryViewModelStrategy strategy = CreateViewModelStrategy();

            return (useResolvedBindings && resolvedIPHostEntries != null)
                ? strategy.GetEntryModels(hostEntries, resolvedIPHostEntries)
                : strategy.GetEntryModels(hostEntries);
        }

        private void UpdateSiteBindings()
        {
            siteBindings = bindingInfoProvider.GetBindings(connection)
                        .Where(b => !String.IsNullOrEmpty(b.Host))
                        .Where(b => Array.IndexOf(ValidBindingProtocols, b.Protocol) != -1);
        }

        public void ResolveBindings(Connection connection, Action callback)
        {
            taskManager.QueueTask(() =>
                {
                    IAsyncResult result = null;

                    this.UpdateSiteBindings();

                    AsyncCallback asyncCallback = new AsyncCallback(r =>
                    {
                        if (result != null)
                        {
                            this.resolvedIPHostEntries = bulkHostResolver.EndGetHostEntries(result);

                            callback();
                        }
                    });

                    ISynchronizeInvoke syncInvoke = (ISynchronizeInvoke)navService.CurrentItem.Page;

                    result = bulkHostResolver.BeginGetHostEntries(
                        siteBindings.Select(b => b.Host).Where(h => !String.IsNullOrEmpty(h)),
                        null,
                        TimeSpan.FromMilliseconds(1000), // TODO: Config?
                        null,
                        null
                        );

                    if (result.CompletedSynchronously)
                    {
                        asyncCallback(result);
                    }
                });
        }

        private IHostEntryViewModelStrategy CreateViewModelStrategy()
        {
            if (isSite)
            {
                IEnumerable<string> addresses = addressProvider.GetAddresses();

                return new SiteHostEntryViewModelStrategy(siteBindings, addresses);
            }
            else
            {
                return new GlobalHostEntryViewModelStrategy();
            }
        }

        public void FixEntries(IEnumerable<RichardSzalay.HostsFileExtension.Model.HostEntryViewModel> hostEntryModels)
        {
            IEnumerable<HostEntryViewModel> newModels = hostEntryModels
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

            IEnumerable<HostEntryViewModel> conflictedModels = hostEntryModels
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
        }
    }
}
