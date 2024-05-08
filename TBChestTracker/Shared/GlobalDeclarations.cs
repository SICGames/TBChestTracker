using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class GlobalDeclarations
    {
        public static bool hasNewClanDatabaseCreated {  get; set; }
        public static bool hasClanmatesBeenAdded { get; set; }
        public static bool hasHotkeyBeenPressed { get; set; }
        public static bool isAnyGiftsAvailable { get; set; }
        public static bool isBusyProcessingClanchests { get; set; }
        public static bool canCaptureAgain { get; set; }
        public static string AppFolder
        {
            get => $@"{System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\";
        }
        public static string CommonAppFolder
        {
            get => $@"{System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\SICGames\TotalBattleChestTracker\";
        }
        public static string RecentOpenedClanDatabases
        {
            get => $@"{CommonAppFolder}recent.db";
        }

        public static string TesseractData
        {
            get => $@"{AppFolder}TessData";
        }
        public static bool TessDataExists { get; set; }
        public static bool AutomationRunning = false;

        
        public static bool IsConfiguringHotKeys = false;

        public static bool DebugOCRWizardEnabled = false;
    }
}
