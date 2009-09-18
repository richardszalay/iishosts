using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Administration;
using RichardSzalay.HostsFileExtension.Model;
using RichardSzalay.HostsFileExtension.Extensions;
using System.Net;
using System.Net.Sockets;

namespace RichardSzalay.HostsFileExtension
{
    public class SiteHostEntryViewModelStrategy : IHostEntryViewModelStrategy
    {
        private IEnumerable<SiteBinding> siteBindings;
        private IEnumerable<string> localAddresses;
        private string defaultAddress;

        public SiteHostEntryViewModelStrategy(IEnumerable<SiteBinding> siteBindings, IEnumerable<string> localAddresses)
        {
            this.siteBindings = siteBindings;
            this.localAddresses = localAddresses;
            this.defaultAddress = localAddresses.First();
        }

        public IEnumerable<HostEntryViewModel> GetEntryModels(IEnumerable<HostEntry> localHostEntries)
        {
            return this.GetEntryModels(localHostEntries, null);
        }

        public IEnumerable<HostEntryViewModel> GetEntryModels(IEnumerable<HostEntry> localHostEntries, IEnumerable<IPHostEntry> resolvedIPHostEntries)
        {
            IEnumerable<HostEntryViewModel> models = localHostEntries.Select(c => CreateEntryViewModel(c));

            bool resolvedEntriesAvailable = (resolvedIPHostEntries != null);

            if (resolvedEntriesAvailable)
            {
                models = this.AddResolvedEntries(models, resolvedIPHostEntries);
            }

            models = this.AddMissingEntryModels(models);

            return models;
        }

        private IEnumerable<HostEntryViewModel> AddResolvedEntries(IEnumerable<HostEntryViewModel> allModels, IEnumerable<IPHostEntry> hostEntries)
        {
            IDictionary<string, HostEntryViewModel> allEntriesMap = allModels.ToDictionary(c => c.HostEntry.Hostname);

            List<HostEntryViewModel> outputModels = allModels.ToList();

            foreach(IPHostEntry ipHostEntry in hostEntries)
            {
                bool alreadyHasEntry = allEntriesMap.ContainsKey(ipHostEntry.HostName);

                if (alreadyHasEntry)
                {
                    continue;
                }

                IEnumerable<SiteBinding> hostMatchedBindings = siteBindings.Where(c => c.Host == ipHostEntry.HostName);

                SiteBinding addressMatchedBinding = hostMatchedBindings
                    .Where(b => ipHostEntry.AddressList.Contains(ip => ip.ToString() == b.Address))
                    .FirstOrDefault();

                SiteBinding matchingBinding = addressMatchedBinding ??
                    hostMatchedBindings.FirstOrDefault(b => b.IsAnyAddress);

                bool isConflicted = false;
                
                if (matchingBinding == null)
                {
                    bool dnsIsConflicted = (hostMatchedBindings.Count() > 0);

                    if (dnsIsConflicted)
                    {
                        matchingBinding = hostMatchedBindings.First();
                        isConflicted = true;
                    }
                    else
                    {
                        continue;
                    }
                }

                string bindingAddress = matchingBinding.Address;

                string preferredAddress = IsAnyAddress(bindingAddress) ? defaultAddress : bindingAddress;

                IPAddress ipAddressToUse = ipHostEntry.AddressList.FirstOrDefault(c => c.AddressFamily == AddressFamily.InterNetwork)
                    ?? ipHostEntry.AddressList.FirstOrDefault(c => c.AddressFamily == AddressFamily.InterNetworkV6);

                string addressToUse = ipAddressToUse.ToString();

                bool isAddressLocal = localAddresses.Any(addr => ipHostEntry.AddressList.Contains(ip => ip.ToString() == addr));

                isConflicted = isConflicted || !isAddressLocal;

                HostEntry entry = new HostEntry(matchingBinding.Host, preferredAddress, null);

                HostEntryViewModel model = new HostEntryViewModel(entry, isConflicted, addressToUse);

                outputModels.Add(model);

                allEntriesMap.Add(entry.Hostname, model);
            }

            return outputModels;
        }

        private IEnumerable<HostEntryViewModel> AddMissingEntryModels(IEnumerable<HostEntryViewModel> allEntries)
        {
            Dictionary<string, HostEntryViewModel> allEntriesMap = allEntries.ToDictionary(c => c.HostEntry.Hostname);

            List<HostEntryViewModel> outputModels = new List<HostEntryViewModel>(allEntries);

            IList<string> siteHosts = siteBindings.Select(c => c.Host).Distinct().ToList();
            IList<string> invalidHosts = new List<string>(siteHosts);

            IList<string> addedHosts = new List<string>();

            IEnumerable<SiteBinding> missingBindings = siteBindings
                .Where(b => !allEntries.Contains(e => e.HostEntry.Hostname == b.Host));

            foreach (SiteBinding binding in missingBindings)
            {
                string host = binding.Host;
                string address = binding.Address;
                bool isAnyAddress = binding.IsAnyAddress;

                if (isAnyAddress)
                {
                    address = defaultAddress;
                }

                bool alreadyAdded = addedHosts.Contains(host);

                if (!alreadyAdded)
                {
                    HostEntryViewModel model = new HostEntryViewModel(
                        new HostEntry(host, address, null),
                        false,
                        null
                        );

                    outputModels.Add(model);
                    
                    invalidHosts.Remove(host);
                }
            }

            return outputModels;
        }

        private string GetBindingAddress(Binding binding)
        {
            return binding.BindingInformation.Split(':')[0];
        }

        private string GetBindingHostname(Binding binding)
        {
            return binding.Host;
        }

        private bool IsAnyAddress(string address)
        {
            const string AnyAddress = "*";

            return address == AnyAddress;
        }

        private HostEntryViewModel CreateEntryViewModel(HostEntry entry)
        {
            IEnumerable<SiteBinding> hostMatchingBindings = siteBindings.Where(c => c.Host == entry.Hostname);



            bool wouldResolve = hostMatchingBindings.Any(binding =>
                {
                    string bindingAddress = binding.Address;

                    return (binding.IsAnyAddress || bindingAddress == entry.Address);
                });

            bool isConflicted = !wouldResolve;

            SiteBinding bindingToUse = hostMatchingBindings.First();

            string preferredAddress = bindingToUse.IsAnyAddress
                ? defaultAddress
                : bindingToUse.Address;

            return new HostEntryViewModel(entry, isConflicted, preferredAddress);
        }
    }
}
