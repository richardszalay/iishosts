using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RichardSzalay.HostsFileExtension.Model;
using Microsoft.Web.Management.Client;

namespace RichardSzalay.HostsFileExtension.Controller
{
    public interface IManageHostsController
    {
        IEnumerable<HostEntryViewModel> GetHostEntryModels(Connection connection, bool useResolvedBindings);

        void FixEntries(IEnumerable<HostEntryViewModel> models);

        void ResolveBindings(Connection connection, Action callback);
    }
}
