using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Server;

namespace RichardSzalay.HostsFileExtension.Messages
{
    public class EditEntriesRequest : ServiceMessage
    {
        private IList<HostEntry> originalEntries;
        private IList<HostEntry> changedEntries;

        public EditEntriesRequest(IList<HostEntry> originalEntries, IList<HostEntry> changedEntries)
        {
            this.originalEntries = originalEntries;
            this.changedEntries = changedEntries;
        }

        public EditEntriesRequest(PropertyBag bag)
            : base(bag)
        {
        }

        protected override void LoadMessage(PropertyBag bag)
        {
            PropertyBag originalEntriesBag = (PropertyBag)bag[0];
            PropertyBag changedEntriesBag = (PropertyBag)bag[1];

            this.originalEntries = LoadHostEntryList(originalEntriesBag);
            this.changedEntries = LoadHostEntryList(changedEntriesBag);
        }

        protected override PropertyBag CreateMessagePropertyBag()
        {
            PropertyBag bag = new PropertyBag();
            bag[0] = CreateHostEntryListPropertyBag(originalEntries);
            bag[1] = CreateHostEntryListPropertyBag(changedEntries);

            return bag;
        }

        protected IList<HostEntry> LoadHostEntryList(PropertyBag bag)
        {
            int entryCount = (int)bag[0];

            List<HostEntry> entries = new List<HostEntry>(entryCount);

            for (int i = 0; i < entryCount; i++)
            {
                PropertyBag entryBag = (PropertyBag)bag[1 + i];

                entries[i] = new HostEntry(entryBag);
            }

            return entries;
        }

        protected PropertyBag CreateHostEntryListPropertyBag(IList<HostEntry> hostEntries)
        {
            PropertyBag bag = new PropertyBag();

            bag[0] = hostEntries.Count;

            for (int i = 0; i < hostEntries.Count; i++)
            {
                bag[1 + i] = hostEntries[i].ToPropertyBag();
            }

            return bag;
        }

        public IList<HostEntry> OriginalEntries
        {
            get { return originalEntries; }
        }

        public IList<HostEntry> ChangedEntries
        {
            get { return changedEntries; }
        }
    }
}
