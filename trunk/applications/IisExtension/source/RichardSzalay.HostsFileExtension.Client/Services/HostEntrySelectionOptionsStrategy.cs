using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RichardSzalay.HostsFileExtension.Client.Model;

namespace RichardSzalay.HostsFileExtension.Client.Services
{
    public class HostEntrySelectionOptionsStrategy : RichardSzalay.HostsFileExtension.Client.Services.IHostEntrySelectionOptionsStrategy
    {
        private IEnumerable<HostEntryViewModel> entryModels;

        public HostEntrySelectionOptionsStrategy(IEnumerable<HostEntryViewModel> entryModels)
        {
            this.entryModels = entryModels;
        }

        public HostEntrySelectionOptions GetOptions(IEnumerable<HostEntryViewModel> selectedModels)
        {
            var options = new HostEntrySelectionOptions();

            IEnumerable<string> alternateAddresses = null;

            HashSet<string> uniqueAddresses = new HashSet<string>();
            HashSet<string> uniqueHostnames = new HashSet<string>();
            HashSet<string> uniqueComments = new HashSet<string>();

            foreach (var model in selectedModels)
            {
                uniqueAddresses.Add(model.HostEntry.Address);
                uniqueComments.Add(model.HostEntry.Comment);

                if (uniqueHostnames.Add(model.HostEntry.Hostname))
                {
                    alternateAddresses = (alternateAddresses == null)
                        ? GetAlternateAddresses(model.HostEntry)
                        : alternateAddresses.Union(GetAlternateAddresses(model.HostEntry));
                }

                if (model.HostEntry.Enabled)
                {
                    options.CanDisable = true;
                }
                else
                {
                    options.CanEnable = true;
                }

                options.CanEdit = true;
                options.CanDelete = true;
                options.CanSwitchAddress = true;
            }

            options.AlternateAddresses = (alternateAddresses ?? new string[0]).ToList();

            if (uniqueAddresses.Count == 1)
            {
                options.EditableFields |= HostEntryField.Address;
            }

            if (uniqueHostnames.Count == 1)
            {
                options.EditableFields |= HostEntryField.Hostname;
            }

            if (uniqueComments.Count == 1)
            {
                options.EditableFields |= HostEntryField.Comment;
            }

            if (options.CanEnable ^ options.CanDisable)
            {
                options.EditableFields |= HostEntryField.Enabled;
            }

            return options;
        }

        private IEnumerable<string> GetAlternateAddresses(HostEntry entry)
        {
            return entryModels
                .Where(x => x.HostEntry.Hostname == entry.Hostname &&
                            !x.HostEntry.Enabled)
                .Select(x => x.HostEntry.Address)
                .Distinct()
                .ToList();
        }
    }
}
