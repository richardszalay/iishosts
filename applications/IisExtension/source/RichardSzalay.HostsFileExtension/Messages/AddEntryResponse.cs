using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Server;

namespace RichardSzalay.HostsFileExtension.Messages
{
    public class AddEntryResponse : VoidMessage
    {
        public AddEntryResponse()
            : base()
        {
        }

        public AddEntryResponse(PropertyBag bag)
            : base(bag)
        {
        }
    }
}
