using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Client;
using RichardSzalay.HostsFileExtension.View;
using RichardSzalay.HostsFileExtension.Properties;
using Microsoft.Web.Management.Server;
using System.ComponentModel.Design;
using Microsoft.Web.Management.Client.Extensions;

namespace RichardSzalay.HostsFileExtension.Registration
{
    public class ManageHostsModule : Module
    {
        public ManageHostsModule()
        {
        }

        protected override void Initialize(IServiceProvider serviceProvider, Microsoft.Web.Management.Server.ModuleInfo moduleInfo)
        {
            base.Initialize(serviceProvider, moduleInfo);

            IServiceContainer serviceContainer = (IServiceContainer)serviceProvider.GetService(typeof(IServiceContainer));
            this.AddCommonServices(serviceContainer);

            IExtensibilityManager extensibilityManager = (IExtensibilityManager)serviceProvider.GetService(typeof(IExtensibilityManager));
            this.RegisterProtocolProvider(serviceProvider, extensibilityManager);

            IControlPanel controlPanel = (IControlPanel)serviceProvider.GetService(typeof(IControlPanel));
            this.RegisterModulePage(controlPanel);
        }

        private void RegisterProtocolProvider(IServiceProvider provider, IExtensibilityManager extensibilityManager)
        {
            extensibilityManager.RegisterExtension(
                typeof(ProtocolProvider),
                new ManageHostsProtocolProvider(provider)
            );

            extensibilityManager.RegisterExtension(
                typeof(IHomepageTaskListProvider),
                new ManageHostsHomepageTaskListProvider(this)
            );
        }

        private void RegisterModulePage(IControlPanel controlPanel)
        {
            ModulePageInfo modulePageInfo = new ModulePageInfo(
                this,
                typeof(ManageHostsModulePage),
                Resources.ManageHostsIconTitle,
                Resources.ManageHostsIconDescription,
                null, null,
                Resources.ManageHostsIconDescription
            );

            controlPanel.RegisterPage(ControlPanelCategoryInfo.Management, modulePageInfo);
        }

        private void AddCommonServices(IServiceContainer serviceContainer)
        {
            serviceContainer.AddService(typeof(IBindingInfoProvider), new SiteBindingInfoProvider());
            serviceContainer.AddService(typeof(IAddressProvider), new DnsAddressProvider());
            serviceContainer.AddService(typeof(IBulkHostResolver), new DnsBulkHostResolver());
        }

        protected override bool IsPageEnabled(ModulePageInfo pageInfo)
        {
            Connection service = (Connection)this.GetService(typeof(Connection));

            if (service.ConfigurationPath.PathType != ConfigurationPathType.Server &&
                service.ConfigurationPath.PathType != ConfigurationPathType.Site)
            {
                return false;
            }

            return base.IsPageEnabled(pageInfo);
        }
    }
}
