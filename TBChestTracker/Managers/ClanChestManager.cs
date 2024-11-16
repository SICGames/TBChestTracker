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
    */
    [System.Serializable]
    public class ClanChestManager : IDisposable
    {
        #region Declarations
        public IList<ClanChestData> clanChestData { get; set; }
        public Dictionary<string, IList<ClanChestData>> ClanChestDailyData;
        private ChestProcessingState pChestProcessingState = ChestProcessingState.IDLE;
        bool filteringErrorOccurred = false;
        string lastFilterString = String.Empty;
        public ChestRewardsManager ChestRewards { get; private set; }

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
            ClanChestDailyData = new Dictionary<string, IList<ClanChestData>>();
            ChestProcessingState = ChestProcessingState.IDLE;
            ChestRewards = new ChestRewardsManager();

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
            var increment = 3;
            var chestboxes = new List<ChestBox>();
            var clicks = SettingsManager.Instance.Settings.AutomationSettings.AutomationClicks;

            var tmpResult = new List<string>();

            for (var b = 0; b < result.Count; b++)
            {
                ChestBox cb = new ChestBox();

                //-- 4 clicks 
                //-- 3 lines each box
                //-- expired chest gives 4 lines.
                for(var a = b; a < result.Count; a++)
                {
                    tmpResult.Add(result[a]);
                }

                var bContainsIndex = tmpResult.FindIndex(r => r.StartsWith("Contains:"));
                var _inc = bContainsIndex > -1 ? 4 : 3;
                for(var c = 0; c < _inc; c++)
                {
                    var w = tmpResult[c];
                    cb.Content.Add(w);
                }
                
                tmpResult.Clear();

                b += cb.Content.Count - 1;

                chestboxes.Add(cb);
            }

            tmpResult.Clear();
            tmpResult = null;

            foreach (var chestbox in chestboxes)
            {
                for (var x = 0; x < chestbox.Content.Count; x += chestbox.Content.Count)
                {
                    var word = chestbox.Content[x];

                    var bHasContains = !String.IsNullOrEmpty(chestbox.Content.Where(s => s.StartsWith("Contains:")).FirstOrDefault());
                    if (bHasContains)
                    {
                        Debug.WriteLine($"Expired Chest detected.");
                    }

                    if (word == null)
                        break;


                    if (!word.Contains(TBChestTracker.Resources.Strings.Clan))
                    {
                        var chestName = "";
                        var clanmate = "";
                        var chestobtained = "";
                        var chestcontains = "";
                        try
                        {
                            chestName = chestbox.Content[x + 0];
                            clanmate = chestbox.Content[x + 1];
                            chestobtained = chestbox.Content[x + 2];

                            if (bHasContains)
                            {
                                chestcontains = chestbox.Content[x + 3];
                            }

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
                                    if (fromStartingPos >= 0)
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
                        int level = 0;
                        var ChestType = String.Empty;
                        var ChestSource = String.Empty;
                        var ChestReward = String.Empty;

                        if (chestobtained.Contains(TBChestTracker.Resources.Strings.Source))
                        {
                            //-- foreign language prevent speeding this up.
                            //-- Spanish - Cripta de nivel 10 translate to english Level 10 Crypt.
                            //-- First approach was extract type of crypt and phase out chesttype completely. 
                            //-- Source: Cripta de 
                            //-- Level of crypt: 10

                            ChestSource = chestobtained.Substring(chestobtained.IndexOf(":") + 2);
                            var levelStartPos = -1;

                            if (ChestSource.Contains(TBChestTracker.Resources.Strings.Level))
                            {
                                levelStartPos = ChestSource.IndexOf(TBChestTracker.Resources.Strings.Level);
                            }
                            else if (ChestSource.Contains(TBChestTracker.Resources.Strings.lvl))
                            {
                                levelStartPos = ChestSource.IndexOf(TBChestTracker.Resources.Strings.lvl);
                            }


                            //-- level in en-US is 0 position.
                            //-- level in es-ES is 11 position.
                            //-- crypt in en-US is 9 position 
                            //-- crypt in es-ES is 0 position.

                            //-- we can check direction of level position.
                            //-- if more than 1 then we know we should go backwares. If 0 then we know to go forwards.
                            var levelFullLength = 0;


                            if (levelStartPos > -1)
                            {
                                var levelStr = ChestSource.Substring(levelStartPos).ToLower();
                                var levelNumberStr = levelStr.Substring(levelStr.IndexOf(" ") + 1);

                                //-- using a quantifer to check if there is an additional space after the level number. If user is spanish, no space after level number.
                                levelNumberStr = levelNumberStr.IndexOf(" ") > 0 ? levelNumberStr.Substring(0, levelNumberStr.IndexOf(" ")) : levelNumberStr;
                                var levelFullStr = String.Empty;
                                if (ChestSource.Contains(TBChestTracker.Resources.Strings.Level))
                                {
                                    levelFullStr = $"{TBChestTracker.Resources.Strings.Level} {levelNumberStr}";
                                }
                                else if (ChestSource.Contains(TBChestTracker.Resources.Strings.lvl))
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
                                if (level == 0)
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
                                if (ChestType.StartsWith(TBChestTracker.Resources.Strings.OnlyCrypt))
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


                        }

                        if (chestcontains.Contains("Contains"))
                        {
                            ChestReward = chestcontains.Substring(chestcontains.IndexOf(":") + 2);
                        }

                        tmpchests.Add(new ChestData(clanmate, new Chest(chestName, ChestType, ChestSource, level)));

                        if (bHasContains)
                        {
                            ChestRewards.Add(ChestType, level, ChestReward);
                        }

                        var dbg_msg = String.Empty;
                        var reward_msg = String.Empty;
                        if (String.IsNullOrEmpty(ChestReward) == false)
                        {
                            reward_msg = $" that contained chest rewards => {ChestReward}";
                        }

                        if (level != 0)
                        {
                            dbg_msg = $"--- ADDING level {level} {ChestType.ToString()}  '{chestName}' from {clanmate} {reward_msg} ----";
                        }
                        else
                        {
                            dbg_msg = $"--- ADDING {ChestType.ToString()}  '{chestName}' from {clanmate} {reward_msg} ----";
                        }

                        com.HellStormGames.Logging.Console.Write(dbg_msg, "OCR Result", com.HellStormGames.Logging.LogType.INFO);
                    }
                }
            }

            chestboxes.Clear();
            chestboxes = null;  

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
            var inputLength = input.Length; 
            IList<Clanmate> clanmates = ClanManager.Instance.ClanmateManager.Database.Clanmates.ToList();
            bool bMatch = false;
            
            Clanmate matchedClanmate = null;

            var jw = new F23.StringSimilarity.JaroWinkler();
            
            Parallel.ForEach(clanmates, (clanmate,state) => 
            {
                var similarity = jw.Similarity(clanmate.Name, input) * 100.0;
                Debug.WriteLine($"{clanmate.Name} and {input} have a Similiarity Percent => {similarity}%");
                if(similarity > similiarityThreshold)
                {
                    bMatch = true;
                    matchedClanmate = clanmate;
                    state.Break();
                }
            });
            if (bMatch)
            {
                return matchedClanmate;
            }

            return null;
        }

        public async Task ProcessChestData(List<string> result, ChestAutomation chestAutomation)
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
            IList<ClanChestData> tmpchestdata = CreateClanChestData(tmpchests);
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
            Parallel.ForEach(tmpchestdata.ToList(), tmpchest =>  //foreach (var tmpchest in tmpchestdata.ToList())
             {
                 bool exists = clanmates.Select(mate_name => mate_name.Name).Contains(tmpchest.Clanmate, StringComparer.CurrentCultureIgnoreCase);
                 if(exists)
                 {
                     mate_index++;
                     return;
                 }

                 if (!exists)
                 {
                     var tClanmate = tmpchest.Clanmate;
                     var match_clanmate = Clanmate_Scan(tClanmate, SettingsManager.Instance.Settings.OCRSettings.ClanmateSimilarity);
                     if (match_clanmate != null)
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
             });

            //-- update clanchestdata
            Parallel.ForEach(clanChestData, chestdata => //foreach (var chestdata in clanChestData)
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

                        foreach(var m_chest in m_chestdata.chests.ToList()) //foreach (var m_chest in m_chestdata.chests.ToList())
                         {
                             if (ClanManager.Instance.ClanChestSettings.GeneralClanSettings.ChestOptions == ChestOptions.UsePoints)
                             {
                                 var pointvalues = ClanManager.Instance.ClanChestSettings.ChestPointsSettings.ChestPoints;
                                 foreach (var pointvalue in pointvalues)
                                 {
                                     var chest_type = m_chest.Type.ToString();
                                     var chest_name = m_chest.Name.ToString();

                                     var level = m_chest.Level;
                                     //Debug.WriteLine($" Chest Type is -> {chest_type} and Point Value Chest Type is -> {pointvalue.ChestType}");

                                     //if (pointvalue.ChestType.ToLower().Contains(chest_type.ToLower().ToString()))
                                     if (chest_type.ToLower().Contains(pointvalue.ChestType.ToLower()))
                                     {
                                         //Debug.WriteLine($"Chest name in Points -> {pointvalue.ChestName}");

                                         if (pointvalue.Level.Equals("(Any)") == false)
                                         {
                                             // Debug.WriteLine($"Chest Level in Points -> {Int32.Parse(pointvalue.Level.ToString())} and Chest Level -> {level}");
                                         }
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
                                             // Debug.WriteLine($"Chest name in Points -> {pointvalue.ChestName}");
                                             if (pointvalue.Level.Equals("(Any)") == false)
                                             {
                                                 //   Debug.WriteLine($"Chest Level in Points -> {Int32.Parse(pointvalue.Level.ToString())} and Chest Level -> {level}");
                                             }

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
            });

            var datestr = DateTime.Now.ToString("d", new CultureInfo(CultureInfo.CurrentCulture.Name));   
            ClanChestDailyData[datestr] = clanChestData;
            ChestProcessingState = ChestProcessingState.COMPLETED;
           
            AppContext.Instance.isBusyProcessingClanchests = false;

            Automation.AutomationChestProcessedEventArguments args = new AutomationChestProcessedEventArguments(new ClanChestProcessResult("200", 200, ClanChestProcessEnum.SUCCESS));
            chestAutomation.InvokeChestProcessed(args);

        }

        //-- RepairChestData returns false if nothing needs to be done. Returns true if errors exist.
        public IntegrityResult DoesChestDataNeedsRepairs()
        {
            int chestErrors = 0;
            IntegrityResult result = new IntegrityResult();

            var chestsettings = ClanManager.Instance.ClanChestSettings;
            var chestdata = ClanManager.Instance.ClanChestManager.ClanChestDailyData;
            var df = SettingsManager.Instance.Settings.GeneralSettings.DateFormat;

            var chestpointsvalues = ClanManager.Instance.ClanChestSettings.ChestPointsSettings.ChestPoints;
            if (chestpointsvalues == null || chestpointsvalues.Count == 0)
            {

            }

            var dateformat = SettingsManager.Instance.Settings.GeneralSettings.DateFormat;
            var currentCulture = CultureInfo.CurrentCulture; //-- en-GB
            var uiCulture = CultureInfo.CurrentUICulture; //-- en-US

            bool invalidateDates = DoesDatesNeedRepair();
            if (invalidateDates)
            {
                result.Add("Invalid Locale Date Format Detected", chestErrors);
                chestErrors++;
            }

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
                        if(result != null)
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

        public bool RepairChestData()
        {
            int chestErrors = 0;

            var chestsettings = ClanManager.Instance.ClanChestSettings;
            var chestdata = ClanManager.Instance.ClanChestManager.ClanChestDailyData;
            var cultureUI = CultureInfo.CurrentUICulture;
            var currentCulture = CultureInfo.CurrentCulture;

            var chestpointsvalues = ClanManager.Instance.ClanChestSettings.ChestPointsSettings.ChestPoints;
            if (chestpointsvalues == null || chestpointsvalues.Count == 0)
            {

            }

            var df = SettingsManager.Instance.Settings.GeneralSettings.DateFormat;

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
                            if(chest == null)
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
                SaveData();
                CreateBackup();
                return true;
            }
            return false;
        }

        public bool LoadBackup(string file)
        {
            ClanChestDailyData.Clear();
            Dictionary<string, IList<ClanChestData>> _clanchestdailydata = new Dictionary<string, IList<ClanChestData>>();
            if(JsonHelper.TryLoad(file, out _clanchestdailydata))
            {
                ClanChestDailyData = _clanchestdailydata;
                return true;
            }
            else
            {
                return false;
            }
        }


        private async Task<bool> LoadClanChestData(string file)
        {
            Dictionary<string, IList<ClanChestData>> _clanchestdailydata = new Dictionary<string, IList<ClanChestData>>();
            if (JsonHelper.TryLoad(file, out _clanchestdailydata))
            {
                ClanChestDailyData = _clanchestdailydata;
                return true;
            }
            else
            {
                return false;
            }
        }
        public async Task<bool> LoadData()
        {
            var rootFolder = $"{SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder}";
            var clanFolder = $"{rootFolder}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath}";
            var databaseFolder = $"{clanFolder}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanDatabaseFolder}";
            var clanmatefile = $"{clanFolder}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanmateDatabaseFile}";
            var clanchestfile = $"{clanFolder}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanChestDatabaseFile}";
            var chestsettingsfile = $"{clanFolder}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanSettingsFile}";

            var chestrewardsFile = $@"{databaseFolder}\ChestRewards.db";

            bool clanchestcorrupted = false;


            if (ClanManager.Instance.ClanmateManager.Database.Clanmates != null)
                ClanManager.Instance.ClanmateManager.Database.Clanmates.Clear();

            if (!System.IO.File.Exists(clanmatefile))
            {
                AppContext.Instance.ClanmatesBeenAdded = false;
                return false;
            }
            else
            {
                if(!ClanManager.Instance.ClanmateManager.Load(clanmatefile))
                {
                    //-- there's a problem loading clanmate file.
                    com.HellStormGames.Logging.Console.Write($@"Failed to load clanmate database file.", "Load Issue", LogType.ERROR);
                }
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
                bool result = await LoadClanChestData(clanchestfile); 
                if(result == false) 
                {
                    //-- we need to let them know there's a possible corrupt clanchestdatabase file
                    //-- we caught the exception. User needs to restore. Need a way to force them. 
                    AppContext.Instance.IsClanChestDataCorrupted = true;
                    clanchestcorrupted = true;
                    com.HellStormGames.Logging.Console.Write($@"Clan Chest Database possibly corrupted.", "Clan Chest Database Error", LogType.ERROR);
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

                if(!ClanManager.Instance.ClanChestSettings.LoadSettings(chestsettingsfile))
                {
                    //-- problem loading clan chest settings.
                    com.HellStormGames.Logging.Console.Write($@"Failed to load clan settings file.", "Load Issue", LogType.ERROR);
                }
            }

            if (System.IO.File.Exists(chestrewardsFile))
            {
                if(ChestRewards.Load() == false)
                {
                    //-- problem loading chest rewards 
                    com.HellStormGames.Logging.Console.Write($@"Failed to load chest rewards file.", "Load Issue", LogType.ERROR);
                }
            }

            if(clanchestcorrupted)
            {
                return false;
            }

            return true;
        }

        public enum ChestDataBuildResult
        {
            OK = 0,
            LOAD_FAIL = 1,
            DATA_CORRUPT = 2
        }
     
        private bool hasDuplicatedDate(DateTime date, string dateformat)
        {
            //-- maybe possible duplicate.
            //-- make bad date and turn into good date and check for duplicates.
            var badDateStr = date;

            var goodDateTimestamp = date.ConvertToUnixTimeStamp();
            var goodDate = goodDateTimestamp.ConvertToDateTime();

            var bContainsDuplicate = ClanChestDailyData.ContainsKey(goodDate.ToString(dateformat));
            if (bContainsDuplicate == true)
            {
                return true;
            }

            return false;
        }

        public bool DoesDatesNeedRepair()
        {
            var currentCulture = CultureInfo.CurrentCulture; //-- en-GB
                                                             //-- attempting to repair date.
            var dates = new List<string>();
            foreach (var date_key in ClanChestDailyData.Keys.ToList())
            {
                dates.Add(date_key);

                //ClanChestDailyData.UpdateKey<string, IList<ClanChestData>>(date_key, dateformat);
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
                    var newLocaleDate = date_Created?.ToString("d", currentCulture);
                    result = date.Equals(newLocaleDate);
                }
                catch (Exception ex)
                {

                }
            }

            return !result; //-- returning the opposite. If it matches then we return false. If it does not match, we return true.
        }

        public ChestDataBuildResult BuildData()
        {
            var result = LoadData().Result;
            var dateformat = SettingsManager.Instance.Settings.GeneralSettings.DateFormat;
            var currentCulture = CultureInfo.CurrentCulture; //-- en-GB
            bool invalidDateFormat = false;

            if(result == false)
            {
                MessageBox.Show("Unable to build clan data. Something went horribly wrong.", "Building Clan Data Failed");
                return ChestDataBuildResult.LOAD_FAIL;
            }

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
                var dateStr = DateTime.Now.ToString(dateformat, new CultureInfo(CultureInfo.CurrentCulture.Name));

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

            if(invalidDateFormat)
            {
                return ChestDataBuildResult.DATA_CORRUPT;
            }

            AppContext.Instance.ClanmatesBeenAdded = true;

            return ChestDataBuildResult.OK;
        }
        
        public void SaveData(string chestdatafile = "")
        {
            //-- write to file.
            string file = String.Empty;
            var root = $"{SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder}";
            var clanfolder = $"{root}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath}";
            var databaseFolder = $"{clanfolder}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanDatabaseFolder}\\";
            if (String.IsNullOrEmpty(chestdatafile))
                file = $"{clanfolder}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanChestDatabaseFile}";
            else
                file = $"{databaseFolder}{chestdatafile}";

            try
            {
                using (var sw = File.CreateText(file))
                {
                    JsonSerializer serialize = new JsonSerializer();
                    serialize.Formatting = Formatting.Indented;
                    serialize.Serialize(sw, ClanChestDailyData);
                    sw.Close();
                    serialize = null;
                }
                if (ChestRewards.ChestRewardsList.Count > 0)
                {
                    ChestRewards.Save();
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

            var root = $"{SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder}";
            var clanfolder = $"{root}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath}";


            var clanchestsBackupFolder = $"{clanfolder}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanDatabaseBackupFolderPath}//Clanchests";
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

        public Dictionary<string, IList<ClanChestData>> FilterClanChestByConditions()
        {
            var dailyChests = new Dictionary<string, IList<ClanChestData>>();
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

        public void Dispose()
        {
            ChestRewards.Dispose();
            ClanChestDailyData.Clear();
            ClanChestDailyData.Clear();
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
