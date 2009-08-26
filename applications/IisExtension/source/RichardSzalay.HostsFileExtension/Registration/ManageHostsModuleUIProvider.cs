using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Server;
using RichardSzalay.HostsFileExtension.View;
using System.Reflection;

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
            get { return null; }
        }

        public override bool SupportsScope(ManagementScope scope)
        {
            return (scope == ManagementScope.Server);
        }
    }
}
