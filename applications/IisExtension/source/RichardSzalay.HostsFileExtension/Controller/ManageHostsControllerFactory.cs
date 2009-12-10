using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RichardSzalay.HostsFileExtension.Model;
using Microsoft.Web.Management.Client;

namespace RichardSzalay.HostsFileExtension.Controller
{
    public class ManageHostsControllerFactory : IManageHostsControllerFactory
    {
        IManageHostsController IManageHostsControllerFactory.Create(IServiceProvider serviceProvider, Module module)
        {
            return new ManageHostsController(serviceProvider, module);
        }
    }
}
