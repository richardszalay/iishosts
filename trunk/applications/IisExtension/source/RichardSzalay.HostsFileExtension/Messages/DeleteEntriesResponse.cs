using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Server;

namespace RichardSzalay.HostsFileExtension.Messages
{
    public class DeleteEntriesResponse : VoidMessage
    {
        public DeleteEntriesResponse()
            : base()
        {
        }

        public DeleteEntriesResponse(PropertyBag bag)
            : base(bag)
        {
        }
    }
}
