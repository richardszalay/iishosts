using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Administration;

namespace RichardSzalay.HostsFileExtension
{
    public class SiteBinding
    {
        public const string AnyAddress = "*";

        public SiteBinding(Binding iisBinding)
        {
            this.Host = iisBinding.Host;

            this.Protocol = iisBinding.Protocol;
            this.BindingInformation = iisBinding.BindingInformation;
        }

        public SiteBinding()
        {
        }

        public string Host { get; set; }

        public string Protocol { get; set; }

        public string BindingInformation { get; set; }

        public string Address
        {
            get
            {
                return (this.BindingInformation ?? String.Empty).Split(':')[0];
            }
        }

        public bool IsAnyAddress
        {
            get { return this.Address == AnyAddress; }
        }
    }
}
