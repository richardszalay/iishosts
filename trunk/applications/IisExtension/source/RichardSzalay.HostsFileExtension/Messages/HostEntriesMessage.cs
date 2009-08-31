using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Server;

namespace RichardSzalay.HostsFileExtension.Messages
{
    public class HostEntriesMessage : ServiceMessage
    {
        private IList<HostEntry> hostEntries;

        public HostEntriesMessage(PropertyBag bag)
            : base(bag)
        {
        }

        public HostEntriesMessage(IList<HostEntry> hostEntries)
        {
            this.hostEntries = hostEntries;
        }

        protected override void LoadMessage(PropertyBag bag)
        {
            int entryCount = (int)bag[0];

            List<HostEntry> entries = new List<HostEntry>(entryCount);

            for (int i = 0; i < entryCount; i++)
            {
                PropertyBag entryBag = (PropertyBag)bag[1 + i];

                entries[i] = new HostEntry(entryBag);
            }
        }

        protected override PropertyBag CreateMessagePropertyBag()
        {
            PropertyBag bag = new PropertyBag();

            bag[0] = hostEntries.Count;

            for (int i = 0; i < hostEntries.Count; i++)
            {
                bag[1 + i] = hostEntries[i].ToPropertyBag();
            }

            return bag;
        }

        public IList<HostEntry> Entries
        {
            get { return hostEntries; }
        }
    }
}
