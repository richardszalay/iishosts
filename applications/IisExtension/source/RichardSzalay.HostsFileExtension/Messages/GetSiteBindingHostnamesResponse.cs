using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Server;

namespace RichardSzalay.HostsFileExtension.Messages
{
    public class GetSiteBindingHostnamesResponse : ServiceMessage
    {
        private SiteBinding[] bindings;

        public SiteBinding[] Bindings
        {
            get { return bindings; }
        }

        public GetSiteBindingHostnamesResponse(SiteBinding[] bindings)
            : base()
        {
            this.bindings = bindings;
        }

        public GetSiteBindingHostnamesResponse(PropertyBag bag)
            : base(bag)
        {
        }

        protected override void LoadMessage(PropertyBag bag)
        {
            int count = (int)bag[0];

            bindings = new SiteBinding[count];

            for (int i = 0; i < count; i++)
            {
                bindings[i] = new SiteBinding((PropertyBag)bag[i + 1]);
            }
        }

        protected override PropertyBag CreateMessagePropertyBag()
        {
            PropertyBag bag = new PropertyBag();

            bag[0] = this.bindings.Length;

            for (int i = 0; i < bindings.Length; i++)
            {
                bag[i + 1] = bindings[i].ToPropertyBag();
            }

            return bag;
        }
    }
}
