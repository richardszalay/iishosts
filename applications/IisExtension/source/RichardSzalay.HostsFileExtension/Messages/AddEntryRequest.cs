using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Server;

namespace RichardSzalay.HostsFileExtension.Messages
{
    public class AddEntryRequest : HostEntryMessage
    {
        public AddEntryRequest(HostEntry entry)
            : base(entry)
        {
        }

        public AddEntryRequest(PropertyBag bag)
            : base(bag)
        {
        }
    }
}
