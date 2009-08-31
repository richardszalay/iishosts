using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Server;

namespace RichardSzalay.HostsFileExtension.Messages
{
    public class GetEntriesRequest : VoidMessage
    {
        public GetEntriesRequest()
        {
        }

        public GetEntriesRequest(PropertyBag bag)
            : base(bag)
        {
        }
    }
}
