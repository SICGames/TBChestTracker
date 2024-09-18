using com.HellStormGames.Logging;
using Emgu.CV.CvEnum;
using Microsoft.Win32;
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
using System.Threading.Tasks;
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
    */
    [System.Serializable]
    public class ClanChestManager
    {
        #region Declarations
        public List<ClanChestData> clanChestData { get; set; }
        public Dictionary<string, List<ClanChestData>> ClanChestDailyData;
        private ChestProcessingState pChestProcessingState = ChestProcessingState.IDLE;
        bool filteringErrorOccurred = false;
        string lastFilterString = String.Empty;
        #endregion

        #region ChestProcessingState
        public ChestProcessingState ChestProcessingState
        {
            get => pChestProcessingState;
            set => pChestProcessingState = value;
        }
        #endregion

        #region Constructor
        public ClanChestManager()
        {
            clanChestData = new List<ClanChestData>();
            ClanChestDailyData = new Dictionary<string, List<ClanChestData>>();
            ChestProcessingState = ChestProcessingState.IDLE;
        }
        #endregion

        #region Give Chest
        public void Give(string clanmate, ClanChestData pClanChestData)
        {
            //-- needs to finish before release.
            var todayStr = DateTime.Now.ToShortDateString();
            var currentClanChestDate = ClanChestDailyData.Where(d => d.Key.Equals(todayStr));
            var clanmates = ClanManager.Instance.ClanmateManager.Database.Clanmates.ToList();
            if (currentClanChestDate.Count() == 0)
            {
                //-- new date needs to be placed.
                clanChestData.Clear();
                foreach(var mate in clanmates)
                {
                    clanChestData.Add(new ClanChestData(mate.Name, null, 0));
                }

                ClanChestDailyData.Add(DateTime.Now.ToString("d", new CultureInfo(CultureInfo.CurrentCulture.Name)), clanChestData);
            }
            bool bAliasFound = false;
            var bClanmate = ClanManager.Instance.ClanmateManager.Database.Find(clanmate); //clanmates.Select(mate_name => mate_name.Name).Contains(clanmate, StringComparer.CurrentCultureIgnoreCase);
            if(bClanmate != null)
            {
                //-- Check Aliases
                foreach (var mate in clanmates)
                {
                    if (mate.Aliases.Count > 0)
                    {
                        var aliasMatches = mate.Aliases.Contains(clanmate, StringComparer.CurrentCultureIgnoreCase);
                        if (aliasMatches)
                        {

                        }
                    }
                }
            }
        }
        #endregion

        //-- RepairChestData returns false if nothing needs to be done. Returns true if it's been repaired.
        public bool RepairChestData()
        {
            int chestErrors = 0;

            var chestsettings = ClanManager.Instance.ClanChestSettings;
            var chestdata = ClanManager.Instance.ClanChestManager.ClanChestDailyData;
          
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
                        foreach (var chest in chests)
                        {
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

                            //-- fix corrupted chests
                            /*
                                    {
                                        "Name": "Gladiator's Chest",
                                        "Type": "rena",
                                        "Source": "Arena",
                                        "Level": 5
                                    }
                              Most affected are Bank and Arenas. 
                            */

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
                            Debug.WriteLine("Chest Points don't add up.");
                            chestErrors++;
                        }
                    }

                    // Search for clanmates no longer within clanmates db
                    var clanmate = _data.Clanmate;
                    var clanmates = ClanManager.Instance.ClanmateManager.Database.Clanmates;
                    bool bExists = clanmates.Select(c => c.Name).Contains(clanmate);
                    if(bExists == false)
                    {
                        Debug.WriteLine($"{clanmate} doesn't exist anymore within clanmates database. Removing.");
                        RemoveChestData(clanmate);
                        chestErrors++;
                    }

                }
            }

            if(chestErrors > 0)
            {
                com.HellStormGames.Logging.Console.Write("Chest Data Automatically Repaired", "Chest Integrity", LogType.INFO);
                CreateBackup();
                SaveData();
                return true;
            }
            com.HellStormGames.Logging.Console.Write("Chest Data looks good. No repairs needed.", "Chest Integrity", LogType.INFO);
            return false;
        }
        
        #region ClearData
        public void ClearData()
        {
            ClanChestDailyData.Clear();
            clanChestData.Clear();
            ClanManager.Instance.ClanmateManager.Database.Clanmates.Clear();
        }
        #endregion

        #region RemoveChestData
        public void RemoveChestData(string clanmatename)
        {
            foreach (var date in ClanChestDailyData.ToList())
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
            SaveData();
            return;
        }
        #endregion

        #region FilterChestData & ContainsAny
        private void FilterChestData(ref List<String> data, String[] words, System.Action<string> onError)
        {
            var filtered = data.Where(d => ContainsAny(d, words));
            data = filtered.ToList();
            if (filteringErrorOccurred)
            {
                //-- if some how we came to this point, junk data exists.
                var dbg_msg = $"---- '{lastFilterString}' from OCR results doesn't match specified values. Make sure spelling is correct in OCR filtering in Settings.";
                onError(dbg_msg); 
             
            }
        }

        private bool ContainsAny(string str, IEnumerable<string> values)
        {
            if (!string.IsNullOrEmpty(str) || values.Any())
            {
                foreach (string value in values)
                {
                    var bExists = str.IndexOf(value, StringComparison.CurrentCultureIgnoreCase) != -1;
                    if (bExists)
                        return true;
                }

                //-- since we don't care about Contains: on expired gifts. We skip it.
                //-- But we care about anything that is not listed within the OCR Filtering.
                if (!str.ToLower().Contains("contains"))
                {
                    lastFilterString = str;
                    filteringErrorOccurred = true;
                }
            }
            return false;
        }
        #endregion

        #region ProcessText3 
        private ProcessingTextResult ProcessText3(List<string> result)
        {
            ProcessingTextResult ProcessingTextResult = new ProcessingTextResult();
            bool bFilteringError = false;
            string sFilteringErrorMessage = String.Empty;

            lastFilterString = String.Empty;
            filteringErrorOccurred = false;

            //---Processing Chests Took: 00:00:00.0115509
            List<ChestData> tmpchests = new List<ChestData>();

            //-- quick filter. 
            //-- in Triumph Chests, divider creates dirty characters. 
            var filter_words = SettingsManager.Instance.Settings.OCRSettings.Tags.ToArray(); // new string[] { "Chest", "From", "Source", "Gift" };

            //-- OCR Result exception from filtering resulted in:
            //-- From : [player_name]
            //-- Patched 1/22/24

            FilterChestData(ref result, filter_words, errorResult =>
            {
                if(!String.IsNullOrEmpty(errorResult))
                {
                    com.HellStormGames.Logging.Console.Write(errorResult, "Invalid OCR", com.HellStormGames.Logging.LogType.INFO);
                    bFilteringError = true;
                    sFilteringErrorMessage = errorResult;
                }
            });

            if(bFilteringError)
            {
                ProcessingTextResult.Status = ProcessingStatus.UNKNOWN_ERROR;
                ProcessingTextResult.Message = sFilteringErrorMessage;
                return ProcessingTextResult;
            }

            bool bError = false;

            for (var x = 0; x < result.Count; x += 3)
            {
                var word = result[x];

                if (word == null)
                    break;

                if (!word.Contains(TBChestTracker.Resources.Strings.Clan))
                {
                    var chestName = "";
                    var clanmate = "";
                    var chestobtained = "";
                    try
                    {
                        chestName = result[x + 0];
                        clanmate = result[x + 1];
                        chestobtained = result[x + 2];
                    }
                    catch (Exception e)
                    {
                        ProcessingTextResult.Status = ProcessingStatus.INDEX_OUT_OF_RANGE;
                        ProcessingTextResult.Message = $"An error occured while processing OCR text from screen. This could indicate a word not added to filtering list. Go to Settings -> OCR and add the appropriate chest name to the filter list.";
                        bError = true;
                        break;
                    }

                    com.HellStormGames.Logging.Console.Write($"OCR RESULT [{chestName}, {clanmate}, {chestobtained}", "OCR Result", LogType.INFO);

                    if (clanmate.ToLower().Contains(TBChestTracker.Resources.Strings.From.ToLower()))
                    {

                        //--- clean up
                        //--- Sometimes there's a From : Playername
                        //--- Causing The Iroh Bug.
                        try
                        {
                            //--- skip the space check and the odd symbol to get straight to the meat.
                            clanmate = clanmate.Substring(clanmate.IndexOf(' ') + 1);
                            if (clanmate.ToLower().Contains(TBChestTracker.Resources.Strings.From.ToLower()))
                            {
                                //-- error - shouldn't even have reached this point.
                                //-- game actually causes this error from not rendering name fast enough 
                                //-- hasbeen patched but throw exception just in case.
                                var badname = 0;
                                var fromStartingPos = clanmate.IndexOf(TBChestTracker.Resources.Strings.From);
                                if(fromStartingPos >= 0)
                                {
                                    com.HellStormGames.Logging.Console.Write($"Attempting to correct clanmate name => {clanmate}", "Clanmate Name Issue", LogType.INFO);
                                    clanmate = clanmate.Remove(fromStartingPos, clanmate.IndexOf(' ') + 1);
                                    com.HellStormGames.Logging.Console.Write($"Clanmate name after correction => {clanmate}", "Clanmate Repair Result", LogType.INFO);
                                }

                                //throw new Exception("Clanmate name is blank. Increase thread sleep timer to prevent this.");
                            }

                        }
                        catch (Exception e)
                        {
                            bError = true;
                            ProcessingTextResult.Status = ProcessingStatus.CLANMATE_ERROR;
                            ProcessingTextResult.Message = $"Seems to be an issue while extracting clanmate's name. Exception caught => {e.Message}. ";
                            com.HellStormGames.Logging.Console.Write($"Couldn't Process Clanmate name correctly. Affected Clanmate => {clanmate}", "Clanmate Extraction Failed", LogType.ERROR);
                            break;
                        }
                    }

                    //-- building tmpchestdata
                    if (chestobtained.Contains(TBChestTracker.Resources.Strings.Source))
                    {
                        //-- foreign language prevent speeding this up.
                        //-- Spanish - Cripta de nivel 10 translate to english Level 10 Crypt.
                        //-- First approach was extract type of crypt and phase out chesttype completely. 
                        //-- Source: Cripta de 
                        //-- Level of crypt: 10

                        var ChestSource = chestobtained.Substring(chestobtained.IndexOf(":") + 2);
                        var levelStartPos = -1;

                        if (ChestSource.Contains(TBChestTracker.Resources.Strings.Level))
                        {
                          levelStartPos = ChestSource.IndexOf(TBChestTracker.Resources.Strings.Level);
                        }
                        else if(ChestSource.Contains(TBChestTracker.Resources.Strings.lvl))
                        {
                           levelStartPos = ChestSource.IndexOf(TBChestTracker.Resources.Strings.lvl);
                        }

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
                            var levelStr = ChestSource.Substring(levelStartPos).ToLower();
                            var levelNumberStr = levelStr.Substring(levelStr.IndexOf(" ") + 1);

                            //-- using a quantifer to check if there is an additional space after the level number. If user is spanish, no space after level number.
                            levelNumberStr = levelNumberStr.IndexOf(" ") > 0 ? levelNumberStr.Substring(0, levelNumberStr.IndexOf(" ")) : levelNumberStr;
                            var levelFullStr = String.Empty;
                            if (ChestSource.Contains(TBChestTracker.Resources.Strings.Level))
                            {
                               levelFullStr =  $"{TBChestTracker.Resources.Strings.Level} {levelNumberStr}";
                            }
                            else if(ChestSource.Contains(TBChestTracker.Resources.Strings.lvl))
                            {
                                levelFullStr = $"{TBChestTracker.Resources.Strings.lvl} {levelNumberStr}";
                            }

                            levelFullLength = levelFullStr.Length; //-- 'level|nivel 10' should equal to 8 characters in length.

                            var levelArray = levelNumberStr.Split('-');
                                
                            if(levelArray.Count() == 1)
                            {
                                if (Int32.TryParse(levelArray[0], out level) == false)
                                {
                                    //-- couldn't extract level.
                                }
                            }
                            else if(levelArray.Count() > 1)
                            {
                                if(Int32.TryParse(levelArray[0],out level) == false)
                                {
                                    //-- couldn't extract level.

                                }
                            }
                            if(level == 0)
                            {
                                level = 5;
                            }

                            //-- now we make sure levelStartPos == 0 or more than 0.
                            var direction = levelStartPos == 0 ? "forwards" : "backwards";
                            if (direction == "forwards")
                            {
                                ChestType = ChestSource.Substring(levelFullLength + 1);
                            }
                            else
                            {
                                //-- Cripta de nivel 10 = 18 characters long.
                                //-- Cripta de = 10 characters long
                                //-- nivel 10 =  8 characters long.

                                ChestType = ChestSource.Substring(0, ChestSource.Length - levelFullLength);
                                ChestType = ChestType.Trim(); //-- remove any whitespaces at the end 

                            }
                            if(ChestType.StartsWith(TBChestTracker.Resources.Strings.OnlyCrypt))
                            {
                              ChestType = ChestType.Insert(0, $"{TBChestTracker.Resources.Strings.Common} ");
                            }
                        }
                        else
                        {
                            //--- there is no level
                            ChestType = ChestSource;
                            level = 0;
                        }

                        tmpchests.Add(new ChestData(clanmate, new Chest(chestName, ChestType, ChestSource, level)));
                        var dbg_msg = String.Empty;

                        if (level != 0)
                        {
                            dbg_msg = $"--- ADDING level {level} {ChestType.ToString()}  '{chestName}' from {clanmate} ----";
                        }
                        else
                        {
                            dbg_msg = $"--- ADDING {ChestType.ToString()}  '{chestName}' from {clanmate} ----";
                        }

                        com.HellStormGames.Logging.Console.Write(dbg_msg, "OCR Result", com.HellStormGames.Logging.LogType.INFO);
                    }
                }
            }

            if (bError)
            {
                ProcessingTextResult.ChestData = tmpchests;
                return ProcessingTextResult;
            }

            ProcessingTextResult.Status = ProcessingStatus.OK;
            ProcessingTextResult.Message = "Success";
            ProcessingTextResult.ChestData = tmpchests;

            return ProcessingTextResult;
        }
        #endregion

        #region CreateClanChestData
        private List<ClanChestData> CreateClanChestData(List<ChestData> chestdata)
        {
            List<ClanChestData> clanChestData = new List<ClanChestData>();
            var clanmates = chestdata.GroupBy(clanmate => clanmate.Clanmate);

            //-- create temp clanchestdata
            foreach (var clanmate in clanmates)
            {
                ClanChestData clanchestdata = new ClanChestData();
                clanchestdata.chests = new List<Chest>();
                clanchestdata.Clanmate = clanmate.Key;
                foreach (var name in clanmate)
                {
                    clanchestdata.chests.Add(name.Chest);
                }
                clanChestData.Add(clanchestdata);
            }
            return clanChestData;
        }
        #endregion

        private double ClanmateSimilarities(string input1, string input2)
        {
            return input1.CalculateSimilarity(input2);
        }
        private Clanmate Clanmate_Scan(string input, double similiarityThreshold)
        {
            /*
               "Name": "Naida Il",
                      "Aliases": [
                         "Naida II",
                         "Naida ١",
                         "Naida ll",
                         "Naida ١١",
                         "Naida 11",
                         "Naida l|" 
            */
            var inputLength = input.Length; 

            var clanmates = ClanManager.Instance.ClanmateManager.Database.Clanmates.ToList();

            bool bMatch = false;
            Clanmate matchedClanmate = null;

            var jw = new F23.StringSimilarity.JaroWinkler();

            foreach (var clanmate in clanmates)
            {
                var similarity = jw.Similarity(clanmate.Name, input) * 100.0;
                Debug.WriteLine($"{clanmate.Name} and {input} have a Similiarity Percent => {similarity}%");
                if(similarity >  similiarityThreshold)
                {
                    bMatch = true;
                    matchedClanmate = clanmate;
                    break;
                }
            }
            if (bMatch)
            {
                return matchedClanmate;
            }

            return null;
        }

        public void ProcessChestData(List<string> result, ChestAutomation chestAutomation)
        {

            /*
             * TmpChests gather processed chests.
             * Stuffs into temporary clanchestdata
             * Need to make sure we don't need to start new entry in ClanChestDaily.
             * ClanChestData should be obsolete.
            */

            AppContext.Instance.isBusyProcessingClanchests = true;
            var resulttext = result;
            
            if (resulttext[0].ToLower().Contains("no gifts"))
            {
                ChestProcessingState = ChestProcessingState.IDLE;
                AppContext.Instance.isAnyGiftsAvailable = false;
                
                chestAutomation.InvokeChestProcessed(new Automation.AutomationChestProcessedEventArguments(new ClanChestProcessResult("No Gifts", 404, ClanChestProcessEnum.NO_GIFTS)));
                return;
                //return new ClanChestProcessResult("No Gifts", 404, ClanChestProcessEnum.NO_GIFTS);
            }

            ChestProcessingState = ChestProcessingState.PROCESSING;
            AppContext.Instance.isAnyGiftsAvailable = true;
            ProcessingTextResult textResult = ProcessText3(resulttext);

            if (textResult.Status != ProcessingStatus.OK)
            {
                //onError(new ChestProcessingError($"{textResult.Message}"));
                chestAutomation.InvokeChestProcessingFailed(new AutomationChestProcessingFailedEventArguments($"{textResult.Message}", 0));
                return;
                //return new ClanChestProcessResult($"{textResult.Message}", 0, ClanChestProcessEnum.ERROR);
            }

            List<ChestData> tmpchests = textResult.ChestData;
            
            List<ClanChestData> tmpchestdata = CreateClanChestData(tmpchests);
            var clanmates = ClanManager.Instance.ClanmateManager.Database.Clanmates.ToList();

            //-- do we need to insert new entry?
            //-- causes midnight bug.
            var currentdate = DateTime.Now.ToShortDateString();
            var dates = ClanChestDailyData.Where(x => x.Key.Equals(currentdate));

            if (dates.Count() == 0)
            {
                clanChestData.Clear();
                foreach (var mate in clanmates)
                {
                    clanChestData.Add(new ClanChestData(mate.Name, null, 0));
                }

                ClanChestDailyData.Add(DateTime.Now.ToString("d", new CultureInfo(CultureInfo.CurrentCulture.Name)), clanChestData);
            }

            //-- make sure clanmate exists.
            var mate_index = 0;
            foreach (var tmpchest in tmpchestdata.ToList())
            {
                bool exists = clanmates.Select(mate_name => mate_name.Name).Contains(tmpchest.Clanmate, StringComparer.CurrentCultureIgnoreCase);
               
                if (!exists)
                {
                    var tClanmate = tmpchest.Clanmate;
                    var match_clanmate = Clanmate_Scan(tClanmate,80.0);
                    if(match_clanmate != null)
                    {
                        com.HellStormGames.Logging.Console.Write($"{tmpchest.Clanmate} is actually {match_clanmate.Name}.", "Unknown Clanmate Found", LogType.INFO);
                        
                        tmpchestdata[mate_index].Clanmate = match_clanmate.Name; //--- unknown clanmate properly identified and been re-written with correct parent clan name.

                        var parent_data = clanChestData.Select(pd => pd).Where(data => data.Clanmate.Equals(match_clanmate.Name, StringComparison.InvariantCultureIgnoreCase)).ToList()[0];
                        if (parent_data.chests == null)
                        {
                            parent_data.chests = new List<Chest>();
                        }
                    }
                    else
                    {
                        clanChestData.Add(new ClanChestData(tmpchest.Clanmate, tmpchest.chests, tmpchest.Points));
                        com.HellStormGames.Logging.Console.Write($"Adding {tmpchest.Clanmate} to clanmates database.", "Clanmate Not Found", LogType.WARNING);
                        ClanManager.Instance.ClanmateManager.Add(tmpchest.Clanmate);
                        ClanManager.Instance.ClanmateManager.Save();
                    }
                    
                    //-- attempt to check to see if the unfound clanmate is under an alias.
                    /*
                    foreach (var mate in clanmates)
                    {
                        if (mate.Aliases.Count > 0)
                        {
                            bool isMatch = mate.Aliases.Contains(tmpchest.Clanmate, StringComparer.InvariantCultureIgnoreCase);
                            if (isMatch)
                            {
                                com.HellStormGames.Logging.Console.Write($"\t\t Unknown clanmate ({tmpchest.Clanmate}) belongs to {mate.Name} aliases.", "Unknown Clanmate", LogType.INFO);
                                var parent_data = clanChestData.Select(pd => pd).Where(data => data.Clanmate.Equals(mate.Name, StringComparison.InvariantCultureIgnoreCase)).ToList()[0];
                                alias_found = true;
                                break;
                            }
                        }
                    }
                    */
                }
                else
                {
                    mate_index += 1;
                    continue;
                }
            }

            //-- update clanchestdata
            foreach (var chestdata in clanChestData)
            {
                var _chestdata = tmpchestdata.Where(name => name.Clanmate.Equals(chestdata.Clanmate,
                    StringComparison.CurrentCultureIgnoreCase)).Select(chests => chests).ToList();

                if (_chestdata.Count > 0)
                {
                    var m_chestdata = _chestdata[0];

                    //--- chest data returning null after correcting parent clan name.
                    if (chestdata.Clanmate.Equals(m_chestdata.Clanmate, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (chestdata.chests == null)
                        {
                            chestdata.chests = new List<Chest>();
                            chestdata.Points = 0;
                        }

                        foreach (var m_chest in m_chestdata.chests.ToList())
                        {
                            if (ClanManager.Instance.ClanChestSettings.GeneralClanSettings.ChestOptions == ChestOptions.UsePoints)
                            {
                                var pointvalues = ClanManager.Instance.ClanChestSettings.ChestPointsSettings.ChestPoints;
                                foreach (var pointvalue in pointvalues)
                                {
                                    var chest_type = m_chest.Type.ToString();
                                    var chest_name = m_chest.Name.ToString();

                                    var level = m_chest.Level;

                                    if(pointvalue.ChestType.ToLower().Contains(chest_type.ToLower().ToString()))
                                    {
                                        if (pointvalue.ChestName.Equals("(Any)"))
                                        {
                                            if (pointvalue.Level.Equals("(Any)"))
                                            {
                                                chestdata.Points += pointvalue.PointValue;
                                                break;
                                            }
                                            else
                                            {
                                                var chestlevel = Int32.Parse(pointvalue.Level.ToString());
                                                if (level == chestlevel)
                                                {
                                                    chestdata.Points += pointvalue.PointValue;
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (chest_name.ToLower().Equals(pointvalue.ChestName.ToLower()))
                                            {
                                                if (pointvalue.Level.Equals("(Any)"))
                                                {
                                                    chestdata.Points += pointvalue.PointValue;
                                                    break;
                                                }
                                                else
                                                {
                                                    var chestlevel = Int32.Parse(pointvalue.Level.ToString());
                                                    if (level == chestlevel)
                                                    {
                                                        chestdata.Points += pointvalue.PointValue;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            chestdata.chests.Add(m_chest);
                        }
                    }
                }
            }

            var datestr = DateTime.Now.ToString("d", new CultureInfo(CultureInfo.CurrentCulture.Name));   
            ClanChestDailyData[datestr] = clanChestData;
            ChestProcessingState = ChestProcessingState.COMPLETED;
           
            AppContext.Instance.isBusyProcessingClanchests = false;
            AppContext.Instance.canCaptureAgain = true;

            Automation.AutomationChestProcessedEventArguments args = new AutomationChestProcessedEventArguments(new ClanChestProcessResult("200", 200, ClanChestProcessEnum.SUCCESS));
            chestAutomation.InvokeChestProcessed(args);

            //return new ClanChestProcessResult("Success", 200, ClanChestProcessEnum.SUCCESS);
        }
        
        public async void LoadData()
        {
            var clanmatefile = $"{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanmateDatabaseFile}";
            var clanchestfile = $"{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanChestDatabaseFile}";
            var chestsettingsfile = $"{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanSettingsFile}";

            if (ClanManager.Instance.ClanmateManager.Database.Clanmates != null)
                ClanManager.Instance.ClanmateManager.Database.Clanmates.Clear();

            if (!System.IO.File.Exists(clanmatefile))
            {
                AppContext.Instance.ClanmatesBeenAdded = false;
                return;
            }
            else
            {
                ClanManager.Instance.ClanmateManager.Load(clanmatefile);
            }

            //-- when adding new member, it pulls previous clan chest statistic data.
            //-- needs to add the new member in.

            if (!System.IO.File.Exists(clanchestfile))
            {
                ClanChestDailyData.Add(DateTime.Now.ToString("d", new CultureInfo(CultureInfo.CurrentCulture.Name)), clanChestData);
                await SaveDataTask();
            }
            else
            {
                using (var sw = File.OpenText(clanchestfile))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    ClanChestDailyData = (Dictionary<string, List<ClanChestData>>)serializer.Deserialize(sw, typeof(Dictionary<string, List<ClanChestData>>));
                }
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

                ClanManager.Instance.ClanChestSettings.LoadSettings(chestsettingsfile);
            }
            return;
        }
        public void BuildData()
        {
            LoadData(); 
            
            //--- build blank clanchestdata 
            foreach (var clanmate in ClanManager.Instance.ClanmateManager.Database.Clanmates)
            {
                if (!String.IsNullOrEmpty(clanmate.Name))
                {
                    clanChestData.Add(new ClanChestData(clanmate.Name, null));
                }
            }

            //--- midnight bug may occur here.
            if (ClanChestDailyData != null && ClanChestDailyData.Keys != null && ClanChestDailyData.Keys.Count > 0)
            {
                var lastDate = ClanChestDailyData.Keys.Last();
                var dateStr = DateTime.Now.ToString("d");
                if (lastDate.Equals(dateStr))
                {
                    clanChestData = ClanChestDailyData[lastDate];
                    foreach (var member in ClanManager.Instance.ClanmateManager.Database.Clanmates)
                    {
                        var clanmate_exists = clanChestData.Exists(mate => mate.Clanmate.ToLower().Contains(member.Name.ToLower()));
                        if (!clanmate_exists)
                        {
                            clanChestData.Add(new ClanChestData(member.Name, null));
                        }
                    }
                }
                else
                {
                    ClanChestDailyData.Add(DateTime.Now.ToString("d", new CultureInfo(CultureInfo.CurrentCulture.Name)), clanChestData);
                }

                AppContext.Instance.ClanmatesBeenAdded = true;
            }

            return;
        }
        
        public void SaveData(string filepath = "")
        {
            //-- write to file.
            string file = String.Empty;
            if (String.IsNullOrEmpty(filepath))
                file = $"{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanChestDatabaseFile}";
            else
                file = filepath;

            try
            {
                using (var sw = File.CreateText(file))
                {
                    JsonSerializer serialize = new JsonSerializer();
                    serialize.Formatting = Formatting.Indented;
                    serialize.Serialize(sw, ClanChestDailyData);
                    serialize = null;
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine(ex.Message);
            }
        }
        public Task SaveDataTask()
        {
            return Task.Run(() => SaveData());
        }

        public void CreateBackup()
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.Now;
            var clanchestsBackupFolder = $"{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanDatabaseBackupFolderPath}//Clanchests";
            var di = new DirectoryInfo(clanchestsBackupFolder);
            if (di.Exists == false)
            {
                di.Create();
            }


            string file = $"{clanchestsBackupFolder}//clanchest_backup_{dateTimeOffset.ToUnixTimeSeconds()}.db";
            try
            {
                using (var sw = File.CreateText(file))
                {
                    JsonSerializer serialize = new JsonSerializer();
                    serialize.Formatting = Formatting.Indented;
                    serialize.Serialize(sw, ClanChestDailyData);
                    serialize = null;
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine(ex.Message);
            }
        }

        public Dictionary<string, List<ClanChestData>> FilterClanChestByConditions()
        {
            var dailyChests = new Dictionary<string, List<ClanChestData>>();
            var chestRequirements = ClanManager.Instance.ClanChestSettings.ChestRequirements;
            foreach (var dailyChestData in ClanChestDailyData.ToList())
            {
                var date = dailyChestData.Key;
                var dailychest = ClanChestDailyData[date];
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

                                    if (chest.Type.ToLower().Contains( condition.ChestType.ToLower()))
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
                                            if(chest.Name.ToLower().Equals(condition.ChestName.ToLower()))
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
        #region "Soon To Be Deleted"
        /*
        public Task<List<ChestCountData>> BuildAllChestCountDataAsync(SortType sortType = SortType.NONE) => 
            Task.FromResult(Task.Run(() => BuildAllChestCountData(SortType.NONE)).Result);

        public List<ChestCountData> BuildAllChestCountData(SortType sortType = SortType.NONE)
        {
            List<ChestCountData> chestcountdata = new List<ChestCountData>();

            //-- Start to initialize Chest Count Data.
            var index = 0;
            foreach (var clanmate in ClanManager.Instance.ClanmateManager.Database.Clanmates)
            {
                chestcountdata.Add(new ChestCountData(clanmate.Name, 0));
                index++;
            }
           
            foreach (var data in ClanChestDailyData)
            {
                var clanchestdata = data.Value;
                foreach (var chestcount in chestcountdata)
                {
                    var chestdata = clanchestdata.Where(name => name.Clanmate.Equals(chestcount.Clanmate)).ToList();

                    //--- found clanmate
                    if (chestdata.Count > 0)
                    {
                        var m_chestdata = chestdata[0];

                        if (m_chestdata.chests == null)
                        {
                            chestcount.Count += 0;
                        }
                        else
                        {
                            chestcount.Count += m_chestdata.chests.Count();
                        }
                    }
                }
            }


            ClanChestDataComparator clanChestDataComparator = new ClanChestDataComparator();
            List<ChestCountData> sortedChestCountData = new List<ChestCountData>();
            if (sortType == SortType.ASCENDING)
            {
                sortedChestCountData = chestcountdata.OrderBy(i => i.Count).ToList();
            }
            else if (sortType == SortType.DESENDING)
            {
                sortedChestCountData = chestcountdata.OrderByDescending(i => i.Count).ToList();
            }

            if (sortedChestCountData.Count > 0)
            {
                chestcountdata.Clear();
                chestcountdata = sortedChestCountData;
            }

            return chestcountdata;
        }

        public Task<List<ChestCountData>> BuildChestPointsDataAsync(SortType sortType = SortType.NONE) => 
            Task.FromResult(Task.Run(()=> BuildChestPointsData(sortType)).Result);

        public List<ChestCountData> BuildChestPointsData(SortType sortType = SortType.NONE)
        {
            List<ChestCountData> chestcountdata = new List<ChestCountData>();

            //-- Start to initialize Chest Count Data.
            foreach (var clanmate in ClanManager.Instance.ClanmateManager.Database.Clanmates)
            {
                chestcountdata.Add(new ChestCountData(clanmate.Name, 0));
            }

            foreach (var data in ClanChestDailyData)
            {
                var clanchestdata = data.Value;
                foreach (var chestcount in chestcountdata)
                {
                    var chestdata = clanchestdata.Where(name => name.Clanmate.Equals(chestcount.Clanmate)).ToList();

                    //--- found clanmate
                    if (chestdata.Count > 0)
                    {
                        var m_chestdata = chestdata[0];
                        if (m_chestdata.chests == null)
                        {
                            chestcount.Count += 0;
                        }
                        else
                        {
                            chestcount.Count += m_chestdata.Points;
                        }
                    }
                }
            }


            ClanChestDataComparator clanChestDataComparator = new ClanChestDataComparator();
            List<ChestCountData> sortedChestCountData = new List<ChestCountData>();
            if (sortType == SortType.ASCENDING)
            {
                sortedChestCountData = chestcountdata.OrderBy(i => i.Count).ToList();
            }
            else if (sortType == SortType.DESENDING)
            {
                sortedChestCountData = chestcountdata.OrderByDescending(i => i.Count).ToList();
            }

            if (sortedChestCountData.Count > 0)
            {
                chestcountdata.Clear();
                chestcountdata = sortedChestCountData;
            }

            return chestcountdata;
        }

        public Task<List<ChestCountData>> BuildSpecificChestCountDataAsync(SortType sortType = SortType.NONE)
        {
            return Task.FromResult(Task.Run(() => BuildSpecificChestCountData(sortType)).Result);
        }
        public List<ChestCountData> BuildSpecificChestCountData(SortType sortType = SortType.NONE)
        {
            List<ChestCountData> chestcountdata = new List<ChestCountData>();
            var requirements = ClanManager.Instance.ClanChestSettings.ClanRequirements.ClanSpecifiedRequirements;

            //-- Start to initialize Chest Count Data.
            var index = 0;
            foreach (var clanmate in ClanManager.Instance.ClanmateManager.Database.Clanmates)
            {
                chestcountdata.Add(new ChestCountData(clanmate.Name, 0));
                //-- initialize it first.
                foreach (var requirement in requirements)
                {
                    chestcountdata[index].ChestTypes.Add(new ChestTypeData(requirement.ChestType, 0));
                }
                index++;
            }

            foreach (var data in ClanChestDailyData)
            {
                var clanchestdata = data.Value;
                foreach (var chestcount in chestcountdata)
                {
                    var chestdata = clanchestdata.Where(name => name.Clanmate.Equals(chestcount.Clanmate)).ToList();

                    //--- found clanmate
                    if (chestdata.Count > 0)
                    {
                        var m_chestdata = chestdata[0];

                        if (m_chestdata.chests == null)
                        {
                            chestcount.Count += 0;
                        }
                        else
                        {
                            foreach (var requirement in requirements)
                            {
                                var name = requirement.ChestType;
                                var total = 0;

                                foreach (var chest in m_chestdata.chests)
                                {
                                    if (requirement.ChestType.ToLower().Equals(chest.Type.ToString().ToLower()))
                                    {
                                        name = requirement.ChestType;
                                        total += 1;
                                    }
                                }

                                for (var i = 0; i < chestcount.ChestTypes.Count; i++)
                                {
                                    if (chestcount.ChestTypes[i].Chest.Equals(name))
                                    {
                                        chestcount.ChestTypes[i].Total += total;
                                        break;
                                    }
                                }
                            }
                        }
                        
                        var chests_total = 0;
                        foreach (var chesttypes in chestcount.ChestTypes)
                        {
                            chests_total += chesttypes.Total;
                        }
                        chestcount.Count = chests_total;
                    }
                }
            }
      

            ClanChestDataComparator clanChestDataComparator = new ClanChestDataComparator();
            List<ChestCountData> sortedChestCountData = new List<ChestCountData>();
            if (sortType == SortType.ASCENDING)
            {
                sortedChestCountData = chestcountdata.OrderBy(i => i.Count).ToList();
            }
            else if (sortType == SortType.DESENDING)
            {
                sortedChestCountData = chestcountdata.OrderByDescending(i => i.Count).ToList();
            }

            if (sortedChestCountData.Count > 0)
            {
                chestcountdata.Clear();
                chestcountdata = sortedChestCountData;
            }

            return chestcountdata;
        }

        public double GetPercentageOfClanchestsfromClan(List<ChestCountData> chestcount)
        {
            //-- add clan total percentage 
            var num_clanmates = ClanManager.Instance.ClanmateManager.Database.Clanmates.Count;
            var num_clanmates_gifts = 0;
            foreach (var chest in chestcount)
            {
                if (chest.Count > 0)
                {
                    num_clanmates_gifts += 1;
                }
            }
             
            return ((double)num_clanmates_gifts / num_clanmates * 100.0);
        }

        public void ExportData(string filename,  List<ChestCountData> chestcountdata, int chest_points_value = 0, int count_method = 0)
        {
            if(!String.IsNullOrEmpty(filename)) 
            {
                using (StreamWriter sw = File.CreateText(filename))
                {
                    //-- write Header
                    string date = DateTime.Now.ToString(@"MM/dd/yyyy");
                    var header = $"{date} Chest Count\r\n";
                    header += $"{new string('-', 75)}";
                    sw.WriteLine(header);

                    //-- Write Percentage
                    var clanmates_chests_percent = GetPercentageOfClanchestsfromClan(chestcountdata);
                    var statistics_message = $"Percentage of clan actively hunting clan chests: {clanmates_chests_percent:0.##}%\r\n";
                    sw.WriteLine(statistics_message);

                    //-- no longer use Table, looks extremely bad in game when posted.
                    string column = $"Clanmate".PadRight(25) + $"Total".PadLeft(5);
                    string line = new string('-', 75);
                    sw.WriteLine($"{new string('-', 75)}\r\n{column}\r\n{line}\r\n");
                    foreach (var chestcount in chestcountdata)
                    {
                        //--- 180 chests bug. Game doesn't like 180 number posted. Not sure why.
                        if (count_method == 0)
                        {
                            var chests = chestcount.Count == 180 ? chestcount.Count + 1 : chestcount.Count;
                            if (chest_points_value > 0 && chests > 0)
                                chests += chest_points_value;

                            var content = "";
                            content += $"{chestcount.Clanmate.PadRight(23, ' ')} {chests.ToString().PadLeft(5, ' ')}";

                            sw.WriteLine(content);
                        }
                        else
                        {
                            var chests = chestcount.Count == 180 ? chestcount.Count + 1 : chestcount.Count;
                            if (chest_points_value > 0 && chests > 0)
                                chests += chest_points_value;
                        }
                    }
                    sw.Close();
                }
            }
        }
        */
        #endregion

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
