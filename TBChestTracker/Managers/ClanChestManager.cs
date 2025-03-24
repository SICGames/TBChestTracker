using com.HellStormGames.Diagnostics;

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

namespace TBChestTracker
{
    [System.Serializable]
    public class ClanChestManager : IDisposable
    {
        public ChestProcessor ChestProcessor { get; private set; }
        public string DateFormat => AppContext.Instance.ForcedDateFormat;

        public ClanChestManager()
        {
            ChestProcessor = new ChestProcessor();
        }

        public ChestProcessor GetChestProcessor() { return ChestProcessor; }

        public async Task WriteChests(string filename, List<string> result, ChestAutomation chestAutomation)
        {
            if(String.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException(nameof(filename));
            }

            if (chestAutomation == null)
            {
                throw new ArgumentNullException(nameof(chestAutomation));
            }

            bool r = ChestProcessor.Write(filename, result.ToArray());
        }

        public void ProcessChestsAsRaw(List<string> result, ChestAutomation chestautomation)
        {
            if (chestautomation == null)
            {
                throw new ArgumentNullException(nameof(chestautomation));
            }

            var buildClanChestsAfterStoppingAutomation = SettingsManager.Instance.Settings.AutomationSettings.BuildChestsAfterStoppingAutomation;
            if(buildClanChestsAfterStoppingAutomation)
            {
                //-- automatic build
                ChestProcessor.ProcessToTempFile(result, chestautomation);
            }
            else
            {
                //-- manual build
                ChestProcessor.ProcessToFile(result, chestautomation);
            }
        }

        public CommonResult Load(string filename = "")
        {
            try
            {
                //var rootFolder = $"{SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder}";
                var clanFolder = $"{ClanManager.Instance.CurrentProjectDirectory}";
                var databaseFolder = $"{clanFolder}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanDatabaseFolder}";
                // var clanchestfile = $"{clanFolder}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanChestDatabaseFile}";
                //-- need to seperate following variables somewhere else.
                var chestsettingsfile = $"{clanFolder}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanSettingsFile}";
                //-- load clan chest settings
                if (!System.IO.File.Exists(chestsettingsfile))
                {
                    if (ClanManager.Instance.ClanChestSettings.ChestRequirements == null)
                    {
                        ClanManager.Instance.ClanChestSettings.ChestRequirements = new ChestRequirements();
                        ClanManager.Instance.ClanChestSettings.InitSettings();
                    }
                    ClanManager.Instance.ClanChestSettings.SaveSettings(chestsettingsfile);
                }
                else
                {
                    if (ClanManager.Instance.ClanChestSettings.ChestRequirements == null)
                        ClanManager.Instance.ClanChestSettings.ChestRequirements = new ChestRequirements();

                    if (!ClanManager.Instance.ClanChestSettings.LoadSettings(chestsettingsfile))
                    {
                        //-- problem loading clan chest settings.
                        Loggio.Warn("Load Issue", "Unable to load Clan Chest Settings.");
                    }
                }
                return new CommonResult(CommonResultCodes.Success, "Everything went peachy.");
            }
            catch (Exception ex)
            {
                Loggio.Error(ex, "Load File", "Something really bad happened. An exception was caught. See Log file for more information.");
                return new CommonResult(CommonResult.Error, $"Oops! Something really bad happened. An exception was caught. Reason for this error: {ex.Message}");
            }
        }
        #region BuildData()
        public ChestDataBuildResult BuildData()
        {
            
            var result = Load();
            var dateformat = DateFormat;
            var currentCulture = CultureInfo.CurrentCulture; //-- en-GB

            if (result.Code != CommonResultCodes.Success)
            {
                MessageBox.Show($"{result.Message}", "Building Clan Data Failed");
                return ChestDataBuildResult.LOAD_FAIL;
            }
            var build_result = ChestProcessor.Init();
            return build_result;
        }
        #endregion
        public void Dispose()
        {
            ChestProcessor.Dispose();
            ChestProcessor = null;
        }
    }

    #region ClanChestDataComparator
    public class ClanChestDataComparator : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            if (x == 0 || y == 0)
                return 0;

            return x.CompareTo(y);  
        }
    }
    #endregion
}
