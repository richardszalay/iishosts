Hosts File Manager allows local host entries (in %windir%\system32\drivers\etc) to be edited from within IIS Manager. In addition to providing a GUI, IIS Manager can also provide integration into individual websites on the local machine. IIS Manager is also auto-elevated in Windows 7, making the process even smoother.

For PowerShell users, try [richardszalay/pshosts](https://github.com/richardszalay/pshosts)

![IIS Hosts](docs/Home_iishostsfile-r1.png?raw=true)

## Features

* Does not modify the formatting of existing entries (even when making changes)
* Supports enabling/disabling entries (by commenting them out)
* Integration into IIS 7+ (still supported in Windows 10)
* Inherit's IIS 7 auto-elevation in Windows 7

## Installation

Simply [download the installer for the latest release](https://github.com/richardszalay/iishosts/releases/latest) and run it.

## Release Notes

### 1.0.2
* Added Chocolatey package
* Updated links to refer to github

### 1.0.1

* [Bulk editing](docs/Editing%20Host%20Entries.md)
* [Address switching](docs/Host%20Switching.md) (also [across site bindings](docs/Editing%20Site%20Binding%20Host%20Entries.md))
* [Documentation](docs/Documentation.md)
* Remote management
* Significant installer improvements

### Release 1

* Initial release with installer
* Supports create/edit/delete and enable/disable
* Address field is automatically populated with local IP addresses
* Local connections only

**Warning: The installer for Release 1 does not check for IIS7 but will not fail if it is not found**

**Warning: Do not install Release 1 x86 on Windows Vista/7 x64. The installation will fail, but appear to be successful. Uninstall and download the correct version**

## Roadmap

### 1.5

* Host entry groupings (including by site)
* Host entry re-ordering
* Clean up options (remove disabled, remove unused)
