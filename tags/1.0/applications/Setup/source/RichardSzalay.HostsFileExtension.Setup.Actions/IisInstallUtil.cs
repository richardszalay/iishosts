using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Administration;
using Microsoft.Web.Management.Client;

namespace RichardSzalay.HostsFileExtension.Setup.Actions
{
    public static class IisInstallUtil
    {
        public static void AddUIModuleProvider(string name, string type)
        {
            using (ServerManager mgr = new ServerManager())
            {
                // First register the Module Provider  
                Configuration adminConfig = mgr.GetAdministrationConfiguration();

                ConfigurationSection moduleProvidersSection = adminConfig.GetSection("moduleProviders");
                ConfigurationElementCollection moduleProviders = moduleProvidersSection.GetCollection();
                if (FindByAttribute(moduleProviders, "name", name) == null)
                {
                    ConfigurationElement moduleProvider = moduleProviders.CreateElement();
                    moduleProvider.SetAttributeValue("name", name);
                    moduleProvider.SetAttributeValue("type", type);
                    moduleProviders.Add(moduleProvider);
                }

                mgr.CommitChanges();
            }
        }

        /// <summary> 
        /// Helper method to find an element based on an attribute 
        /// </summary> 
        private static ConfigurationElement FindByAttribute(ConfigurationElementCollection collection, string attributeName, string value)
        {
            foreach (ConfigurationElement element in collection)
            {
                if (String.Equals((string)element.GetAttribute(attributeName).Value, value, StringComparison.OrdinalIgnoreCase))
                {
                    return element;
                }
            }

            return null;
        }

        /// <summary> 
        /// Removes the specified UI Module by name 
        /// </summary> 
        public static void RemoveUIModuleProvider(string name)
        {
            using (ServerManager mgr = new ServerManager())
            {
                Configuration adminConfig = mgr.GetAdministrationConfiguration();

                // now remove the ModuleProvider 
                ConfigurationSection moduleProvidersSection = adminConfig.GetSection("moduleProviders");
                ConfigurationElementCollection moduleProviders = moduleProvidersSection.GetCollection();
                ConfigurationElement moduleProvider = FindByAttribute(moduleProviders, "name", name);
                if (moduleProvider != null)
                {
                    moduleProviders.Remove(moduleProvider);
                }

                mgr.CommitChanges();
            }
        }
    }
}
