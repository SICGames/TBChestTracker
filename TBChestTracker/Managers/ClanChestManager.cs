﻿using com.HellStormGames.Logging;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Markup;
using TBChestTracker.Managers;
using Windows.Media.Ocr;
using Windows.Networking.Proximity;

namespace TBChestTracker
{

    public class ClanChestManager
    {
        public List<ClanChestData> clanChestData { get; set; }
        public Dictionary<String, List<ClanChestData>> ClanChestDailyData;
        private ChestProcessingState pChestProcessingState = ChestProcessingState.IDLE;

        public ChestProcessingState ChestProcessingState
        {
            get => pChestProcessingState;
            set => pChestProcessingState = value;   
        }

        public ClanChestManager()
        {

            clanChestData = new List<ClanChestData>();  
            ClanChestDailyData = new Dictionary<string, List<ClanChestData>>();
            ChestProcessingState = ChestProcessingState.IDLE;
        }
        public void ClearData()
        {
            ClanChestDailyData.Clear();
            clanChestData.Clear();
            ClanManager.Instance.ClanmateManager.Database.Clanmates.Clear();
        }

        public void RemoveChestData(string clanmatename)
        {
            foreach(var date in ClanChestDailyData.ToList())
            {
                var chestdata = date.Value;
                foreach(var data in  chestdata.ToList())
                {
                    if(data.Clanmate.Equals(clanmatename, StringComparison.CurrentCultureIgnoreCase)) 
                    {
                        var name = data.Clanmate;
                        chestdata.Remove(data);
                        com.HellStormGames.Logging.Console.Write($"{name} was successfully removed from clanchestdata.","Clanmate Removal", com.HellStormGames.Logging.LogType.INFO);
                    }
                }
            }
            //--- we're done.
            SaveData();
            return;
        }

