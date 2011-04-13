using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Client.Extensions;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Management.Server;
using RichardSzalay.HostsFileExtension.Service;
using System.Collections;

namespace RichardSzalay.HostsFileExtension.Client.Registration
{
    public class TestTaskListProvider : IHomepageTaskListProvider
    {
        private readonly ManageHostsModule module;

        public TestTaskListProvider(ManageHostsModule module)
        {
            this.module = module;
        }

        public Microsoft.Web.Management.Client.TaskList GetTaskList(IServiceProvider serviceProvider, Microsoft.Web.Management.Client.ModulePageInfo selectedModulePage)
        {
            TaskList taskList = new TestTaskList();

            Connection connection = (Connection)serviceProvider.GetService(typeof(Connection));

            if (connection.ConfigurationPath.PathType == ConfigurationPathType.Site)
            {
                string siteName = connection.ConfigurationPath.SiteName;

                var proxy = (ManageHostsFileModuleProxy)connection.CreateProxy(module, typeof(ManageHostsFileModuleProxy));

                proxy.GetSiteBindings(siteName);
            }

            return taskList;        
        }

        private class TestTaskList : TaskList
        {
            public override System.Collections.ICollection GetTaskItems()
            {
                return new ArrayList();
            }
        }
    }
}
