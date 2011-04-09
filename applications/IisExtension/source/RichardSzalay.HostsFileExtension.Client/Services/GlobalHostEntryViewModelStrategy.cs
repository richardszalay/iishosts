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
            var enabledHostnameCounts = localHostEntries
                .Where(entry => entry.Enabled)
                .GroupBy(entry => entry.Hostname)
                .ToDictionary(entry => entry.Key, entry => entry.Count());

            return localHostEntries.Select(c => new HostEntryViewModel(c,
                c.Enabled && enabledHostnameCounts[c.Hostname] > 1, null));
        }

        public IEnumerable<HostEntryViewModel> GetEntryModels(IEnumerable<HostEntry> localHostEntries, IEnumerable<System.Net.IPHostEntry> resolvedHostEntries)
        {
            throw new NotSupportedException();
        }
    }
}
