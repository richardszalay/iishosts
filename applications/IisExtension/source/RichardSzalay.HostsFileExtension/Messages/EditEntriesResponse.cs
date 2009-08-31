using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Server;

namespace RichardSzalay.HostsFileExtension.Messages
{
    public class EditEntriesResponse : VoidMessage
    {
        public EditEntriesResponse()
            : base()
        {
        }

        public EditEntriesResponse(PropertyBag bag)
            : base(bag)
        {
        }
    }
}
