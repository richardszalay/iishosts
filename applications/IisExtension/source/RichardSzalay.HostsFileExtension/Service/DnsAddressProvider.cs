using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace RichardSzalay.HostsFileExtension.Service
{
    public class DnsAddressProvider : IAddressProvider
    {
        public string[] GetAddresses()
        {
            List<string> addresses = new List<string>();

            addresses.Add(LoopbackAddress);

            string hostname = Dns.GetHostName();

            IPHostEntry host = Dns.GetHostEntry(hostname);

            addresses.AddRange(
                host.AddressList.Select(c => c.ToString())
                );

            return addresses.ToArray();
        }

        private const string LoopbackAddress = "127.0.0.1";
    }
}
