using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichardSzalay.HostsFileExtension.Messages
{
    public class HostsFileServiceException : Exception
    {
        public HostsFileServiceException(string message)
            : base(message)
        {
        }
    }
}
