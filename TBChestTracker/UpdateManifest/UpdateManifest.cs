using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TBChestTracker
{
    [System.Serializable]
    public class UpdateManifest
    {
        public string Hash { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public string AssetUrl { get; set; }
        public DateTime DateCreated { get; set; }
    }
}