using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichardSzalay.HostsFileExtension.Setup
{
    public static class IisInstallUtil
    {
        /// <summary> 
        /// Registers a new Module in the Modules section inside ApplicationHost.config 
        /// </summary> 
        public static void AddModule(string name, string type)
        {
            using (ServerManager mgr = new ServerManager())
            {
                Configuration appHostConfig = mgr.GetApplicationHostConfiguration();
                ConfigurationSection modulesSection = appHostConfig.GetSection("system.webServer/modules");
                ConfigurationElementCollection modules = modulesSection.GetCollection();

                if (FindByAttribute(modules, "name", name) == null)
                {
                    ConfigurationElement module = modules.CreateElement();
                    module.SetAttributeValue("name", name);
                    if (!String.IsNullOrEmpty(type))
                    {
                        module.SetAttributeValue("type", type);
                    }

                    modules.Add(module);
                }

                mgr.CommitChanges();
            }
        }

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

                // Now register it so that all Sites have access to this module 
                ConfigurationSection modulesSection = adminConfig.GetSection("modules");
                ConfigurationElementCollection modules = modulesSection.GetCollection();
                if (FindByAttribute(modules, "name", name) == null)
                {
                    ConfigurationElement module = modules.CreateElement();
                    module.SetAttributeValue("name", name);
                    modules.Add(module);
                }

                mgr.CommitChanges();
            }
        }

        /// <summary> 
        /// Create a new Web Application 
        /// </summary> 
        public static void CreateApplication(string siteName, string virtualPath, string physicalPath)
        {
            using (ServerManager mgr = new ServerManager())
            {
                Site site = mgr.Sites[siteName];
                if (site != null)
                {
                    site.Applications.Add(virtualPath, physicalPath);
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

        public static void RemoveApplication(string siteName, string virtualPath)
        {
            using (ServerManager mgr = new ServerManager())
            {
                Site site = mgr.Sites[siteName];
                if (site != null)
                {
                    Application app = site.Applications[virtualPath];
                    if (app != null)
                    {
                        site.Applications.Remove(app);
                        mgr.CommitChanges();
                    }
                }
            }
        }

        /// <summary> 
        /// Removes the specified module from the Modules section by name 
        /// </summary> 
        public static void RemoveModule(string name)
        {
            using (ServerManager mgr = new ServerManager())
            {
                Configuration appHostConfig = mgr.GetApplicationHostConfiguration();
                ConfigurationSection modulesSection = appHostConfig.GetSection("system.webServer/modules");
                ConfigurationElementCollection modules = modulesSection.GetCollection();
                ConfigurationElement module = FindByAttribute(modules, "name", name);
                if (module != null)
                {
                    modules.Remove(module);
                }

                mgr.CommitChanges();
            }
        }


        /// <summary> 
        /// Removes the specified UI Module by name 
        /// </summary> 
        public static void RemoveUIModuleProvider(string name)
        {
            using (ServerManager mgr = new ServerManager())
            {
                // First remove it from the sites 
                Configuration adminConfig = mgr.GetAdministrationConfiguration();
                ConfigurationSection modulesSection = adminConfig.GetSection("modules");
                ConfigurationElementCollection modules = modulesSection.GetCollection();
                ConfigurationElement module = FindByAttribute(modules, "name", name);
                if (module != null)
                {
                    modules.Remove(module);
                }

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
