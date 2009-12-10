using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Administration;

namespace RichardSzalay.HostsFileExtension
{
    public interface IBindingInfoProvider
    {
        IEnumerable<SiteBinding> GetBindings(Connection connection);
    }
}
