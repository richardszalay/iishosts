using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Server;

namespace RichardSzalay.HostsFileExtension.Messages
{
    public class GetServerAddressesResponse : ServiceMessage
    {
        private string[] addresses;

        public GetServerAddressesResponse(string[] addresses)
        {
            this.addresses = addresses;
        }

        public GetServerAddressesResponse(PropertyBag bag)
            : base(bag)
        {
        }

        protected override void LoadMessage(PropertyBag bag)
        {
            int count = (int)bag[0];
            addresses = new string[count];

            for (int i = 0; i < count; i++)
            {
                addresses[i] = (string)bag[i + 1];
            }
        }

        protected override PropertyBag CreateMessagePropertyBag()
        {
            var bag = new PropertyBag();
            bag[0] = addresses.Length;

            for (int i = 0; i < addresses.Length; i++)
            {
                bag[i + 1] = addresses[i];
            }

            return bag;
        }

        public string[] Addresses
        {
            get { return addresses; }
        }
    }
}
