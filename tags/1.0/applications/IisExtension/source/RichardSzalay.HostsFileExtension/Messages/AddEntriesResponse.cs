using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Server;

namespace RichardSzalay.HostsFileExtension.Messages
{
    public class AddEntriesResponse : VoidMessage
    {
        public AddEntriesResponse()
            : base()
        {
        }

        public AddEntriesResponse(PropertyBag bag)
            : base(bag)
        {
        }
    }
}
