using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Management.Server;
using RichardSzalay.HostsFileExtension.Messages;
using System.Diagnostics;

namespace RichardSzalay.HostsFileExtension.Service
{
    public class ManageHostsFileModuleProxy : ModuleServiceProxy
    {
        public ManageHostsFileModuleProxy()
        {
        }

        public IList<HostEntry> GetEntries()
        {
            var request = new GetEntriesRequest();

            PropertyBag responseBag = (PropertyBag)base.Invoke("GetEntries", new object[] { request.ToPropertyBag() });

            var response = new GetEntriesResponse(responseBag);

            return response.Entries;
        }

        public void EditEntries(IList<HostEntry> originalEntries, IList<HostEntry> changedEntries)
        {
            Debug.Assert(originalEntries.Count == changedEntries.Count, "Number of original entries does not match changed entries");

            var request = new EditEntriesRequest(originalEntries, changedEntries);

            PropertyBag responseBag = (PropertyBag)base.Invoke("EditEntries", new object[] { request.ToPropertyBag() });

            var response = new EditEntriesResponse(responseBag);
        }

        public void AddEntries(IList<HostEntry> hostEntries)
        {
            var request = new AddEntriesRequest(hostEntries);

            PropertyBag responseBag = (PropertyBag)base.Invoke("AddEntries", new object[] { request.ToPropertyBag() });

            var response = new AddEntriesResponse(responseBag);
        }

        public void DeleteEntries(IList<HostEntry> entries)
        {
            var request = new DeleteEntriesRequest(entries);

            PropertyBag responseBag = (PropertyBag)base.Invoke("DeleteEntries", new object[] { request.ToPropertyBag() });

            var response = new DeleteEntriesResponse(responseBag);
        }

        public IEnumerable<SiteBinding> GetSiteBindings(string siteName)
        {
            var request = new GetSiteBindingHostnamesRequest(siteName);

            PropertyBag responseBag = (PropertyBag)base.Invoke("GetSiteBindings", new object[] { siteName });

            var response = new GetSiteBindingHostnamesResponse(responseBag);

            return response.Bindings;
        }

        public string[] GetServerAddresses()
        {
            var request = new GetServerAddressesRequest();

            PropertyBag responseBag = (PropertyBag)base.Invoke("GetServerAddresses");

            var response = new GetServerAddressesResponse(responseBag);

            return response.Addresses;
        }
    }
}
