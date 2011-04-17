using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Server;
using System.Reflection;
using RichardSzalay.HostsFileExtension.Service;

namespace RichardSzalay.HostsFileExtension.Registration
{
    /// <summary>
    /// The starting registration point of the plugin
    /// </summary>
    /// <remarks>
    /// Registered in %windir%\system32\inetsrv\config\administration.config
    /// </remarks>
    public class ManageHostsModuleUIProvider : ModuleProvider
    {
        public override ModuleDefinition GetModuleDefinition(IManagementContext context)
        {
            return new ModuleDefinition(Name,
                "RichardSzalay.HostsFileExtension.Client.Registration.ManageHostsModule, " + GetClientAssemblyName());
        }

        public override Type ServiceType
        {
            get { return typeof(ManageHostFileModuleService); }
        }

        public override bool SupportsScope(ManagementScope scope)
        {
            return (scope == ManagementScope.Server ||
                scope == ManagementScope.Site);
        }

        private static string GetClientAssemblyName()
        {
            AssemblyName assemblyName = typeof(ManageHostsModuleUIProvider).Assembly.GetName();

            return assemblyName.FullName.Replace(assemblyName.Name, assemblyName.Name + ".Client");
        }

        public IEnumerable<string> SupportedProtocols
        {
            get
            {
                return new string[] { Uri.UriSchemeHttp, Uri.UriSchemeHttps };
            }
        }
    }
}
