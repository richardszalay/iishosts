using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Management.Server;

namespace RichardSzalay.HostsFileExtension.Messages
{
    public abstract class ServiceMessage
    {
        public const int ServiceVersion = 2;

        public ServiceMessage()
        {
        }

        public ServiceMessage(PropertyBag propertyBag)
        {
            int messageServiceVersion = (int)propertyBag[0];

            if (messageServiceVersion != ServiceVersion)
            {
                throw new HostsFileServiceException("Cannot manage remote hosts as the extension version is not compatible with the version installed locally.");
            }

            bool success = (bool)propertyBag[1];

            if (!success)
            {
                string errorMessage = (string)propertyBag[2];

                throw new HostsFileServiceException(errorMessage);
            }

            LoadMessage((PropertyBag)propertyBag[2]);
        }

        protected abstract void LoadMessage(PropertyBag bag);

        public PropertyBag ToPropertyBag()
        {
            PropertyBag bag = new PropertyBag();

            bag[0] = ServiceVersion;
            bag[1] = true;
            bag[2] = CreateMessagePropertyBag();

            return bag;
        }

        protected abstract PropertyBag CreateMessagePropertyBag();

        public static PropertyBag CreateError(string message)
        {
            PropertyBag bag = new PropertyBag();
            bag[0] = ServiceVersion;
            bag[1] = false;
            bag[2] = message;

            return bag;
        }
    }
}
