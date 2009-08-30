using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RichardSzalay.HostsFileExtension
{
    internal interface IResource
    {
        Stream OpenRead();
        Stream OpenWrite();
    }
}
