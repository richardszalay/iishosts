using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Client.Extensions;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Management.Server;
using System.Diagnostics;

namespace RichardSzalay.HostsFileExtension.Registration
{
    class ManageHostsHomepageTaskListProvider : IHomepageTaskListProvider
    {
        #region IHomepageTaskListProvider Members

        public Microsoft.Web.Management.Client.TaskList GetTaskList(IServiceProvider serviceProvider, Microsoft.Web.Management.Client.ModulePageInfo selectedModulePage)
        {
            Connection connection = (Connection)serviceProvider.GetService(typeof(Connection));

            bool isSite = (connection.ConfigurationPath.PathType == ConfigurationPathType.Site);

            return null;
        }

        #endregion
    }
}
