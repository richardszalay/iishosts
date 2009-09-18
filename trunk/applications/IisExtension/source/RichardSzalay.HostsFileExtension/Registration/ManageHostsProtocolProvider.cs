using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Client.Extensions;
using Microsoft.Web.Management.Client;

namespace RichardSzalay.HostsFileExtension.Registration
{
    public class ManageHostsProtocolProvider : ProtocolProvider
    {
        private IServiceProvider serviceProvider;

        public ManageHostsProtocolProvider(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this.serviceProvider = serviceProvider;

            base.SiteUpdated = OnSiteUpdatedHandler;
        }

        private void OnSiteUpdatedHandler(object sender, SiteUpdatedEventArgs e)
        {
            Connection connection = (Connection)serviceProvider.GetService(typeof(Connection));
        }

        /*
        public override TaskList GetSiteTaskList(string siteName, ICollection<string> bindingProtocols)
        {
            
        }*/

        private static readonly string[] ValidProtocols = new string[] { "http", "https", "tcp" };

        public override TaskList GetSiteTaskList(string siteName, ICollection<string> bindingProtocols)
        {
            return null;
        }

        public override TaskList GetSitesTaskList()
        {
            return null;
        }

        /*
        public override bool IsIPPortProtocol
        {
            get
            {
                return true;
            }
        }

        public override string SupportedProtocol
        {
            get
            {
                return "http";
            }
        }*/
    }
}
