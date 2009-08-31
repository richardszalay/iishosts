﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Server;
using RichardSzalay.HostsFileExtension.Messages;

namespace RichardSzalay.HostsFileExtension.Service
{
    public class ManageHostFileModuleService : ModuleService
    {
        public PropertyBag GetEntries(PropertyBag bag)
        {
            HostsFile hostsFile = GetHostsFile();

            IList<HostEntry> entries = hostsFile.Entries.ToList();

            var response = new GetEntriesResponse(entries);

            return response.ToPropertyBag();
        }

        public PropertyBag AddEntry(PropertyBag bag)
        {
            return CatchCommonExceptions(() =>
                {
                    AddEntryRequest request = new AddEntryRequest(bag);

                    HostEntry hostEntry = request.Entry;

                    HostsFile hostsFile = GetHostsFile();

                    hostsFile.AddEntry(hostEntry);
                    hostsFile.Save();

                    return new AddEntryResponse().ToPropertyBag();
                });
        }

        public PropertyBag EditEntries(PropertyBag bag)
        {
            return CatchCommonExceptions(() =>
                {
                    EditEntriesRequest request = new EditEntriesRequest(bag);

                    HostsFile hostsFile = GetHostsFile();

                    IEnumerable<HostEntry> hostEntries = hostsFile.Entries;

                    for (int i = 0; i < request.ChangedEntries.Count; i++)
                    {
                        HostEntry originalEntry = request.OriginalEntries[i];
                        HostEntry changedEntry = request.ChangedEntries[i];

                        HostEntry hostEntry = FindHostEntry(originalEntry, hostEntries);

                        hostEntry.Address = changedEntry.Address;
                        hostEntry.Hostname = changedEntry.Hostname;
                        hostEntry.Comment = changedEntry.Comment;
                        hostEntry.Enabled = changedEntry.Enabled;
                    }

                    hostsFile.Save();

                    return new EditEntriesResponse().ToPropertyBag();
                });
        }

        public PropertyBag DeleteEntries(PropertyBag bag)
        {
            return CatchCommonExceptions(() =>
            {
                DeleteEntriesRequest request = new DeleteEntriesRequest(bag);

                HostsFile hostsFile = GetHostsFile();

                IEnumerable<HostEntry> hostEntries = hostsFile.Entries;

                foreach(HostEntry remoteEntry in request.Entries)
                {
                    HostEntry localEntry = FindHostEntry(remoteEntry, hostEntries);

                    hostsFile.DeleteEntry(localEntry);
                }

                hostsFile.Save();

                return new DeleteEntriesResponse().ToPropertyBag();
            });
        }

        private HostEntry FindHostEntry(HostEntry entryToFind, IEnumerable<HostEntry> entries)
        {
            foreach (HostEntry hostEntry in entries)
            {
                if (entryToFind.ToString() == hostEntry.ToString())
                {
                    return hostEntry;
                }
            }

            throw new HostsFileServiceException("File has changed. Please reload");
        }

        private PropertyBag CatchCommonExceptions(Func<PropertyBag> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                return ServiceMessage.CreateError(ex.Message);
            }
        }

        private HostsFile GetHostsFile()
        {
            return new HostsFile();
        }
    }
}
