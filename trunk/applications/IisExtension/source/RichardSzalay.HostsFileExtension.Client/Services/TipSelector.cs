using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;
using RichardSzalay.HostsFileExtension.Client.Properties;

namespace RichardSzalay.HostsFileExtension.Client.Services
{
    public class TipSelector
    {
        private static readonly string[] TipResourceNames = new[]
            {
                "TipEditMultipleItems",
                "TipSwitchAddress",
                "TipSiteBindingEntries"
            };

        private Random random = new Random();
        private ResourceManager resourceManager = Resources.ResourceManager;

        public TipSelector()
        {
        }

        public string SelectTip()
        {
            int randomTipIndex = random.Next(0, TipResourceNames.Length);

            return resourceManager.GetString(TipResourceNames[randomTipIndex]);
        }
    }
}
