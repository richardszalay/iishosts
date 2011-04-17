using System;
using RichardSzalay.HostsFileExtension.Client.Model;
using System.Collections.Generic;
namespace RichardSzalay.HostsFileExtension.Client.Services
{
    public interface IHostEntrySelectionOptionsStrategy
    {
        HostEntrySelectionOptions GetOptions(IEnumerable<HostEntryViewModel> selectedModels);
    }
}
