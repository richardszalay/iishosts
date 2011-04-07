using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Server;

namespace RichardSzalay.HostsFileExtension.Messages
{
    public class GetSiteBindingHostnamesRequest : ServiceMessage
    {
        private string siteName;

        public GetSiteBindingHostnamesRequest(string siteName)
        {
            this.siteName = siteName;
        }

        protected override void LoadMessage(Microsoft.Web.Management.Server.PropertyBag bag)
        {
            siteName = (string)bag[0];
        }

        protected override Microsoft.Web.Management.Server.PropertyBag CreateMessagePropertyBag()
        {
            PropertyBag bag = new PropertyBag();
            bag[0] = siteName;

            return bag;
        }
    }
}
