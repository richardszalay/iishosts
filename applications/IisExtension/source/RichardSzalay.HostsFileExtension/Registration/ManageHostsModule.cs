using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Client;
using RichardSzalay.HostsFileExtension.View;
using RichardSzalay.HostsFileExtension.Properties;
using Microsoft.Web.Management.Server;

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

            IControlPanel controlPanel = (IControlPanel)serviceProvider.GetService(typeof(IControlPanel));
            IExtensibilityManager extensibilityManager = (IExtensibilityManager)serviceProvider.GetService(typeof(IExtensibilityManager));

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
