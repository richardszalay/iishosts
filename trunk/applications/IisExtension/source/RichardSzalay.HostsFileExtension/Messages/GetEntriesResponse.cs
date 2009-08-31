using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Server;

namespace RichardSzalay.HostsFileExtension.Messages
{
    public class GetEntriesResponse : HostEntriesMessage
    {
        public GetEntriesResponse(PropertyBag bag)
            : base(bag)
        {
        }

        public GetEntriesResponse(IList<HostEntry> hostEntries)
            : base(hostEntries)
        {
        }
    }
}
