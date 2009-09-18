using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RichardSzalay.HostsFileExtension.Model;
using System.Net;

namespace RichardSzalay.HostsFileExtension
{
    interface IHostEntryViewModelStrategy
    {
        IEnumerable<HostEntryViewModel> GetEntryModels(IEnumerable<HostEntry> localHostEntries);
        IEnumerable<HostEntryViewModel> GetEntryModels(IEnumerable<HostEntry> localHostEntries, IEnumerable<IPHostEntry> resolvedHostEntries);
    }
}
