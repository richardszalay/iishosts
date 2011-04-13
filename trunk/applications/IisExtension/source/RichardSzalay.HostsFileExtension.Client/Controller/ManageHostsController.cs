using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Client;
using System.Net;
using Microsoft.Web.Management.Server;
using RichardSzalay.HostsFileExtension.Service;
using System.ComponentModel;
using System.Threading;
using RichardSzalay.HostsFileExtension.Client.Model;

namespace RichardSzalay.HostsFileExtension.Client.Controller
{
    public class ManageHostsController : IManageHostsController
    {
        private Module module;

        private INavigationService navService;
        private Connection connection;

        private ManageHostsFileModuleProxy hostsFileProxy;
        
        bool isSite = false;

        private readonly object lockObject = new object();

        public ManageHostsController(IServiceProvider serviceProvider, Module module)
        {
            this.module = module;

            this.navService = (INavigationService)serviceProvider.GetService(typeof(INavigationService));

            this.connection = (Connection)serviceProvider.GetService(typeof(Connection));

            this.isSite = (connection.ConfigurationPath.PathType == ConfigurationPathType.Site);
            this.hostsFileProxy = (ManageHostsFileModuleProxy)connection.CreateProxy(module, typeof(ManageHostsFileModuleProxy));
        }

        public IEnumerable<HostEntryViewModel> GetHostEntryModels(Connection connection)
        {
            IEnumerable<HostEntry> hostEntries = this.hostsFileProxy.GetEntries();

            IHostEntryViewModelStrategy strategy = CreateViewModelStrategy();

            return strategy.GetEntryModels(hostEntries);
        }

        public IEnumerable<SiteBinding> GetSiteBindings(Connection connection)
        {
            return this.hostsFileProxy.GetSiteBindings(connection.ConfigurationPath.SiteName);
        }

        private IHostEntryViewModelStrategy CreateViewModelStrategy()
        {
            return new GlobalHostEntryViewModelStrategy();
        }
    }
}
