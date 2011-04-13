using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Client;
using RichardSzalay.HostsFileExtension.Client.View;
using RichardSzalay.HostsFileExtension.Client.Properties;
using Microsoft.Web.Management.Server;
using System.ComponentModel.Design;
using Microsoft.Web.Management.Client.Extensions;
using RichardSzalay.HostsFileExtension.Client.Controller;
using RichardSzalay.HostsFileExtension.Client.Registration;

namespace RichardSzalay.HostsFileExtension.Client.Registration
{
    /// <summary>
    /// The starting registration point of the plugin
    /// </summary>
    /// <remarks>
    /// Registered by the ManageHostsModuleUIProvider (in the server project)
    /// </remarks>
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
                typeof(IHomepageTaskListProvider),
                new TestTaskListProvider(this)
            );
        }

        private void RegisterModulePage(IControlPanel controlPanel)
        {
            ModulePageInfo modulePageInfo = new ModulePageInfo(
                this,
                typeof(ManageHostsModulePage),
                Resources.ManageHostsIconTitle,
                Resources.ManageHostsIconDescription,
                Resources.ManageHostsFeatureImage,
                Resources.ManageHostsFeatureImage,
                Resources.ManageHostsIconDescription
            );

            controlPanel.RegisterPage(ControlPanelCategoryInfo.Management, modulePageInfo);
        }

        private void AddCommonServices(IServiceContainer serviceContainer)
        {
            serviceContainer.AddService(typeof(IManageHostsControllerFactory), new ManageHostsControllerFactory());
        }

        protected override bool IsPageEnabled(ModulePageInfo pageInfo)
        {
            Connection service = (Connection)this.GetService(typeof(Connection));

            if (service.ConfigurationPath.PathType != ConfigurationPathType.Server)
            {
                return false;
            }

            return base.IsPageEnabled(pageInfo);
        }
    }
}
