using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Server;
using System.Diagnostics;

namespace RichardSzalay.HostsFileExtension.Messages
{
    public class HostEntryMessage : ServiceMessage
    {
        private HostEntry entry;

        public HostEntryMessage(HostEntry entry)
        {
            Debug.Assert(entry != null, "entry cannot be null");

            this.entry = entry;
        }

        public HostEntryMessage(PropertyBag bag)
            : base(bag)
        {
        }

        protected override void LoadMessage(PropertyBag bag)
        {
            this.entry = new HostEntry(bag);
        }

        protected override PropertyBag CreateMessagePropertyBag()
        {
            return this.entry.ToPropertyBag();
        }

        public HostEntry Entry
        {
            get { return entry; }
        }
    }
}