        private void FilterChestData(ref List<String> data, String[] words)
        {
            var filtered = data.Where(d => ContainsAny(d, words));
            data = filtered.ToList();
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
                //-- if some how we came to this point, junk data exists.
                var dbg_msg = $"---- '{str}' from OCR results doesn't match specified values.";
                com.HellStormGames.Logging.Console.Write(dbg_msg, "Invalid OCR", com.HellStormGames.Logging.LogType.INFO);
            }
            return false;
        }
        //-- ProcessChestData optimize chest data
        private List<ChestData> ProcessText2(List<String> result)
        {
            //---Processing Chests Took: 00:00:00.0115509

            List<ChestData> tmpchests = new List<ChestData>();

            //-- quick filter. 
            //-- in Triumph Chests, divider creates dirty characters. 
            var filter_words = new string[] { "Chest", "From:", "Source:", "Gift" };

            FilterChestData(ref result, filter_words);

            for (var x = 0; x < result.Count; x += 3)
            {
                var word = result[x];

                if (word == null)
                    break;

                if (!word.Contains("Clan"))
                {
                    var chestName = result[x + 0];
                    var clanmate = result[x + 1];
                    var chestobtained = result[x + 2];

                    com.HellStormGames.Logging.Console.Write($"OCR RESULT [{chestName}, {clanmate}, {chestobtained}", "OCR Result", LogType.INFO);

                    if (clanmate.Contains("From:"))
                    {
                        //-- OCR issue converts clan name: ADİĞE CALE
                        //-- into ADiöE CALE
                        clanmate = clanmate.Substring(clanmate.IndexOf(" ") + 1);
                        if (clanmate.Contains("From:"))
                        {
                            //-- error - shouldn't even have reached this point.
                            //-- game actually causes this error from not rendering name fast enough 
                            //-- hasbeen patched but throw exception just in case.
                            var badname = 0;
                            throw new Exception("Clanmate name is blank. Increase thread sleep timer to prevent this.");
                        }
                    }

                    //-- building tmpchestdata
                    if (chestobtained.Contains("Source:"))
                    {
                        var levelStartPos = chestobtained.IndexOf("Level");
                        ChestType type = ChestType.COMMON;

                        if (levelStartPos > 0)
                        {
                            chestobtained = chestobtained.Substring(levelStartPos).ToLower();
                            type = ChestType.COMMON;
                            if (chestobtained.Contains("epic"))
                                type = ChestType.EPIC;
                            else if (chestobtained.Contains("rare"))
                                type = ChestType.RARE;
                            else if (chestobtained.Contains("heroic"))
                                type = ChestType.HEROIC;
                            else if (chestobtained.Contains("citadel"))
                                type = ChestType.CITADEL;

                            int level = 0;

                            if (chestobtained.Contains("io"))
                            {
                                //--- OCR reads 10 as io.Until perm fix. 
                                level = 10;
                            }

                            if (chestobtained.Contains("-"))
                            {
                                string ancientlevel = chestobtained.Substring(chestobtained.IndexOf("-") - 2);
                                ancientlevel = ancientlevel.Substring(0, ancientlevel.IndexOf(" "));
                                var levels = ancientlevel.Split('-');
                                type = ChestType.VAULT;
                                level = Int32.Parse(levels[1]);
                            }
                            else
                            {
                                Regex r = new Regex(@"(\d+)", RegexOptions.Singleline); // new Regex(@"(\d+)(.?|\s)(\d+)");
                                var match = r.Match(chestobtained);
                                if (match.Success)
                                {
                                    level = Int32.Parse(match.Value);
                                }
                            }

                            //--- shouldn't be 0. Normally happens 1st chest that is a level 5. 
                            if (level == 0)
                                level = 5;

                            //-- filter chest if chest conditions enabled
                            //--- if chest requirements are using conditions, we use those conditions. If not, we continue.
                            if (ClanManager.Instance.ClanChestSettings.ChestRequirements.useChestConditions)
                            {
                                foreach (var condition in ClanManager.Instance.ClanChestSettings.ChestRequirements.ChestConditions)
                                {
                                    var typeStr = type.ToString().ToLower();

                                    if (typeStr.Equals(condition.ChestType, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        if (level >= condition.level)
                                        {
                                            tmpchests.Add(new ChestData(clanmate, new Chest(chestName, type, level)));
                                            var dbgmsg = $"[[Level {level} ({type.ToString()}) {chestName} from {clanmate} validated.]]";
                                            com.HellStormGames.Logging.Console.Write(dbgmsg, "OCR Result", com.HellStormGames.Logging.LogType.INFO);
                                        }
                                    }
                                }
                            }
                            else
                            {

                                tmpchests.Add(new ChestData(clanmate, new Chest(chestName, type, level)));
                                var dbg_msg = $"--- ADDING level {level} {type.ToString()}  '{chestName}' from {clanmate} ----";
                                com.HellStormGames.Logging.Console.Write(dbg_msg, "OCR Result", com.HellStormGames.Logging.LogType.INFO);
                            }
                        }
                        else
                        {
                            chestobtained = chestobtained.ToLower();
                            type = ChestType.OTHER;
                            if (chestobtained.Contains("arena"))
                                type = ChestType.ARENA;
                            else if (chestobtained.Contains("bank"))
                                type = ChestType.BANK;
                            else if (chestobtained.Contains("union"))
                                type = ChestType.UNION_TRIUMPH;
                            else if (chestobtained.Contains("mimic"))
                                type = ChestType.MIMIC;
                            else if (chestobtained.Contains("rise"))
                                type = ChestType.ANCIENT_EVENT;
                            else if (chestobtained.Contains("story"))
                                type = ChestType.STORY;
                            else if (chestobtained.Contains("wealth"))
                                type = ChestType.WEALTH;

                            if (ClanManager.Instance.ClanChestSettings.ChestRequirements.useChestConditions)
                            {
                                foreach (var condition in ClanManager.Instance.ClanChestSettings.ChestRequirements.ChestConditions)
                                {
                                    var typeStr = type.ToString().ToLower();

                                    if (typeStr.Equals(condition.ChestType, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        tmpchests.Add(new ChestData(clanmate, new Chest(chestName, type, 0)));
                                        var dbgmsg = $"[[({type.ToString()}) {chestName} from {clanmate} validated.]]";
                                        com.HellStormGames.Logging.Console.Write(dbgmsg, "OCR Result", com.HellStormGames.Logging.LogType.INFO);   
                                    }
                                }
                            }
                            else
                            {
                                tmpchests.Add(new ChestData(clanmate, new Chest(chestName, type, 0)));
                                var dbgmsg = $"--- ADDING {type.ToString()}  '{chestName}' from {clanmate} ----";
                                com.HellStormGames.Logging.Console.Write (dbgmsg, "OCR Result", LogType.INFO);  
                            }
                        }
                    }
                }
            }
            return tmpchests;
        }

        #region ProcessText -- OBSOLETE
        [Obsolete("Use ProcessText2 instead.")]
        private List<ChestData> ProcessText(List<String> result)
        {

            //-- Processing Chests Took: 00:00:00.0147902

            List<ChestData> tmpchests = new List<ChestData>();

            //-- quick filter. 
            //-- in Triumph Chests, divider creates dirty characters. 
            foreach(var data in result.ToList())
            {
                bool bExists = data.Contains("Chest") ||
                        data.Contains("From:") ||
                    data.Contains("Source:") ||
                    data.Contains("gift");

                if (!bExists)
                {
                    var dbg_msg = $"---- Successfully removed '{data}' invalid data from OCR results.";
                    com.HellStormGames.Logging.Console.Write(dbg_msg, com.HellStormGames.Logging.LogType.INFO);  
                    result.Remove(data);
                }
            }
            
            for (var x = 0; x < result.Count; x += 3)
            {
                var word = result[x];

                if (word == null)
                    break;

                if (!word.Contains("Clan"))
                {

                    var chestName = result[x + 0];
                    var clanmate = result[x + 1];
                    var chestobtained = result[x + 2];

                    if (clanmate.Contains("From:"))
                    {
                        //-- OCR issue converts clan name: ADİĞE CALE
                        //-- into ADiöE CALE
                        clanmate = clanmate.Substring(clanmate.IndexOf(" ") + 1);
                        if (clanmate.Contains("From:"))
                        {
                            //-- error - shouldn't even have reached this point.
                            //-- game actually causes this error from not rendering name fast enough 
                            //-- hasbeen patched but throw exception just in case.
                            var badname = 0;
                            throw new Exception("Clanmate name is blank. Increase thread sleep timer to prevent this.");
                        }
                    }

                    //-- building tmpchestdata
                    if (chestobtained.Contains("Source:"))
                    {
                        var levelStartPos = chestobtained.IndexOf("Level");
                        ChestType type = ChestType.COMMON;

                        if (levelStartPos > 0)
                        {
                            chestobtained = chestobtained.Substring(levelStartPos).ToLower();
                            type = ChestType.COMMON;
                            if (chestobtained.Contains("epic"))
                                type = ChestType.EPIC;
                            else if (chestobtained.Contains("rare"))
                                type = ChestType.RARE;
                            else if (chestobtained.Contains("heroic"))
                                type = ChestType.HEROIC;
                            else if (chestobtained.Contains("citadel"))
                                type = ChestType.CITADEL;

                            int level = 0;

                            if (chestobtained.Contains("io"))
                            {
                                //--- OCR reads 10 as io.Until perm fix. 
                                level = 10;
                            }

                            if (chestobtained.Contains("-"))
                            {
                                string ancientlevel = chestobtained.Substring(chestobtained.IndexOf("-") - 2);
                                ancientlevel = ancientlevel.Substring(0, ancientlevel.IndexOf(" "));
                                var levels = ancientlevel.Split('-');
                                type = ChestType.VAULT;
                                level = Int32.Parse(levels[1]);
                            }
                            else
                            {
                                Regex r = new Regex(@"(\d+)", RegexOptions.Singleline); // new Regex(@"(\d+)(.?|\s)(\d+)");
                                var match = r.Match(chestobtained);
                                if (match.Success)
                                {
                                    level = Int32.Parse(match.Value);
                                }
                            }

                            //--- shouldn't be 0. Normally happens 1st chest that is a level 5. 
                            if (level == 0)
                                level = 5;

                            tmpchests.Add(new ChestData(clanmate, new Chest(chestName, type, level)));
                            var dbg_msg = $"--- ADDING level {level} {type.ToString()}  '{chestName}' from {clanmate} ----";
                            com.HellStormGames.Logging.Console.Write(dbg_msg, com.HellStormGames.Logging.LogType.INFO);  
                        }
                        else
                        {
                            chestobtained = chestobtained.ToLower();
                            type = ChestType.OTHER;

                            if (chestobtained.Contains("arena"))
                                type = ChestType.ARENA;
                            else if (chestobtained.Contains("bank"))
                                type = ChestType.BANK;
                            else if (chestobtained.Contains("union"))
                                type = ChestType.UNION_TRIUMPH;
                            else if (chestobtained.Contains("mimic"))
                                type = ChestType.MIMIC;
                            else if (chestobtained.Contains("rise"))
                                type = ChestType.ANCIENT_EVENT;
                            else if (chestobtained.Contains("story"))
                                type = ChestType.STORY;
                            else if (chestobtained.Contains("wealth"))
                                type = ChestType.WEALTH;
                            
                            var dbg_msg = $"--- ADDING ({chestName}) {type.ToString()} Chest from {clanmate} ----";
                            tmpchests.Add(new ChestData(clanmate, new Chest(chestName, type, 0)));
                            com.HellStormGames.Logging.Console.Write(dbg_msg, com.HellStormGames.Logging.LogType.INFO);
                        }
                    }
                }
            }

            return tmpchests;
        }
        #endregion

        #region ProcessChestConditions -- OBSOLETE 
        [Obsolete]
        private void ProcessChestConditions(ref List<ChestData> chestdata)
        {
            //--- if chest requirements are using conditions, we use those conditions. If not, we continue.
            if (ClanManager.Instance.ClanChestSettings.ChestRequirements.useChestConditions)
            {
                List<ChestData> validated = new List<ChestData>();

                foreach (var ttmpChest in chestdata.ToList())
                {
                    var isValidated = false;
                    foreach (var condition in ClanManager.Instance.ClanChestSettings.ChestRequirements.ChestConditions)
                    {
                        var sChestType = ttmpChest.Chest.Type.ToString().ToLower();

                        if (sChestType.Equals(condition.ChestType, StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (condition.Comparator.ToLower() == "greater than")
                            {
                                if (ttmpChest.Chest.Level >= condition.level)
                                {
                                    validated.Add(new ChestData(ttmpChest.Clanmate, ttmpChest.Chest));
                                    var dbg_msg = $"[[Level {ttmpChest.Chest.Level} ({ttmpChest.Chest.Type.ToString()}) {ttmpChest.Chest.Name} from {ttmpChest.Clanmate} validated.]]";
                                    com.HellStormGames.Logging.Console.Write(dbg_msg, com.HellStormGames.Logging.LogType.INFO);
                                    isValidated = true;
                                }
                            }
                        }
                        if (isValidated)
                            break;
                    }
                }
                if (validated.Count > 0)
                {
                    chestdata.Clear();
                    chestdata = validated;
                }
            }
        }
        #endregion

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

        public async void ProcessChestData(List<string> result)
        {

            /*
             * TmpChests gather processed chests.
             * Stuffs into temporary clanchestdata
             * Need to make sure we don't need to start new entry in ClanChestDaily.
             * ClanChestData should be obsolete.
            */
            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();

            GlobalDeclarations.isBusyProcessingClanchests = true;
            var resulttext = result;
            
            //- doesn't handle expired gifts. Will patch soon.
            if (!resulttext[0].Contains("No gifts"))
            {
                ChestProcessingState = ChestProcessingState.PROCESSING;
                GlobalDeclarations.isAnyGiftsAvailable = true;
                
                List<ChestData> tmpchests = ProcessText2(resulttext);
                List<ClanChestData> tmpchestdata = CreateClanChestData(tmpchests);

                var clanmates = ClanManager.Instance.ClanmateManager.Database.Clanmates.ToList();

                //-- do we need to insert new entry?
                //-- causes midnight bug.
                var currentdate = DateTime.Now.ToString(@"MM-dd-yyyy");
                var dates = ClanChestDailyData.Where(x => x.Key.Equals(currentdate));

                if (dates.Count() == 0)
                {
                    clanChestData.Clear();
                    foreach (var mate in clanmates)
                    {
                        clanChestData.Add(new ClanChestData(mate.Name, null));
                    }

                    ClanChestDailyData.Add(DateTime.Now.ToString(@"MM-dd-yyyy"), clanChestData);
                }

                //-- make sure clanmate exists.
                var mate_index = 0;
                foreach (var tmpchest in tmpchestdata.ToList())
                {
                    bool alias_found = false;
                    bool exists = clanmates.Select(mate_name => mate_name.Name).Contains(tmpchest.Clanmate, StringComparer.CurrentCultureIgnoreCase);
                    if (!exists)
                    {
                        com.HellStormGames.Logging.Console.Write($"{tmpchest.Clanmate} doesn't exist within clanmates database. Attempting to find Aliases.", "Unknown Clanmate", LogType.WARNING);
                        Debug.WriteLine($"{tmpchest.Clanmate} doesn't exist within clanmates database. Attempting to find Aliases.");
                        //-- attempt to check to see if the unfound clanmate is under an alias.
                        foreach (var mate in clanmates)
                        {
                            if (mate.Aliases.Count > 0)
                            {
                                bool isMatch = mate.Aliases.Contains(tmpchest.Clanmate, StringComparer.InvariantCultureIgnoreCase);
                                if (isMatch)
                                {
                                    com.HellStormGames.Logging.Console.Write($"\t\t Unknown clanmate ({tmpchest.Clanmate}) belongs to {mate.Name} aliases.","Unknown Clanmate", LogType.INFO);
                                    var parent_data = clanChestData.Select(pd => pd).Where(data => data.Clanmate.Equals(mate.Name, StringComparison.InvariantCultureIgnoreCase)).ToList()[0];
                                    if (parent_data.chests == null)
                                    {
                                        parent_data.chests = new List<Chest>();
                                    }
                                    
                                    tmpchestdata[mate_index].Clanmate = mate.Name; //--- unknown clanmate properly identified and been re-written with correct parent clan name.
                                    
                                    alias_found = true;
                                    break;
                                }
                            }
                        }

                        if (!alias_found)
                        {
                            clanChestData.Add(new ClanChestData(tmpchest.Clanmate, tmpchest.chests));
                            com.HellStormGames.Logging.Console.Write($"{tmpchest.Clanmate} doesn't exist within clanmates database.", "Clanmate Not Found", LogType.WARNING);
                            ClanManager.Instance.ClanmateManager.Add(tmpchest.Clanmate);
                            ClanManager.Instance.ClanmateManager.Save(ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanmateDatabaseFile);
                        }
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
                                chestdata.chests = new List<Chest>();

                            foreach (var m_chest in m_chestdata.chests.ToList())
                            {
                                chestdata.chests.Add(m_chest);
                            }
                        }
                    }

                }

                var datestr = DateTime.Now.ToString(@"MM-dd-yyyy");
                ClanChestDailyData[datestr] = clanChestData;
                ChestProcessingState = ChestProcessingState.COMPLETED;

                sw.Stop();
                var elapsed = sw.Elapsed;
                var msg = $"--- Processing Chests Took: {elapsed}";
            }
            else
            {
                ChestProcessingState = ChestProcessingState.IDLE;
                GlobalDeclarations.isAnyGiftsAvailable = false;
            }

            GlobalDeclarations.isBusyProcessingClanchests = false;
            GlobalDeclarations.canCaptureAgain = true;

            return;
        }
        
        public async void LoadData()
        {
            var clanmatefile = ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanmateDatabaseFile;
            var clanchestfile = ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanChestDatabaseFile;
            var chestrequirementfile = ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanChestRequirementsFile;

            if (ClanManager.Instance.ClanmateManager.Database.Clanmates != null)
                ClanManager.Instance.ClanmateManager.Database.Clanmates.Clear();

            if (!System.IO.File.Exists(clanmatefile))
            {
                GlobalDeclarations.hasClanmatesBeenAdded = false;
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
                ClanChestDailyData.Add(DateTime.Now.ToString(@"MM-dd-yyyy"), clanChestData);
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
            //-- load chest requirements
            if (!System.IO.File.Exists(chestrequirementfile))
            {
                if (ClanManager.Instance.ClanChestSettings.ChestRequirements == null)
                {
                    ClanManager.Instance.ClanChestSettings.ChestRequirements = new ChestRequirements();
                    ClanManager.Instance.ClanChestSettings.InitSettings();
                }
                ClanManager.Instance.ClanChestSettings.SaveSettings(chestrequirementfile);
            }
            else
            {
                if (ClanManager.Instance.ClanChestSettings.ChestRequirements == null)
                    ClanManager.Instance.ClanChestSettings.ChestRequirements = new ChestRequirements();

                ClanManager.Instance.ClanChestSettings.LoadSettings(chestrequirementfile);
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
            var lastDate = ClanChestDailyData.Keys.Last();
            var dateStr = DateTime.Now.ToString(@"MM-dd-yyyy");
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
                ClanChestDailyData.Add(DateTime.Now.ToString(@"MM-dd-yyyy"), clanChestData);
            }

            GlobalDeclarations.hasClanmatesBeenAdded = true;

            return;
        }

        public void SaveData(string filepath = "")
        {
            //-- write to file.
            string file = String.Empty;
            if (String.IsNullOrEmpty(filepath))
                file = ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanChestDatabaseFile;
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
        public void CreateBackup()
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.Now;
            string file = $"{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanDatabaseBackupFolderPath}//clanchest_backup_{dateTimeOffset.ToUnixTimeSeconds()}.db";
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
            return Task.Run(()=>SaveData());
        }
        public Task ExportDataAsync(string filename, int chest_points_value, SortType sortType) => Task.Run(() =>  ExportData(filename, chest_points_value, sortType));
        public void ExportData(string filename, int chest_points_value = 0, SortType sortType = SortType.NONE)
        {
            if(!String.IsNullOrEmpty(filename)) 
            {
                using (StreamWriter sw = File.CreateText(filename))
                {
                    string date = DateTime.Now.ToString(@"MM/dd/yyyy");
                    var header = $"{date} Chest Count\r\n";
                    header += "--------------------------------------------";
                    sw.WriteLine(header);

                    

                    var count = 0;
                    List<ChestCountData> chestcountdata = new List<ChestCountData>();
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
                            if (chestdata.Count < 1)
                            {
                                //-- unable to find clanmate because possibly new.
                                continue;
                            }
                            else
                            {
                                var m_chestdata = chestdata[0];
                                if (m_chestdata.chests == null)
                                    chestcount.Count += 0;
                                else
                                    chestcount.Count += m_chestdata.chests.Count;
                            }
                        }
                    }

                    ClanChestDataComparator clanChestDataComparator = new ClanChestDataComparator();

                    List<ChestCountData> sortedChestCountData = new List<ChestCountData>();
                    if(sortType ==  SortType.ASCENDING)
                    {
                        sortedChestCountData = chestcountdata.OrderBy(i=> i.Count).ToList();      
                    }
                    else if(sortType == SortType.DESENDING) 
                    { 
                        sortedChestCountData = chestcountdata.OrderByDescending(i=> i.Count).ToList();  
                    }

                    if(sortedChestCountData.Count > 0)
                    {
                        chestcountdata.Clear();
                        chestcountdata = sortedChestCountData;
                    }

                    //-- add clan total percentage 
                    var num_clanmates = ClanManager.Instance.ClanmateManager.Database.Clanmates.Count;
                    var num_clanmates_gifts = 0;
                    foreach(var chest in chestcountdata)
                    {
                        if(chest.Count > 0)
                        {
                            num_clanmates_gifts += 1;
                        }
                    }
                    var clanmates_chests_percent = ((double)num_clanmates_gifts / num_clanmates * 100.0);
                    var statistics_message = $"Total Percentage of clan gifts: {clanmates_chests_percent:0.##}%\r\n";
                    statistics_message += "--------------------------------------------";
                    sw.WriteLine(statistics_message);


                    foreach (var chestcount in chestcountdata)
                    {
                        //--- 180 chests bug. Game doesn't like 180 number posted. Not sure why.
                        var chests = chestcount.Count == 180 ? chestcount.Count+1 : chestcount.Count;
                        if (chest_points_value > 0 && chests > 0)
                            chests += chest_points_value;

                        var content = $"{chestcount.Clanmate} - {chests} Chests";
                        sw.WriteLine(content);
                    }
                    sw.Close();
                }
            }
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
