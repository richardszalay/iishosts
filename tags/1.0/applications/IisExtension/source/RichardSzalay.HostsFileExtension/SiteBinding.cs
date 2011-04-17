using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Server;

namespace RichardSzalay.HostsFileExtension
{
    public class SiteBinding
    {
        public SiteBinding()
        {
        }

        public SiteBinding(PropertyBag bag)
        {
            Host = (string)bag[0];
            Address = (string)bag[1];
        }

        public string Host { get; set; }
        public string Address { get; set; }

        public bool IsAnyAddress { get { return Address == "*"; } }

        public PropertyBag ToPropertyBag()
        {
            var bag = new PropertyBag();

            bag[0] = this.Host;
            bag[1] = this.Address;

            return bag;
        }
    }
}
