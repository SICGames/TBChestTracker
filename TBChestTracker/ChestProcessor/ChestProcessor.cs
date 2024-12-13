using CefSharp.DevTools.Database;
using com.HellStormGames.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBChestTracker.Automation;
using TBChestTracker.Managers;

namespace TBChestTracker
{
    /*
      New as of 11/20/2024
    */
    public class ChestProcessor : IDisposable
    {
        private IList<ClanChestData> tmp_ClanChestData;
        bool filteringErrorOccurred = false;
        string lastFilterString = String.Empty;
        public ChestRewardsManager ChestRewards { get; private set; }

        public IList<ClanChestData> GetClanChestData()
        {
            if(tmp_ClanChestData == null)
            {
                throw new NullReferenceException(nameof(tmp_ClanChestData));    
            }

            return tmp_ClanChestData;
        }
        #region ChestProcessingState 
        private ChestProcessingState pPreviousChestProcessingState = ChestProcessingState.IDLE;
        private ChestProcessingState pChestProcessingState = ChestProcessingState.IDLE;
        public ChestProcessingState ChestProcessingState 
        { 
            get 
            { 
                return pChestProcessingState; 
            } 
        }

        public void ChangeProcessingState(ChestProcessingState state)
        {
            if (pChestProcessingState != pPreviousChestProcessingState)
            {
                onChestProcessingStateChanged(new ChestProcessingStateChangedEventArguments(pPreviousChestProcessingState, pChestProcessingState));
                pPreviousChestProcessingState = pChestProcessingState;
            }

            pChestProcessingState = state;
        }
        #endregion

        #region Events
        public event EventHandler<ChestProcessingStateChangedEventArguments> ChestProcessingStateChanged = null;
        public event EventHandler<ChestProcessingEventArguments> ChestProcessed = null;

        protected virtual void onChestProcessed(ChestProcessingEventArguments args)
        {
            if (ChestProcessed != null)
            {
                ChestProcessed(this, args);
            }
        }
        protected virtual void onChestProcessingStateChanged(ChestProcessingStateChangedEventArguments args)
        {
            if (ChestProcessingStateChanged != null)
            {
                ChestProcessingStateChanged(this, args);
            }
        }
        #endregion

