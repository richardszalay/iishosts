using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RichardSzalay.HostsFileExtension.View
{
    public interface IManageHostsModulePage
    {
        event EventHandler Refreshing;
        event EventHandler Initialized;

        ListView.ListViewItemCollection HostEntries { get; }
    }
}
