using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RichardSzalay.HostsFileExtension.Model;
using Microsoft.Web.Management.Client;

namespace RichardSzalay.HostsFileExtension.Controller
{
    public interface IManageHostsControllerFactory
    {
        IManageHostsController Create(IServiceProvider serviceProvider, Module module);
    }
}
