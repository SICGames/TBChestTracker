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

namespace TBChestTracker
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
        public ChestProcessor ChestProcessor { get; private set; }
        public string DateFormat => AppContext.Instance.ForcedDateFormat;

        public ClanChestManager()
        {
            Database = new ClanChestDatabase();
            ChestProcessor = new ChestProcessor();
        }

        public IList<ClanChestData> ClanChestData
        {
            get
            {
                if (ChestProcessor.GetClanChestData() == null)
                {
                    throw new ArgumentNullException("Temp ClanChestData is null.");
                }

                return ChestProcessor.GetClanChestData();
            }
        }
        public ChestProcessingState ChestProcessingState
        {
            get
            {
                if(ChestProcessor == null)
                {
                    return ChestProcessingState.NO_PROCESSOR_ATTACHED;
                }

                return ChestProcessor.ChestProcessingState;
            }
        }

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

        public async Task ClearCache()
        {
            var db = ClanManager.Instance.ClanDatabaseManager.ClanDatabase;

            var clanfolder = ClanManager.Instance.CurrentProjectDirectory;
            var archiveFolder = $"{clanfolder}\\archives";
            var cacheFolder = $"{clanfolder}\\cache";
            DirectoryInfo di = new DirectoryInfo(cacheFolder);
            if (di.Exists)
            {
                var files = di.GetFiles("*.txt");

                DirectoryInfo archiveDI = new DirectoryInfo(archiveFolder);
                if(archiveDI.Exists == false)
                {
                    archiveDI.Create();
                }

                //-- archive them then delete 
                foreach (var file in files)
                {
                    var destFilename = $"{archiveFolder}\\{file.Name}";
                    File.Copy(file.FullName, destFilename, true);
                    file.Delete();
                }
            }
        }

        public async Task BuildChests(string[] files, IProgress<BuildingChestsProgress> progress)
        {
            if (files == null || files.Length == 0) 
            { 
                throw new ArgumentNullException(nameof(files)); 
            }
            await ChestProcessor.Build(files, progress, Database);
        }

        public async Task ProcessChests(List<string> result, ChestAutomation chestAutomation)
        {
            if (chestAutomation == null)
            {
                throw new ArgumentNullException(nameof(chestAutomation));
            }
            
           await ChestProcessor.Process(result, chestAutomation, Database);
        }
        public async Task ProcessChestsAsRaw(List<string> result, ChestAutomation chestautomation)
        {
            if (chestautomation == null)
            {
                throw new ArgumentNullException(nameof(chestautomation));
            }
            await ChestProcessor.ProcessToCache(result, chestautomation);
        }
        public bool Load(string filename = "")
        {
            try
            {
                //var rootFolder = $"{SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder}";
                var clanFolder = $"{ClanManager.Instance.CurrentProjectDirectory}";
                var databaseFolder = $"{clanFolder}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanDatabaseFolder}";

                var clanchestfile = $"{clanFolder}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanChestDatabaseFile}";
                
                //-- need to seperate following variables somewhere else.
                var clanmatefile = $"{clanFolder}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanmateDatabaseFile}";
                var chestsettingsfile = $"{clanFolder}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanSettingsFile}";
                var chestrewardsFile = $@"{databaseFolder}\ChestRewards.db";

                bool clanchestcorrupted = false;

                AppContext.Instance.IsClanChestDataCorrupted = false;

                if (ClanManager.Instance.ClanmateManager.Database.Clanmates != null)
                    ClanManager.Instance.ClanmateManager.Database.Clanmates.Clear();

                if (!System.IO.File.Exists(clanmatefile))
                {
                    AppContext.Instance.ClanmatesBeenAdded = false;
                    return false;
                }
                else
                {
                    if (!ClanManager.Instance.ClanmateManager.Load(clanmatefile))
                    {
                        //-- there's a problem loading clanmate file.
                        com.HellStormGames.Logging.Console.Write($@"Failed to load clanmate database file.", "Load Issue", LogType.ERROR);
                    }
                }

                if (String.IsNullOrEmpty(filename))
                {
                    filename = clanchestfile;
                }

                if (!System.IO.File.Exists(filename))
                {
                    Database.NewEntry(DateTime.Now.ToString(AppContext.Instance.ForcedDateFormat));
                    //ClanChestDailyData.Add(DateTime.Now.ToString(AppContext.Instance.ForcedDateFormat, new CultureInfo(CultureInfo.CurrentCulture.Name)), clanChestData);
                    Save();
                    //await SaveDataTask();
                }
                else
                {
                    ClanChestDatabase database = new ClanChestDatabase();
                    bool result = JsonHelper.TryLoad<ClanChestDatabase>(filename, out database);

                    if (database != null)
                    {
                        Database = database;
                    }
                    
                    if (result == false)
                    {
                        //-- we need to let them know there's a possible corrupt clanchestdatabase file
                        //-- we caught the exception. User needs to restore. Need a way to force them. 
                        AppContext.Instance.IsClanChestDataCorrupted = true;
                        clanchestcorrupted = true;
                        com.HellStormGames.Logging.Console.Write($@"Clan Chest Database possibly corrupted.", "Clan Chest Database Error", LogType.ERROR);
                    }
                    database = null;
                }

                if(clanchestcorrupted)
                {
                    return false;
                }

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
                        com.HellStormGames.Logging.Console.Write($@"Failed to load clan settings file.", "Load Issue", LogType.ERROR);
                    }
                }

                if (System.IO.File.Exists(chestrewardsFile))
                {
                    if (ChestProcessor.ChestRewards.Load() == false)
                    {
                        //-- problem loading chest rewards 
                        com.HellStormGames.Logging.Console.Write($@"Failed to load chest rewards file.", "Load Issue", LogType.ERROR);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool Save(string filename = "")
        {
            try
            {
                //-- write to file.
                string file = String.Empty;
                var root = $"{SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder}";
                var clanfolder = $"{ClanManager.Instance.CurrentProjectDirectory}";
                var databaseFolder = $"{clanfolder}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanDatabaseFolder}\\";

                if (String.IsNullOrEmpty(filename))
                    file = $"{clanfolder}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanChestDatabaseFile}";
                else
                    file = $"{databaseFolder}{filename}";


                using (StreamWriter sw = File.CreateText(file))
                {
                    JsonSerializer jsonSerializer = new JsonSerializer();
                    jsonSerializer.Formatting = Formatting.Indented;
                    jsonSerializer.Serialize(sw, Database);
                    sw.Close();
                }

                if (ChestProcessor.ChestRewards.ChestRewardsList.Count > 0)
                {
                    ChestProcessor.ChestRewards.Save();
                }

            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public bool SaveBackup(string filename)
        {
            if (String.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException(nameof(filename));
            }
            try
            {
                using (StreamWriter sw = File.CreateText(filename))
                {
                    JsonSerializer jsonSerializer = new JsonSerializer();
                    jsonSerializer.Formatting = Formatting.Indented;
                    jsonSerializer.Serialize(sw, Database);
                    sw.Close();
                }
            }
            catch (Exception ex) 
            { 
                return false; 
            }

            return true;
        }
        public bool CreateBackup(string filename = "")
        {
            try
            {
                //var root = $"{SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder}";
                var clanfolder = $"{ClanManager.Instance.CurrentProjectDirectory}";

                var clanchestsBackupFolder = $"{clanfolder}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanDatabaseBackupFolderPath}\\Clanchests";
                var di = new DirectoryInfo(clanchestsBackupFolder);
                if (di.Exists == false)
                {
                    di.Create();
                }

                DateTime dateTimeOffset = DateTime.Now;
                string file = $"{clanchestsBackupFolder}\\clanchest_backup_{dateTimeOffset.ConvertToUnixTimeStamp()}.db";

                if (SaveBackup(file))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool LoadBackup(string filename)
        {
            try
            {
                Database.RemoveAllEntries();
                if (Load(filename))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #region FilterClanChestByConditions
        public Dictionary<string, IList<ClanChestData>> FilterClanChestByConditions()
        {
            var dailyChests = new Dictionary<string, IList<ClanChestData>>();
            var chestRequirements = ClanManager.Instance.ClanChestSettings.ChestRequirements;
            foreach (var dailyChestData in Database.ClanChestData.ToList())
            {
                var date = dailyChestData.Key;
                var dailychest = Database.ClanChestData[date];
                var newdailychest = new List<ClanChestData>();
                if (dailychest != null)
                {
                    foreach (var chestdata in dailychest)
                    {
                        var chests = chestdata.chests;
                        var newChests = new List<Chest>();

                        if (chests != null)
                        {
                            var chestConditions = chestRequirements.ChestConditions;

                            foreach (var chest in chests)
                            {
                                foreach (var condition in chestConditions)
                                {

                                    if (chest.Type.ToLower().Contains(condition.ChestType.ToLower()))
                                    {
                                        if (condition.ChestName.Equals("(Any)") == true)
                                        {
                                            if (condition.level.Equals("(Any)") == false)
                                            {
                                                var level = int.Parse(condition.level);
                                                if (level >= chest.Level)
                                                {
                                                    var c = new Chest(chest.Name, chest.Type, chest.Source, chest.Level);
                                                    newChests.Add(c);
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                var c = new Chest(chest.Name, chest.Type, chest.Source, chest.Level);
                                                newChests.Add(c);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (chest.Name.ToLower().Equals(condition.ChestName.ToLower()))
                                            {
                                                if (condition.level.Equals("(Any)") == false)
                                                {
                                                    var level = int.Parse(condition.level);
                                                    if (chest.Level >= level)
                                                    {
                                                        var c = new Chest(chest.Name, chest.Type, chest.Source, chest.Level);
                                                        newChests.Add(c);
                                                        break;
                                                    }
                                                   
                                                }
                                                else
                                                {
                                                    var c = new Chest(chest.Name, chest.Type, chest.Source, chest.Level);
                                                    newChests.Add(c);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        var ccd = new ClanChestData();
                        ccd.Clanmate = chestdata.Clanmate;
                        ccd.chests = newChests;
                        newdailychest.Add(ccd);
                    }
                }
                dailyChests.Add(date, newdailychest);
            }

            return dailyChests;
        }
        #endregion

        #region CheckIntegrity
        //-- RepairChestData returns false if nothing needs to be done. Returns true if errors exist.
        public IntegrityResult CheckIntegrity()
        {
            int chestErrors = 0;
            IntegrityResult result = new IntegrityResult();

            var chestsettings = ClanManager.Instance.ClanChestSettings;
            var chestdata = ClanManager.Instance.ClanChestManager.Database.ClanChestData; //ClanChestDailyData;

            var chestpointsvalues = ClanManager.Instance.ClanChestSettings.ChestPointsSettings.ChestPoints;
            if (chestpointsvalues == null || chestpointsvalues.Count == 0)
            {

            }
            var currentCulture = CultureInfo.CurrentCulture; //-- en-GB
            var uiCulture = CultureInfo.CurrentUICulture; //-- en-US
            /*
            bool invalidateDates = DoesDatesNeedRepair();
            if (invalidateDates)
            {
                result.Add("Invalid Locale Date Format Detected", chestErrors);
                chestErrors++;
            }
            */

            foreach (var dates in chestdata.Keys.ToList())
            {
                var data = chestdata[dates];
                foreach (var _data in data.ToList())
                {
                    var clanmate_points = _data.Points;
                    var chests = _data.chests;
                    var total_chest_points = 0;

                    try
                    {
                        if (chests != null)
                        {
                            lock (chests)
                            {
                                foreach (var chest in chests.ToList())
                                {
                                    if (chest == null)
                                    {
                                        chests.Remove(chest);
                                        continue;
                                    }
                                    var name = chest.Name;
                                    var source = chest.Source;
                                    /*
                                      Fix Any Chests that start with lvl in it. 
                                    */
                                    if (source.Contains(TBChestTracker.Resources.Strings.lvl))
                                    {
                                        var levelStartPos = -1;
                                        levelStartPos = source.IndexOf(TBChestTracker.Resources.Strings.lvl);
                                        int level = 0;
                                        //-- level in en-US is 0 position.
                                        //-- level in es-ES is 11 position.
                                        //-- crypt in en-US is 9 position 
                                        //-- crypt in es-ES is 0 position.

                                        //-- we can check direction of level position.
                                        //-- if more than 1 then we know we should go backwares. If 0 then we know to go forwards.
                                        var levelFullLength = 0;
                                        var ChestType = String.Empty;

                                        if (levelStartPos > -1)
                                        {
                                            var levelStr = source.Substring(levelStartPos).ToLower();
                                            var levelNumberStr = levelStr.Substring(levelStr.IndexOf(" ") + 1);

                                            //-- using a quantifer to check if there is an additional space after the level number. If user is spanish, no space after level number.
                                            levelNumberStr = levelNumberStr.IndexOf(" ") > 0 ? levelNumberStr.Substring(0, levelNumberStr.IndexOf(" ")) : levelNumberStr;
                                            var levelFullStr = String.Empty;

                                            if (source.Contains(TBChestTracker.Resources.Strings.Level))
                                            {
                                                levelFullStr = $"{TBChestTracker.Resources.Strings.Level} {levelNumberStr}";
                                            }
                                            else if (source.Contains(TBChestTracker.Resources.Strings.lvl))
                                            {
                                                levelFullStr = $"{TBChestTracker.Resources.Strings.lvl} {levelNumberStr}";
                                            }

                                            levelFullLength = levelFullStr.Length; //-- 'level|nivel 10' should equal to 8 characters in length.

                                            var levelArray = levelNumberStr.Split('-');

                                            if (levelArray.Count() == 1)
                                            {
                                                if (Int32.TryParse(levelArray[0], out level) == false)
                                                {
                                                    //-- couldn't extract level.
                                                }
                                            }
                                            else if (levelArray.Count() > 1)
                                            {
                                                if (Int32.TryParse(levelArray[0], out level) == false)
                                                {
                                                    //-- couldn't extract level.
                                                }
                                            }

                                            //-- now we make sure levelStartPos == 0 or more than 0.
                                            var direction = levelStartPos == 0 ? "forwards" : "backwards";
                                            if (direction == "forwards")
                                            {
                                                ChestType = source.Substring(levelFullLength + 1);
                                            }
                                            else
                                            {
                                                //-- Cripta de nivel 10 = 18 characters long.
                                                //-- Cripta de = 10 characters long
                                                //-- nivel 10 =  8 characters long.

                                                ChestType = source.Substring(0, source.Length - levelFullLength);
                                                ChestType = ChestType.Trim(); //-- remove any whitespaces at the end 
                                            }
                                            if (ChestType.StartsWith(TBChestTracker.Resources.Strings.OnlyCrypt))
                                            {
                                                ChestType = ChestType.Insert(0, $"{TBChestTracker.Resources.Strings.Common} ");
                                            }

                                            chest.Type = ChestType;
                                            if (chest.Level != level)
                                            {
                                                result.Add("Incorrect Chest levels", chestErrors);
                                                //chest.Level = level;
                                                Debug.WriteLine($"Repairing Chest {chest.Source} that contain lvl.");
                                                chestErrors++;
                                            }

                                        }
                                    }

                                    //-- these issues contain no level inside source.
                                    if (chest.Source.Contains(TBChestTracker.Resources.Strings.lvl) == false && chest.Source.Contains(TBChestTracker.Resources.Strings.Level) == false)
                                    {
                                        if (chest.Source.ToLower().Equals(chest.Type.ToLower()) == false)
                                        {
                                            if (chest.Source.ToLower().EndsWith(chest.Type.ToLower()))
                                            {
                                                //-- these errors generally have a level 5 whereas should be 0.
                                                if (chest.Level == 5)
                                                {
                                                    chest.Level = 0;
                                                }


                                                //chest.Type = chest.Source;
                                                result.Add("Incorrect Chest Types", chestErrors);

                                                Debug.WriteLine("Fixing Corrupted Chests.");
                                                chestErrors++;
                                            }
                                        }
                                    }
                                    if (chestsettings.GeneralClanSettings.ChestOptions == ChestOptions.UsePoints)
                                    {
                                        foreach (var chestpointvalue in chestpointsvalues)
                                        {
                                            var chestname = chest.Name;
                                            var chesttype = chest.Type;

                                            if (chesttype.ToLower().Contains(chestpointvalue.ChestType.ToLower()))
                                            {
                                                if (chestpointvalue.ChestName.Equals("(Any)"))
                                                {
                                                    if (!chestpointvalue.Level.Equals("(Any)"))
                                                    {
                                                        var chestlevel = Int32.Parse(chestpointvalue.Level);
                                                        if (chest.Level == chestlevel)
                                                        {
                                                            total_chest_points += chestpointvalue.PointValue;
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        total_chest_points += chestpointvalue.PointValue;
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    if (chestname.ToLower().Contains(chestpointvalue.ChestName.ToLower()))
                                                    {
                                                        if (!chestpointvalue.Level.Equals("(Any)"))
                                                        {
                                                            var chestlevel = Int32.Parse(chestpointvalue.Level);
                                                            if (chest.Level == chestlevel)
                                                            {
                                                                total_chest_points += chestpointvalue.PointValue;
                                                                break;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            total_chest_points += chestpointvalue.PointValue;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (clanmate_points != total_chest_points)
                            {
                                //_data.Points = total_chest_points;
                                //Debug.WriteLine("Chest Points don't add up.");
                                result.Add("Incorrect Chest Points", chestErrors);
                                chestErrors++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        com.HellStormGames.Logging.Console.Write("Exception caught while checking if chest data needs repair.", "Chest Integrity", LogType.INFO);
                        if (result != null)
                        {
                            result.Dispose();
                        }

                        return null;
                    }
                    // Search for clanmates no longer within clanmates db
                    var clanmate = _data.Clanmate;
                    var clanmates = ClanManager.Instance.ClanmateManager.Database.Clanmates;
                    bool bExists = clanmates.Select(c => c.Name).Contains(clanmate);
                    if (bExists == false)
                    {
                        //Debug.WriteLine($"{clanmate} doesn't exist anymore within clanmates database. Removing.");
                        //RemoveChestData(clanmate);
                        result.Add("Non-Existing Clanmates", chestErrors);

                        chestErrors++;
                    }
                }
            }

            if (chestErrors > 0)
            {
                return result;
            }

            return null;
        }
        #endregion

        #region Repair
        public bool Repair()
        {
            int chestErrors = 0;

            var chestsettings = ClanManager.Instance.ClanChestSettings;
            var chestdata = ClanManager.Instance.ClanChestManager.Database.ClanChestData; // ClanChestDailyData;
            var cultureUI = CultureInfo.CurrentUICulture;
            var currentCulture = CultureInfo.CurrentCulture;

            var chestpointsvalues = ClanManager.Instance.ClanChestSettings.ChestPointsSettings.ChestPoints;
            if (chestpointsvalues == null || chestpointsvalues.Count == 0)
            {

            }

            foreach (var dates in chestdata.Keys.ToList())
            {
                var data = chestdata[dates];
                foreach (var _data in data.ToList())
                {
                    var clanmate_points = _data.Points;
                    var chests = _data.chests;

                    var total_chest_points = 0;

                    if (chests != null)
                    {
                        foreach (var chest in chests.ToList())
                        {
                            if (chest == null)
                            {
                                chests.Remove(chest);
                                continue;
                            }
                            var name = chest.Name;
                            var source = chest.Source;
                            /*
                              Fix Any Chests that start with lvl in it. 
                            */
                            if (source.Contains(TBChestTracker.Resources.Strings.lvl))
                            {
                                var levelStartPos = -1;
                                levelStartPos = source.IndexOf(TBChestTracker.Resources.Strings.lvl);
                                int level = 0;
                                //-- level in en-US is 0 position.
                                //-- level in es-ES is 11 position.
                                //-- crypt in en-US is 9 position 
                                //-- crypt in es-ES is 0 position.

                                //-- we can check direction of level position.
                                //-- if more than 1 then we know we should go backwares. If 0 then we know to go forwards.
                                var levelFullLength = 0;
                                var ChestType = String.Empty;

                                if (levelStartPos > -1)
                                {
                                    var levelStr = source.Substring(levelStartPos).ToLower();
                                    var levelNumberStr = levelStr.Substring(levelStr.IndexOf(" ") + 1);

                                    //-- using a quantifer to check if there is an additional space after the level number. If user is spanish, no space after level number.
                                    levelNumberStr = levelNumberStr.IndexOf(" ") > 0 ? levelNumberStr.Substring(0, levelNumberStr.IndexOf(" ")) : levelNumberStr;
                                    var levelFullStr = String.Empty;

                                    if (source.Contains(TBChestTracker.Resources.Strings.Level))
                                    {
                                        levelFullStr = $"{TBChestTracker.Resources.Strings.Level} {levelNumberStr}";
                                    }
                                    else if (source.Contains(TBChestTracker.Resources.Strings.lvl))
                                    {
                                        levelFullStr = $"{TBChestTracker.Resources.Strings.lvl} {levelNumberStr}";
                                    }

                                    levelFullLength = levelFullStr.Length; //-- 'level|nivel 10' should equal to 8 characters in length.

                                    var levelArray = levelNumberStr.Split('-');

                                    if (levelArray.Count() == 1)
                                    {
                                        if (Int32.TryParse(levelArray[0], out level) == false)
                                        {
                                            //-- couldn't extract level.
                                        }
                                    }
                                    else if (levelArray.Count() > 1)
                                    {
                                        if (Int32.TryParse(levelArray[0], out level) == false)
                                        {
                                            //-- couldn't extract level.
                                        }
                                    }

                                    //-- now we make sure levelStartPos == 0 or more than 0.
                                    var direction = levelStartPos == 0 ? "forwards" : "backwards";
                                    if (direction == "forwards")
                                    {
                                        ChestType = source.Substring(levelFullLength + 1);
                                    }
                                    else
                                    {
                                        //-- Cripta de nivel 10 = 18 characters long.
                                        //-- Cripta de = 10 characters long
                                        //-- nivel 10 =  8 characters long.

                                        ChestType = source.Substring(0, source.Length - levelFullLength);
                                        ChestType = ChestType.Trim(); //-- remove any whitespaces at the end 
                                    }
                                    if (ChestType.StartsWith(TBChestTracker.Resources.Strings.OnlyCrypt))
                                    {
                                        ChestType = ChestType.Insert(0, $"{TBChestTracker.Resources.Strings.Common} ");
                                    }

                                    chest.Type = ChestType;
                                    if (chest.Level != level)
                                    {
                                        chest.Level = level;
                                        Debug.WriteLine($"Repairing Chest {chest.Source} that contain lvl.");
                                        chestErrors++;
                                    }

                                }
                            }

                            //-- these issues contain no level inside source.
                            if (chest.Source.Contains(TBChestTracker.Resources.Strings.lvl) == false && chest.Source.Contains(TBChestTracker.Resources.Strings.Level) == false)
                            {
                                if (chest.Source.ToLower().Equals(chest.Type.ToLower()) == false)
                                {
                                    if (chest.Source.ToLower().EndsWith(chest.Type.ToLower()))
                                    {
                                        //-- these errors generally have a level 5 whereas should be 0.
                                        if (chest.Level == 5)
                                        {
                                            chest.Level = 0;
                                        }

                                        chest.Type = chest.Source;
                                        Debug.WriteLine("Fixing Corrupted Chests.");
                                        chestErrors++;
                                    }
                                }
                            }
                            if (chestsettings.GeneralClanSettings.ChestOptions == ChestOptions.UsePoints)
                            {
                                foreach (var chestpointvalue in chestpointsvalues)
                                {
                                    var chestname = chest.Name;
                                    var chesttype = chest.Type;

                                    if (chesttype.ToLower().Contains(chestpointvalue.ChestType.ToLower()))
                                    {
                                        if (chestpointvalue.ChestName.Equals("(Any)"))
                                        {
                                            if (!chestpointvalue.Level.Equals("(Any)"))
                                            {
                                                var chestlevel = Int32.Parse(chestpointvalue.Level);
                                                if (chest.Level == chestlevel)
                                                {
                                                    total_chest_points += chestpointvalue.PointValue;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                total_chest_points += chestpointvalue.PointValue;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (chestname.ToLower().Contains(chestpointvalue.ChestName.ToLower()))
                                            {
                                                if (!chestpointvalue.Level.Equals("(Any)"))
                                                {
                                                    var chestlevel = Int32.Parse(chestpointvalue.Level);
                                                    if (chest.Level == chestlevel)
                                                    {
                                                        total_chest_points += chestpointvalue.PointValue;
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    total_chest_points += chestpointvalue.PointValue;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (clanmate_points != total_chest_points)
                        {
                            _data.Points = total_chest_points;
                            chestErrors++;
                        }
                    }

                    // Search for clanmates no longer within clanmates db
                    var clanmate = _data.Clanmate;
                    var clanmates = ClanManager.Instance.ClanmateManager.Database.Clanmates;
                    bool bExists = clanmates.Select(c => c.Name).Contains(clanmate);
                    if (bExists == false)
                    {
                        Debug.WriteLine($"{clanmate} doesn't exist anymore within clanmates database. Removing.");
                        RemoveChestData(clanmate);
                        chestErrors++;
                    }
                }
            }

            if (chestErrors > 0)
            {
                Save();
                CreateBackup();
                return true;
            }
            return false;
        }
        #endregion

        #region BuildData()
        public ChestDataBuildResult BuildData()
        {
            var result = Load();
            var dateformat = DateFormat;
            var currentCulture = CultureInfo.CurrentCulture; //-- en-GB

            if (result == false)
            {
                MessageBox.Show("Unable to build clan data. Something went horribly wrong.", "Building Clan Data Failed");
                return ChestDataBuildResult.LOAD_FAIL;
            }
            var build_result = ChestProcessor.Init(Database);

            if(build_result == ChestDataBuildResult.OK)
            {
                AppContext.Instance.ClanmatesBeenAdded = true;
            }
            return build_result;

            /*
            //--- build blank clanchestdata 
            foreach (var clanmate in ClanManager.Instance.ClanmateManager.Database.Clanmates)
            {
                if (!String.IsNullOrEmpty(clanmate.Name))
                {
                    clanChestData.Add(new ClanChestData(clanmate.Name, null));
                }
            }
            */
            /*
            //--- midnight bug may occur here.
            if (Database != null && Database.ClanChestData != null && Database.ClanChestData.Keys != null && Database.ClanChestData.Keys.Count > 0)
            {
                var lastDate = Database.ClanChestData.Keys.Last();
                var datestring = DateTime.Now.ToString(DateFormat, new CultureInfo(CultureInfo.CurrentCulture.Name));
                invalidDateFormat = DoesDatesNeedRepair();
                if(lastDate.Equals(datestring))
                {

                }

            }

            if (ClanChestDailyData != null && ClanChestDailyData.Keys != null && ClanChestDailyData.Keys.Count > 0)
            {

                var lastDate = ClanChestDailyData.Keys.Last();
                var dateStr = DateTime.Now.ToString(DateFormat, new CultureInfo(CultureInfo.CurrentCulture.Name));

                invalidDateFormat = DoesDatesNeedRepair();

                if (lastDate.Equals(dateStr))
                {

                    clanChestData = ClanChestDailyData[lastDate];
                    foreach (var member in ClanManager.Instance.ClanmateManager.Database.Clanmates)
                    {
                        var clanmate_exists = clanChestData.ToList().Exists(mate => mate.Clanmate.ToLower().Contains(member.Name.ToLower()));
                        if (!clanmate_exists)
                        {
                            clanChestData.Add(new ClanChestData(member.Name, null));
                        }
                    }
                }
                else
                {
                    try
                    {
                        ClanChestDailyData.Add(DateTime.Now.ToString(dateformat, new CultureInfo(CultureInfo.CurrentCulture.Name)), clanChestData);
                    }
                    catch (Exception ex)
                    {
                        return ChestDataBuildResult.DATA_CORRUPT;
                    }
                }
            }

            if (invalidDateFormat)
            {
                return ChestDataBuildResult.DATA_CORRUPT;
            }

            

            return ChestDataBuildResult.OK;
            */

        }
        #endregion

        #region Clear
        public void Clear()
        {
            Database.RemoveAllEntries();
            ClanChestData.Clear();
            ClanManager.Instance.ClanmateManager.Database.Clanmates.Clear();
        }
        #endregion

        #region RemoveChestData
        public void RemoveChestData(string clanmatename)
        {
            foreach (var date in Database.ClanChestData.ToList())
            {
                var chestdata = date.Value;
                foreach (var data in chestdata.ToList())
                {
                    if (data.Clanmate.Equals(clanmatename, StringComparison.CurrentCultureIgnoreCase))
                    {
                        var name = data.Clanmate;
                        chestdata.Remove(data);
                        com.HellStormGames.Logging.Console.Write($"{name} was successfully removed from clanchestdata.", "Clanmate Removal", com.HellStormGames.Logging.LogType.INFO);
                    }
                }
            }
            //--- we're done.
            Save();
            return;
        }
        #endregion

        public bool DoesDatesNeedRepair()
        {
            var currentCulture = CultureInfo.CurrentCulture; //-- en-GB
                                                             //-- attempting to repair date.
            var dates = new List<string>();
            foreach (var date_key in Database.ClanChestData.Keys.ToList())
            {
                dates.Add(date_key);
            }

            var culture_seperator = currentCulture.DateTimeFormat.DateSeparator;
            var culture_shortDateFormat = currentCulture.DateTimeFormat.ShortDatePattern;
            var possible_locale = String.Empty;

            var first_part = String.Empty;
            var second_part = String.Empty;
            var last_part = String.Empty;
            var detected_month = String.Empty;
            var detected_year = String.Empty;
            var detected_day = String.Empty;

            int day_key = -1, month_key = -1, year_key = -1;

            DateTime? date_Created = null;

            int y = 0, d = 0, m = 0;
            var max_samples = 3;
            var result = false;

            foreach (var date in dates)
            {
                var date_seperator = String.Empty;

                for (var sample = 0; sample < max_samples; sample++)
                {
                    var _date = dates[sample];

                    if (_date.Contains("/"))
                    {
                        date_seperator = "/";
                    }
                    else if (_date.Contains("-"))
                    {
                        date_seperator = "-";
                    }
                    else if (_date.Contains("."))
                    {
                        date_seperator = ".";
                    }

                    var date_parts = _date.Split(date_seperator[0]); //-- should be 3 arrays. 
                    var current_first_part = date_parts[0];
                    var current_second_part = date_parts[1];
                    var current_last_part = date_parts[2];

                    if (first_part != current_first_part)
                    {
                        first_part = current_first_part;
                        detected_day = current_first_part;
                        day_key = 0;
                    }
                    else
                    {
                        //-- we can possibly say that we've detected the month or year.
                        if (current_first_part.Length > 2)
                        {
                            detected_year = current_first_part;
                            year_key = 0;
                        }
                        else
                        {
                            detected_month = current_first_part;
                            month_key = 0;
                        }
                    }

                    if (second_part != current_second_part)
                    {
                        second_part = current_second_part;
                        detected_day = current_second_part;
                        day_key = 1;
                    }
                    else
                    {
                        //-- month detected. 
                        detected_month = current_second_part;
                        month_key = 1;
                    }

                    if (last_part != current_last_part)
                    {
                        last_part = current_last_part;
                    }
                    else
                    {
                        //-- probably year or date or month. 
                        if (current_last_part.Length > 2)
                        {
                            //-- year 
                            detected_year = current_last_part;
                            year_key = 2;
                        }
                        else
                        {
                            //-- possibly day.

                        }
                    }
                }
                var dateParts = date.Split(date_seperator[0]);

                Int32.TryParse(dateParts[month_key], out m);
                Int32.TryParse(dateParts[day_key], out d);
                Int32.TryParse(dateParts[year_key], out y);

                try
                {
                    date_Created = new DateTime(y, m, d);
                    var newLocaleDate = date_Created?.ToString(AppContext.Instance.ForcedDateFormat, currentCulture);
                    result = date.Equals(newLocaleDate);
                }
                catch (Exception ex)
                {

                }
            }

            return !result; //-- returning the opposite. If it matches then we return false. If it does not match, we return true.
        }


        public void Dispose()
        {
            if (Database != null)
            {
                Database.Dispose();
                Database = null;
                ChestProcessor.Dispose();
                ChestProcessor=null;
            }
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
