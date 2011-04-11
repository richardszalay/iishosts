using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Server;

namespace RichardSzalay.HostsFileExtension.Messages
{
    public class GetServerAddressesRequest : VoidMessage
    {
        public GetServerAddressesRequest()
        {
        }

        public GetServerAddressesRequest(PropertyBag bag)
            : base(bag)
        {
        }
    }
}
