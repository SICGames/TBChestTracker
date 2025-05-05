﻿using System;
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
        public static string Build = "";
        public static string Tag = "v2.4";
    }
}