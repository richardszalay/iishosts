using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Server;
using RichardSzalay.HostsFileExtension.View;
using System.Reflection;
using RichardSzalay.HostsFileExtension.Service;

namespace RichardSzalay.HostsFileExtension.Registration
{
    public class ManageHostsModuleUIProvider : ModuleProvider
    {
        public override ModuleDefinition GetModuleDefinition(IManagementContext context)
        {
            return new ModuleDefinition(Name, typeof(ManageHostsModule).AssemblyQualifiedName);
        }

        public override Type ServiceType
        {
            get { return typeof(ManageHostFileModuleService); }
        }

        public override bool SupportsScope(ManagementScope scope)
        {
            return (scope == ManagementScope.Server);
        }
    }
}
