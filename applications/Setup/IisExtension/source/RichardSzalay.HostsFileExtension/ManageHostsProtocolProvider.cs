using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Client.Extensions;
using Microsoft.Web.Management.Client;

namespace RichardSzalay.HostsFileExtension
{
    public class ManageHostsProtocolProvider : ProtocolProvider
    {
        public ManageHostsProtocolProvider(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public override TaskList GetSiteTaskList(string siteName, ICollection<string> bindingProtocols)
        {
            return null;
        }

        public override TaskList GetSitesTaskList()
        {
            return null;
        }
    }
}
