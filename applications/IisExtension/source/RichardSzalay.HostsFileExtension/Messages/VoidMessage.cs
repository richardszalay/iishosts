using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Server;

namespace RichardSzalay.HostsFileExtension.Messages
{
    public class VoidMessage : ServiceMessage
    {
        public VoidMessage()
            : base()
        {
        }

        public VoidMessage(PropertyBag bag)
            : base(bag)
        {
        }

        protected override void LoadMessage(PropertyBag bag)
        {
        }

        protected override PropertyBag CreateMessagePropertyBag()
        {
            return new PropertyBag();
        }
    }
}
