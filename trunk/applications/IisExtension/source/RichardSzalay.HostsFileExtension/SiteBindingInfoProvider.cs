using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Administration;

namespace RichardSzalay.HostsFileExtension
{
    public class SiteBindingInfoProvider : IBindingInfoProvider
    {
        public IEnumerable<Binding> GetBindings(Connection connection)
        {
            List<Binding> bindings = new List<Binding>();

            using (ServerManager manager = ServerManager.OpenRemote(connection.Name))
            {
                Site site = manager.Sites[connection.ConfigurationPath.SiteName];

                foreach (Binding binding in site.Bindings)
                {
                    bindings.Add(binding);
                }
            }

            return bindings;
        }
    }
}
