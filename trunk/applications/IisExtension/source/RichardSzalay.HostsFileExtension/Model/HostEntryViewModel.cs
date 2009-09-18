using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichardSzalay.HostsFileExtension.Model
{
    public class HostEntryViewModel
    {
        public HostEntry HostEntry { get; private set; }
        public bool Conflicted { get; set; }
        public string PreferredAddress { get; set; }

        public HostEntryViewModel(HostEntry entry, bool conflicted, string preferredAddress)
        {
            this.HostEntry = entry;
            this.Conflicted = conflicted;
            this.PreferredAddress = preferredAddress;
        }
    }
}
