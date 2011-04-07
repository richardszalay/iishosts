using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RichardSzalay.HostsFileExtension.Client.Model;

namespace RichardSzalay.HostsFileExtension
{
    public class GlobalHostEntryViewModelStrategy : IHostEntryViewModelStrategy
    {
        public IEnumerable<HostEntryViewModel> GetEntryModels(IEnumerable<HostEntry> localHostEntries)
        {
            return localHostEntries.Select(c => new HostEntryViewModel(c, false, null));
        }

        public IEnumerable<HostEntryViewModel> GetEntryModels(IEnumerable<HostEntry> localHostEntries, IEnumerable<System.Net.IPHostEntry> resolvedHostEntries)
        {
            throw new NotSupportedException();
        }
    }
}
