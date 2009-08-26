using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Client;
using RichardSzalay.HostsFileExtension.View;
using RichardSzalay.HostsFileExtension.Properties;

namespace RichardSzalay.HostsFileExtension.Registration
{
    public class ManageHostsModule : Module
    {
        protected override void Initialize(IServiceProvider serviceProvider, Microsoft.Web.Management.Server.ModuleInfo moduleInfo)
        {
            base.Initialize(serviceProvider, moduleInfo);

            IControlPanel controlPanel = (IControlPanel)serviceProvider.GetService(typeof(IControlPanel));

            ModulePageInfo modulePageInfo = new ModulePageInfo(
                this,
                typeof(ManageHostsModulePage),
                Resources.ManageHostsIconTitle,
                Resources.ManageHostsIconDescription
                );

            controlPanel.RegisterPage(modulePageInfo);
        }
    }
}