        #region Init 
        public ChestDataBuildResult Init(ClanChestDatabase database)
        {
            bool invalidDateFormat = false;

            if (tmp_ClanChestData ==  null)
            {
                throw new ArgumentNullException(nameof(tmp_ClanChestData));
            }
            if(database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            var clanmates = ClanManager.Instance.ClanmateManager.Database.Clanmates;
            foreach(var clanmate in clanmates)
            {
                tmp_ClanChestData.Add(new ClanChestData(clanmate.Name, null));
            }

            if(database != null && database.ClanChestData != null && database.ClanChestData.Keys != null && database.ClanChestData.Keys.Count() > 0)
            {
                var lastDate = database.ClanChestData.Keys.Last();
                var datestring = DateTime.UtcNow.ToString(AppContext.Instance.ForcedDateFormat,CultureInfo.InvariantCulture);
                
                invalidDateFormat = IsDataFormatValid(database);
                
                if (lastDate.Equals(datestring))
                {
                    tmp_ClanChestData = database.ClanChestData[datestring];
                    foreach (var member in ClanManager.Instance.ClanmateManager.Database.Clanmates)
                    {
                        var clanmate_exists = tmp_ClanChestData.ToList().Exists(mate => mate.Clanmate.ToLower().Contains(member.Name.ToLower()));
                        if (!clanmate_exists)
                        {
                            tmp_ClanChestData.Add(new ClanChestData(member.Name, null));
                        }
                    }
                }
                else
                {
                    try
                    {
                        //database.NewEntry(DateTime.Now.ToString(AppContext.Instance.ForcedDateFormat, CultureInfo.InvariantCulture), tmp_ClanChestData);
                        database.NewEntry(DateTime.UtcNow.ToString(AppContext.Instance.ForcedDateFormat, CultureInfo.InvariantCulture), tmp_ClanChestData);
                        //ClanChestDailyData.Add(DateTime.Now.ToString(dateformat, new CultureInfo(CultureInfo.CurrentCulture.Name)), clanChestData);
                    }
                    catch (Exception ex)
                    {
                        return ChestDataBuildResult.DATA_CORRUPT;
                    }
                }
            }
            else
            {
                //-- Happens when ClanChestData can not be parsed through json. 
                //-- Signifies DB needs upgrade.
                //-- since it failed to fill structure data to newest version of database.
                AppContext.Instance.IsClanChestDatabaseUpgradeRequired = true;
            }
            if (invalidDateFormat)
            {
                return ChestDataBuildResult.DATA_CORRUPT;
            }

            return ChestDataBuildResult.OK;
        }
        public bool IsDataFormatValid(ClanChestDatabase database)
        {
            var currentCulture = CultureInfo.CurrentCulture; //-- en-GB
            var dates = new List<string>();
            foreach (var date_key in database.ClanChestData.Keys.ToList())
            {
                dates.Add(date_key);
            }
            bool result = false;
            foreach (var date in dates)
            {
                DateTime dateObject;
                result = DateTime.TryParseExact(date, AppContext.Instance.ForcedDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateObject);
            }

            return !result; //-- returning the opposite. If it matches then we return false. If it does not match, we return true.
        }

        #region Validate Temporary Chest Data
        public void ValidateTemporaryChestData()
        {

        }
        #endregion
        #endregion
        #region ChestProcessor Constructor 
        public ChestProcessor()
        {
            tmp_ClanChestData = new List<ClanChestData>(); 
            if(ChestRewards == null)
            {
                ChestRewards = new ChestRewardsManager();
            }
        }

        #endregion
        #region Processing Chest Functions
        public async Task Process(List<string> result,ChestAutomation chestAutomation, ClanChestDatabase database)
        {
            if(database == null)
            {
                throw new ArgumentNullException(nameof(database));  
            }
            var resulttext = result;

            if (resulttext[0].ToLower().Contains("no gifts"))
            {
                ChangeProcessingState(ChestProcessingState.IDLE);
                AppContext.Instance.isAnyGiftsAvailable = false;

                chestAutomation.InvokeChestProcessed(new Automation.AutomationChestProcessedEventArguments(new ClanChestProcessResult("No Gifts", 404, ClanChestProcessEnum.NO_GIFTS)));
                return;
            }
            ChangeProcessingState(ChestProcessingState.PROCESSING);
            
            AppContext.Instance.isAnyGiftsAvailable = true;
            ProcessingTextResult textResult = ProcessText3(resulttext);

            if (textResult.Status != ProcessingStatus.OK)
            {
                chestAutomation.InvokeChestProcessingFailed(new AutomationChestProcessingFailedEventArguments($"{textResult.Message}", 0));
                return;
            }

            List<ChestData> tmpchests = textResult.ChestData;
            IList<ClanChestData> tmpchestdata = CreateClanChestData(tmpchests);
            var clanmates = ClanManager.Instance.ClanmateManager.Database.Clanmates.ToList();

            //-- do we need to insert new entry?
            //-- causes midnight bug.
            var currentdate = DateTime.UtcNow.ToString(AppContext.Instance.ForcedDateFormat);
            
            var dates = database.ClanChestData.Where(x => x.Key.Equals(currentdate));

            if (dates.Count() == 0)
            {
                tmp_ClanChestData.Clear();
                foreach (var mate in clanmates)
                {
                    
                    tmp_ClanChestData.Add(new ClanChestData(mate.Name, null, 0));
                }

                database.NewEntry(DateTime.UtcNow.ToString(AppContext.Instance.ForcedDateFormat), tmp_ClanChestData);
                database.ClanChestData.Add(DateTime.UtcNow.ToString(AppContext.Instance.ForcedDateFormat, CultureInfo.InvariantCulture), tmp_ClanChestData);
            }

            //-- make sure clanmate exists.
            var mate_index = 0;
            Parallel.ForEach(tmpchestdata.ToList(), tmpchest =>  //foreach (var tmpchest in tmpchestdata.ToList())
            {
                bool exists = clanmates.Select(mate_name => mate_name.Name).Contains(tmpchest.Clanmate, StringComparer.CurrentCultureIgnoreCase);
                if (exists)
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

                        var parent_data = tmp_ClanChestData.Select(pd => pd).Where(data => data.Clanmate.Equals(match_clanmate.Name, StringComparison.InvariantCultureIgnoreCase)).ToList()[0];
                        if (parent_data.chests == null)
                        {
                            parent_data.chests = new List<Chest>();
                        }
                    }
                    else
                    {
                        tmp_ClanChestData.Add(new ClanChestData(tmpchest.Clanmate, tmpchest.chests, tmpchest.Points));
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
                                var parent_data = tmp_ClanChestData.Select(pd => pd).Where(data => data.Clanmate.Equals(mate.Name, StringComparison.InvariantCultureIgnoreCase)).ToList()[0];
                                alias_found = true;
                                break;
                            }
                        }
                    }
                    */
                }
            });

            //-- update clanchestdata
            Parallel.ForEach(tmp_ClanChestData, chestdata => //foreach (var chestdata in tmp_ClanChestData)
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

                        foreach (var m_chest in m_chestdata.chests.ToList()) //foreach (var m_chest in m_chestdata.chests.ToList())
                        {
                            if (ClanManager.Instance.ClanChestSettings.GeneralClanSettings.ChestOptions == ChestOptions.UsePoints)
                            {
                                var pointvalues = ClanManager.Instance.ClanChestSettings.ChestPointsSettings.ChestPoints;
                                foreach (var pointvalue in pointvalues)
                                {
                                    var chest_type = m_chest.Type.ToString();
                                    var chest_name = m_chest.Name.ToString();

                                    var level = m_chest.Level;

                                    if (chest_type.ToLower().Contains(pointvalue.ChestType.ToLower()))
                                    {
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

            var datestr = DateTime.UtcNow.ToString(AppContext.Instance.ForcedDateFormat, CultureInfo.InvariantCulture);
            database.UpdateEntry(datestr, tmp_ClanChestData);
            //ClanChestDailyData[datestr] = tmp_ClanChestData;
            
            ChangeProcessingState(ChestProcessingState.COMPLETED);
            //ChestProcessingState = ChestProcessingState.COMPLETED;

            AppContext.Instance.isBusyProcessingClanchests = false;

            Automation.AutomationChestProcessedEventArguments args = 
                new AutomationChestProcessedEventArguments(new ClanChestProcessResult("200", 200, ClanChestProcessEnum.SUCCESS));

            chestAutomation.InvokeChestProcessed(args);

        }

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
                if (!String.IsNullOrEmpty(errorResult))
                {
                    com.HellStormGames.Logging.Console.Write(errorResult, "Invalid OCR", com.HellStormGames.Logging.LogType.INFO);
                    bFilteringError = true;
                    sFilteringErrorMessage = errorResult;
                }
            });

            if (bFilteringError)
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
                for (var a = b; a < result.Count; a++)
                {
                    tmpResult.Add(result[a]);
                }

                var bContainsIndex = tmpResult.FindIndex(r => r.StartsWith("Contains:"));
                var _inc = bContainsIndex > -1 ? 4 : 3;
                for (var c = 0; c < _inc; c++)
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
        #endregion

        #region CreateClanChestData
        private List<ClanChestData> CreateClanChestData(List<ChestData> chestdata)
        {
            List<ClanChestData> tmp_ClanChestData = new List<ClanChestData>();
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
                tmp_ClanChestData.Add(clanchestdata);
            }
            return tmp_ClanChestData;
        }
        #endregion

        #region Clanmate Similiarities 
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

            Parallel.ForEach(clanmates, (clanmate, state) =>
            {
                var similarity = jw.Similarity(clanmate.Name, input) * 100.0;
                Debug.WriteLine($"{clanmate.Name} and {input} have a Similiarity Percent => {similarity}%");
                if (similarity > similiarityThreshold)
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
        #endregion

        #region Dipose
        public void Dispose()
        {
            if(tmp_ClanChestData != null)
            {
                tmp_ClanChestData.Clear();
                tmp_ClanChestData = null;
            }
        }
        #endregion

    }
}
