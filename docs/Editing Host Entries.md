Once Hosts File Manager is installed, the main hosts page can be used to modify all entries in the hosts file. The main hosts page is located in the "Management" section of the "server" homepage, as shown in **Figure 1**.

![Figure 1](Editing%20Host%20Entries_home-icon.png)

_Figure 1_

## Manage Local Hosts Page

**Figure 2** highlights outlines the user interface elements of the host entries page.

![Figure 2](Editing%20Host%20Entries_hosts-home-elements.png)

1. **Host entries** - Displays the hostname, address and comment for each entry in the hosts file. The _comment_ column represents commented text at the end of the line. Valid keyboard shortcuts for this list are **F5** (refresh), **DEL** (delete) and **ENTER** (edit)
1. **Filter** - Filters, in real-time, the enties displayed by hostname, address or comment.
1. **Show All** - Clears the current filter and displays all entries
1. **Alerts** - Displays warnings when there are problems with the host file or usage tips when there are no problems.
1. **Add** - Displays the host entry details dialog to add a new entry to the hosts file
1. **Edit** - Displays the host entry details dialog to edit the selected entry (or entries). This option is only displayed when one or more entries are selected.
1. **Delete** - Deletes the selected host entries. This options is only displayed when one or more entries are selected.
1. **Open in notepad** - Opens the hosts file in an elevated notepad instance for manual editing. The entries list will not refresh automatically if the file is changed in notepad.
1. **Enable/Disable** - Enables or disables the selected entries. Entries are disabled by commenting out the line in the hosts file (prefixing it with a #). These options are only displayed they are valid for at least one of the selected entries
1. **Switch Address** - See [Host Switching](Host%20Switching.md)

### Edit Host Entry Dialog

**Figure 3** highlights outlines the user interface elements of the Edit Host Entry dialog.

![Figure 3](Editing%20Host%20Entries_hosts-edit-elements.png)

_Figure 3_

1. **Address** - The address for the host entry. The local IP addresses for the connected server will be displayed in the drop down list, though any value can be entered
1. **Hostname** - The hostname for the entry.
1. **Comment** - The comment, if any, to be associated with the entry
1. **Enabled** - If unchecked, the entry will be commented out using a # prefix

### Editing Multiple Entries

**Figure 4** shows the Edit Host Entry dialog when multiple entries are selected.

![Figure 4](Editing%20Host%20Entries_hosts-edit-multiple.png)

Fields that are shared across the selected entries will contain their shared value, whereas fields with differing values will be displayed as `<multiple>` (the enabled checkbox will be displayed as indeterminate, as in Figure 4). All fields, including those with differing values, can be modified and the changes will be reflected in each of the selected entries. Leaving a "multiple" field with it's initial value will leave that field unmodified in the selected entries.

If the intention is to change the address of multiple host entries, it is recommended that the [Host Switching](Host%20Switching.md) feature be used instead.
