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
    public class Manifest
    {
        public static string Name = "Total Battle Chest Tracker";
        public static string Version = $@"v{Assembly.GetExecutingAssembly().GetName().Version.ToString()}";
        public static string Build = "['Release Candidate 1.4']";
        public static string Tag = "v2.0-RC1.4";
    }
}