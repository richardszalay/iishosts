using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichardSzalay.HostsFileExtension
{
    public interface IAddressProvider
    {
        string[] GetAddresses();
    }
}
