using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Server;

namespace RichardSzalay.HostsFileExtension.Messages
{
    public abstract class ServiceMessage
    {
        public ServiceMessage()
        {
        }

        public ServiceMessage(PropertyBag propertyBag)
        {
            bool success = (bool)propertyBag[0];

            if (!success)
            {
                string errorMessage = (string)propertyBag[1];

                throw new HostsFileServiceException(errorMessage);
            }

            LoadMessage((PropertyBag)propertyBag[1]);
        }

        protected abstract void LoadMessage(PropertyBag bag);

        public PropertyBag ToPropertyBag()
        {
            PropertyBag bag = new PropertyBag();

            bag[0] = true;
            bag[1] = CreateMessagePropertyBag();

            return bag;
        }

        protected abstract PropertyBag CreateMessagePropertyBag();

        public static PropertyBag CreateError(string message)
        {
            PropertyBag bag = new PropertyBag();
            bag[0] = false;
            bag[1] = message;

            return bag;
        }
    }
}
