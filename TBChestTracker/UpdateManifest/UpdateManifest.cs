using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TBChestTracker
{
    [System.Serializable]
    public class UpdateManifest
    {
        public string Hash { get; set; }
        public string Name
        {
            get => "Total Battle Chest Tracker";
        }
        public string Version
        {
            get => $@"v{Assembly.GetExecutingAssembly().GetName().Version.ToString()}";
        }
    }
}