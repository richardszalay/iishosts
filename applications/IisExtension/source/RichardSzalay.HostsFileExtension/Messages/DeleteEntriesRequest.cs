using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Server;

namespace RichardSzalay.HostsFileExtension.Messages
{
    public class DeleteEntriesRequest : HostEntriesMessage
    {
        public DeleteEntriesRequest(PropertyBag bag)
            : base(bag)
        {
        }

        public DeleteEntriesRequest(IList<HostEntry> hostEntries)
            : base(hostEntries)
        {
        }

    }
}
