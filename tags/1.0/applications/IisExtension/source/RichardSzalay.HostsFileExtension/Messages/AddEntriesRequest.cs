using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Server;

namespace RichardSzalay.HostsFileExtension.Messages
{
    public class AddEntriesRequest : HostEntriesMessage
    {
        public AddEntriesRequest(IList<HostEntry> entries)
            : base(entries)
        {
        }

        public AddEntriesRequest(PropertyBag bag)
            : base(bag)
        {
        }
    }
}
