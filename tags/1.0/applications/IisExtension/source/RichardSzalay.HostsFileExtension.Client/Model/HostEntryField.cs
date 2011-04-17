using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichardSzalay.HostsFileExtension.Client.Model
{
    [Flags]
    public enum HostEntryField
    {
        None = 0,
        Hostname = 1,
        Address = 2,
        Enabled = 4,
        Comment = 8,
        All = Hostname | Address | Enabled | Comment
    }
}
