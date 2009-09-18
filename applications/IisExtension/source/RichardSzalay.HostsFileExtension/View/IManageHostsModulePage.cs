using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Management.Client.Win32;
using Microsoft.Web.Management.Server;
using RichardSzalay.HostsFileExtension.Model;
using System.ComponentModel;

namespace RichardSzalay.HostsFileExtension.View
{
    public interface IManageHostsModulePage : IWin32Window, IServiceProvider, ISynchronizeInvoke
    {
        event EventHandler Refreshing;
        event EventHandler Initialized;
        event EventHandler ListItemDoubleClick;
        event EventHandler SearchFilterChanged;
        event EventHandler DeleteSelected;

        string SearchFilter { get; }

        void SetHostEntries(IEnumerable<HostEntryViewModel> hostEntries);

        IEnumerable<HostEntryViewModel> SelectedEntries { get; }
        void SetTaskList(TaskList taskList);

        IServiceProvider ServiceProvider { get; }

        DialogResult ShowDialog(DialogForm form);

        T CreateProxy<T>() where T : ModuleServiceProxy;

        ConfigurationPathType ConfigurationPathType { get; }

        Connection Connection { get; }
    }
}
