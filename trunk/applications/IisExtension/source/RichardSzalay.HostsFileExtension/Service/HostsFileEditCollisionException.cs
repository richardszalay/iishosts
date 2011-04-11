using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RichardSzalay.HostsFileExtension.Properties;

namespace RichardSzalay.HostsFileExtension.Service
{
    public class HostsFileEditCollisionException : ApplicationException
    {
        public HostsFileEditCollisionException()
            : base(Resources.HostFileEditCollisionException)
        {
        }
    }
}
