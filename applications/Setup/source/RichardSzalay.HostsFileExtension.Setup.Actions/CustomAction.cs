using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;
using System.Reflection;

namespace RichardSzalay.HostsFileExtension.Setup.Actions
{
    public class CustomActions
    {
        private const string UIModuleKey = "ManageHosts";
        private const string ExtensionQualifiedName = "RichardSzalay.HostsFileExtension.Registration.ManageHostsModuleUIProvider, RichardSzalay.HostsFileExtension, Version=1.0.0.0, Culture=neutral, PublicKeyToken=20cb46ecc35c2f94, processorArchitecture=MSIL";

        [CustomAction]
        public static ActionResult InstallUIModule(Session session)
        {
            session.Log("Installing UIModuleProvider {0} as {1}", ExtensionQualifiedName, UIModuleKey);

            IisInstallUtil.AddUIModuleProvider(UIModuleKey, ExtensionQualifiedName);

            session.Log("Installing UIModuleProvider complete");

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult UninstallUIModule(Session session)
        {
            try
            {
                IisInstallUtil.RemoveUIModuleProvider(UIModuleKey);
            }
            catch (Exception ex)
            {
                session.Log("UI Module removal failed: {0}", ex.Message);

                return ActionResult.Failure;
            }

            return ActionResult.Success;
        }
    }
}
