using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RichardSzalay.HostsFileExtension.View
{
    public interface IManageHostsModulePage
    {
        event EventHandler Refresh;
        event EventHandler Load;

        public ListView.ListViewItemCollection HostEntries { get; set; }
    }
}
