using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Client;
using RichardSzalay.HostsFileExtension.Client.Model;

namespace RichardSzalay.HostsFileExtension.Client.Controller
{
    public interface IManageHostsController
    {
        IEnumerable<HostEntryViewModel> GetHostEntryModels(Connection connection);
        IEnumerable<SiteBinding> GetSiteBindings(Connection connection);
    }
}
