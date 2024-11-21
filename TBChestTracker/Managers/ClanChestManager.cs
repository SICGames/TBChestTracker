using com.HellStormGames.Diagnosis;
using com.HellStormGames.Logging;
using Emgu.CV.CvEnum;
using Microsoft.Win32;
using MS.WindowsAPICodePack.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using TBChestTracker.Automation;
using TBChestTracker.Managers;

using static Emgu.CV.Features2D.ORB;

namespace TBChestTracker.Obsolete
{

    /*
     Revising ClanChestManager - 9/4/2024
     Total Battle Chest Tracker 2.0
     Goal is to improve speed. 
     Similar to how a game gives resources, this should act the same.
     Resource.Give("Bob",ResourceType.Gold, 100);
     ClanChestManager.Give("Hellraiser",new ClanChest("Epic","Harpy Chest", 35));
     Ideally, this will remove the need to create unnecessary crap in memory and hold up processing time.
     This needs to be entirely reworked from ground up. 
    */

    [System.Serializable]
    public class ClanChestManager : IDisposable
    {
        public ClanChestDatabase Database { get; set; } //-- new updated version.

        public ClanChestManager()
        {
            Database = new ClanChestDatabase();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
    public class ClanChestDataComparator : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            if (x == 0 || y == 0)
                return 0;

            return x.CompareTo(y);  
        }
    }
}
