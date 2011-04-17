using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Web.Management.Client;

namespace RichardSzalay.HostsFileExtension.Client.Controller
{
    public interface IManageHostsControllerFactory
    {
        IManageHostsController Create(IServiceProvider serviceProvider, Module module);
    }
}
