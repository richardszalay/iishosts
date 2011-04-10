using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichardSzalay.HostsFileExtension.Client.Model
{
    public class HostEntrySelectionOptions
    {
        public bool CanEdit { get; set; }
        public HostEntryField EditableFields { get; set; }

        public bool CanSwitchAddress { get; set; }
        public ICollection<string> AlternateAddresses { get; set; }

        public bool CanEnable { get; set; }
        public bool CanDisable { get; set; }

        public bool CanDelete { get; set; }
    }
}
