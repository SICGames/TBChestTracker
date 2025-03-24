using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    [System.Serializable]
    public class TessDataManifest
    {
        private bool? _requiresReinstall;
        public bool RequiresReinstall
        {
            get => _requiresReinstall.GetValueOrDefault(true);
            set => _requiresReinstall = value;
        }


    }
}
