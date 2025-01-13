using CefSharp.DevTools.Database;
using com.HellStormGames.Logging;
using MS.WindowsAPICodePack.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using TBChestTracker.Automation;
using TBChestTracker.Helpers;
using TBChestTracker.Managers;
using static System.Net.Mime.MediaTypeNames;
using com.HellStormGames.Diagnostics;

namespace TBChestTracker
{
    /*
      New as of 11/20/2024
    */
    public class ChestProcessor : IDisposable
    {
        #region Declarations
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
        #endregion

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
                var datestring = DateTime.Now.ToString(AppContext.Instance.ForcedDateFormat,CultureInfo.InvariantCulture);
                
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
                        database.NewEntry(DateTime.Now.ToString(AppContext.Instance.ForcedDateFormat, CultureInfo.InvariantCulture), tmp_ClanChestData);
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
        #endregion

        #region IsDateFormatValid
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
        #endregion

        #region WriteBlock
        //-- Writes OCR result in simple text file.
        //-- /db/cache/clanchest_2024-12-14.txt
        public bool WriteBlock(string file, string[] result)
        {
            
            try
            {
                using (StreamWriter sw = new StreamWriter(file, true))
                {
                    foreach (var r in result)
                    {
                        var b = Encoding.UTF8.GetBytes(r);
                        sw.Write(b);
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region Write
        public bool Write(string file, string[] result)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(file, true))
                {
                    foreach (var r in result)
                    {
                        sw.WriteLine(r);
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region WriteAsync
        public async Task<bool> WriteAsync(string file, string[] result)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(file, true))
                {
                    foreach (var r in result)
                    {
                        await sw.WriteLineAsync(r).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region Read
        public async Task<string[]> ReadCache(string file, IProgress<BuildingChestsProgress> progress)
        {
            int currentLineRead = 0;
            int maxLines = 0;
            
            var datestring = file.Substring(file.LastIndexOf("_") + 1);
            datestring = datestring.Substring(0, datestring.LastIndexOf("."));

            List<string> final_result = new List<string>();
            if(String.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException(nameof(file));
            }

            try
            {
                long fileSize = new FileInfo(file).Length;

                using (StreamReader sr = new StreamReader(file))
                {
                    var data = sr.ReadToEnd();
                    if (String.IsNullOrEmpty(data) == false)
                    {
                        data = data.Replace("\r\n", "\n");
                        var result = data.Split('\n');
                        maxLines = result.Length;

                        foreach(var r in result)
                        {
                            if(String.IsNullOrEmpty(r) == false)
                            {
                                final_result.Add(r);
                            }
                            var p = new BuildingChestsProgress($"Processing Clan Chests Cache for {datestring} ({currentLineRead}/{maxLines})...",-1, maxLines, currentLineRead, false);
                            progress.Report(p);
                            await Task.Delay(10);
                            currentLineRead++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
             
            }

            return final_result.ToArray();
        }
        #endregion

        #region BuildChestBoxes
        [Obsolete]
        private async Task<List<ChestBox>> BuildChestBoxes(string[] r, IProgress<BuildingChestsProgress> progress = null)
        {
            var chestboxes = new List<ChestBox>();
            var clicks = SettingsManager.Instance.Settings.AutomationSettings.AutomationClicks;
            var tmpResult = new List<string>();

            int currentCheckBox = 0;
            var result = r.ToList();

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

                int maxChestBoxes = result.Count / _inc;
                var p = new BuildingChestsProgress($"Creating ChestBoxes ({currentCheckBox}/{maxChestBoxes})...", -1, maxChestBoxes, currentCheckBox, false);
                progress.Report(p);

                //-- tmpResult somehow returns 2 whereas it should be 3 or 4.
                //-- causing var w = tmpResult[c] to cause argument out of index exception.
                try
                {
                    for (var c = 0; c < _inc; c++)
                    {
                        var w = tmpResult[c];
                        cb.Content.Add(w);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }

                tmpResult.Clear();

                b += cb.Content.Count - 1;

                chestboxes.Add(cb);

                currentCheckBox++;

                await Task.Delay(10);
            }

            tmpResult.Clear();
            tmpResult = null;

            return chestboxes;
        }
        #endregion

        #region ProcessChestBoxes
        private async Task<ProcessingTextResult> ProcessChestBoxes(List<ChestBox> chestboxes, IProgress<BuildingChestsProgress> progress = null)
        {
            ProcessingTextResult ProcessingTextResult = new ProcessingTextResult();
            bool bError = false;
            List<ChestData> tmpchests = new List<ChestData>();

            var currentChestbox = 0;
            var maxChestBox = chestboxes.Count;

            foreach (var chestbox in chestboxes)
            {
                var pra = new BuildingChestsProgress($"Processing Chest Boxes ({currentChestbox}/{maxChestBox})...",-1, maxChestBox, currentChestbox, false);
                progress.Report(pra);

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

                        Consolio.Write($"OCR RESULT [{chestName}, {clanmate}, {chestobtained}", "OCR Result", LogType.INFO);

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
                                        Consolio.Write($"Attempting to correct clanmate name => {clanmate}", "Clanmate Name Issue", LogType.INFO);
                                        clanmate = clanmate.Remove(fromStartingPos, clanmate.IndexOf(' ') + 1);
                                        Consolio.Write($"Clanmate name after correction => {clanmate}", "Clanmate Repair Result", LogType.INFO);
                                    }

                                    //throw new Exception("Clanmate name is blank. Increase thread sleep timer to prevent this.");
                                }

                            }
                            catch (Exception e)
                            {
                                bError = true;
                                ProcessingTextResult.Status = ProcessingStatus.CLANMATE_ERROR;
                                ProcessingTextResult.Message = $"Seems to be an issue while extracting clanmate's name. Exception caught => {e.Message}. ";
                                Consolio.Write($"Couldn't Process Clanmate name correctly. Affected Clanmate => {clanmate}", "Clanmate Extraction Failed", LogType.ERROR);
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

                        Consolio.Write(dbg_msg, "OCR Result", LogType.INFO);
                    }
                }

                currentChestbox++;
                await Task.Delay(10);
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

        #region Build
        public async Task<ProcessingTextResult> BuildFromCacheFile(string file, IProgress<BuildingChestsProgress> progress)
        {
            var chestboxes = await PrepareChestBoxesFromCache(file, progress);
            if (chestboxes == null || chestboxes.Count < 1)
            {
                var errorProcess = new ProcessingTextResult(ProcessingStatus.UNKNOWN_ERROR, "ChestBoxBuilder.exe not found.", null, null);
                return errorProcess;
            }
            var processedChestBoxes = await ProcessChestBoxes(chestboxes, progress);

            chestboxes?.Clear();
            chestboxes = null;
            return processedChestBoxes;
        }

        public async Task<List<ChestBox>> PrepareChestBoxesFromCache(string filename, IProgress<BuildingChestsProgress> progress)
        {
            bool errorOccurred = false;
            
            if(File.Exists($"{AppContext.Instance.AppFolder}ChestBoxBuilder.exe") == false)
            {
                var ee = new BuildingChestsProgress("ChestBoxBuilder.exe can not be located. Ensure it is inside program's directory.", -1,0,0,false, true);
                progress.Report(ee);
                return null;
            }

            List<ChestBox> boxes = new List<ChestBox>();
            if(File.Exists(filename) == false)
            {
                var ee = new BuildingChestsProgress($"{filename} does not exist as it should. Will not continue.", -1, 0, 0, false, true);
                progress.Report(ee);
                await Task.Delay(200);
                return null;
            }

            var e = new BuildingChestsProgress("Starting ChestBoxBuilder....", -1, 0, 0, false);
            progress.Report(e);
            await Task.Delay(250);

            var outputFile = filename;
            outputFile = outputFile.Replace(Path.GetExtension(outputFile), ".old");
            var chestboxbuilderPath = $"{AppContext.Instance.AppFolder}ChestBoxBuilder.exe";
            string infile = "\"" + filename + "\"";
            string outfile = "\"" + outputFile + "\"";
            string language = "\"en_US\"";

            var chestboxbuilderArgs = $@"-i {infile} -o {outfile} -L {language}";
            ConsoleInterop consoleInterop = new ConsoleInterop(chestboxbuilderPath, chestboxbuilderArgs , AppContext.Instance.AppFolder);
            
            consoleInterop.Completed += (s, e) =>
            {
                bool outputFileExists = false;
                if(e.isCompleted == true)
                {
                    outputFileExists = File.Exists(outputFile);

                    if(outputFileExists == false)
                    {
                        var ef = new BuildingChestsProgress($"Error occured. Somehow the output file '{outputFile}' does not exist or can not be accessed.", -1, 0, 0, false, true);
                        progress.Report(ef);
                        Task.Delay(250);
                        errorOccurred = true;
                        return;
                    }

                    using (StreamReader sr = new StreamReader(outputFile))
                    {
                        var sr_data = sr.ReadToEnd();
                        var checkboxes = sr_data.Split('#');
                        checkboxes = checkboxes.Select(x => x.Replace("\r\n", "\n")).ToArray();

                        foreach (var checkbox in checkboxes)
                        {
                            if (String.IsNullOrEmpty(checkbox))
                            {
                                continue;
                            }
                            var chestboxitem = new ChestBox();
                            var checkbox_lines = checkbox.Split('\n');
                            foreach (var checkbox_line in checkbox_lines)
                            {
                                if (!String.IsNullOrEmpty(checkbox_line))
                                {
                                    chestboxitem.Content.Add(checkbox_line);
                                }
                            }

                            boxes.Add(chestboxitem);
                        }
                        sr.Close();
                    }
                }
            };

            consoleInterop.DataReceived += (s, e) =>
            {
                var data = e.Data;
                if(String.IsNullOrEmpty((data)) == false)
                {
                    //--- [CODE][MESSAGE][PERCENT]
                    //--- CODES:
                    /*
                     100 - Message
                     200 - Completed
                     404 - Not Found
                     500 - Error
                    */
                    int? status_code = e.StatusCode;
                    string? message = e.Data;
                    double? percent = e.Percent;

                    var p = new BuildingChestsProgress(message, percent.Value);
                    progress.Report(p);
                }
                else
                {
                    //-- have we somehow stopped receiving data from ChestBoxBuilder?
                    var exited = false;
                }

            };
            
            string error_msg = String.Empty;

            consoleInterop.Error += (s, e) =>
            {
                error_msg = e.ErrorMessage;
                errorOccurred = true;
                var ef = new BuildingChestsProgress($"Error occured. Reason '{error_msg}'", -1, 0, 0, false, true);
                progress.Report(ef);
                Task.Delay(250);
                return;

            };

            CancellationTokenSource cts = new CancellationTokenSource();
            
            bool result = await consoleInterop.Run(cts.Token);
            if(result == false)
            {
                var x = new BuildingChestsProgress("Failed Starting ChestBoxBuilder....", -1, 0, 0, false, true);
                progress.Report(x);
                await Task.Delay(250);
                return null;
            }
            if(errorOccurred)
            {
                var ea = new BuildingChestsProgress(error_msg, -1, 0, 0, false, true);
                progress.Report(ea);
                await Task.Delay(1000);
                return null;
            }
            return boxes;
        }
        #endregion

        #region UpdateUI 
        private async Task UpdateUI(BuildingChestsProgress buildingChestsProgress, IProgress<BuildingChestsProgress> progress, int delay = 100)
        {
            if (buildingChestsProgress == null)
            {
                throw new Exception(nameof(buildingChestsProgress));
            }

            progress.Report(buildingChestsProgress);
            await Task.Delay(delay);
        }
        private async Task UpdateUI(string message, double percentage, double total, double current, bool isCompleted, IProgress<BuildingChestsProgress> progress, int delay = 100)
        {
            var p = new BuildingChestsProgress(message, percentage, total, current, isCompleted);
            progress.Report(p);
            await Task.Delay(delay);
        }
        #endregion

        #region FinalizeBuildToDatabase
        public async Task FinalizeBuildToDatabase(string file, ProcessingTextResult result, IProgress<BuildingChestsProgress> progress, ClanChestDatabase database)
        {
            List<ChestData> tmpchests = result.ChestData;
            IList<ClanChestData> tmpchestdata = CreateClanChestData(tmpchests);

            var clanmates = ClanManager.Instance.ClanmateManager.Database.Clanmates.ToList();

            var datestring = file.Substring(file.LastIndexOf("_") + 1);
            datestring = datestring.Substring(0, datestring.LastIndexOf("."));

            var dates = database.ClanChestData.Where(x => x.Key.Equals(datestring));

            int currentChestDataCount = 0;
            int currentTempChestDataCount = 0;
            int maxTempChestDataCount = tmpchestdata.Count;
            var mate_index = 0;

            //-- Initialize tmp_ClanChestData if no dates exist and add new date.
            if (dates.Count() == 0)
            {
                tmp_ClanChestData.Clear();
                foreach (var mate in clanmates)
                {
                    tmp_ClanChestData.Add(new ClanChestData(mate.Name, null, 0));
                }
                database.NewEntry(datestring, tmp_ClanChestData);
            }

            //-- make sure clanmate exists.
            foreach (var tmpchest in tmpchestdata.ToList())
            {
                await UpdateUI(new BuildingChestsProgress($"Building Temporary Chests for {datestring}", -1, tmpchestdata.Count, currentTempChestDataCount, false),
                    progress);

                //-- Does Clanmate exist?
                bool exists = clanmates.Select(mate_name => mate_name.Name).Contains(tmpchest.Clanmate, StringComparer.CurrentCultureIgnoreCase);
                if (exists)
                {
                    mate_index++;
                    continue; //-- advance to next iteration
                }
                else
                {
                    //-- Do we have a possibly a mispelled clanmate?
                    var tClanmate = tmpchest.Clanmate;
                    var match_clanmate = Clanmate_Scan(tClanmate, SettingsManager.Instance.Settings.OCRSettings.ClanmateSimilarity);
                    if (match_clanmate != null)
                    {
                        Consolio.Write($"{tmpchest.Clanmate} is actually {match_clanmate.Name}.", "Unknown Clanmate Found", LogType.INFO);

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
                        Consolio.Write($"Adding {tmpchest.Clanmate} to clanmates database.", "Clanmate Not Found", LogType.WARNING);
                        ClanManager.Instance.ClanmateManager.Add(tmpchest.Clanmate);
                        ClanManager.Instance.ClanmateManager.Save();
                    }
                }
                currentTempChestDataCount++;
                await Task.Delay(50);
            }

            //-- update clanchestdata
            int maxChestDataCount = tmp_ClanChestData.Count;
            foreach (var chestdata in tmp_ClanChestData)
            {
                var _chestdata = tmpchestdata.Where(name => name.Clanmate.Equals(chestdata.Clanmate,
                  StringComparison.CurrentCultureIgnoreCase)).Select(chests => chests).ToList();

                //-- update ui
                await UpdateUI(new BuildingChestsProgress($"Building Chest Data for {datestring}", -1, tmp_ClanChestData.Count, currentChestDataCount, false),
                    progress);
                
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
                            //-- Begin Calculating Chest Points For Clanmate(s).
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

                            //-- Finish up by adding the chests.
                            chestdata.chests.Add(m_chest);
                        }
                    }
                }

                currentChestDataCount++;
                await Task.Delay(50);
            }

            //-- Update Entry inside Database.
            database.UpdateEntry(datestring, tmp_ClanChestData);
        }
        #endregion

        #region Build
        public async Task Build(string[] files, IProgress<BuildingChestsProgress> progress, ClanChestDatabase database)
        {
            var filesMax = files.Length;
            var currentFile = 0;
            var errorOccured = false;
            ProcessingTextResult result = null;
            foreach (var file in files)
            {
                result = await BuildFromCacheFile(file, progress);
                if (result != null)
                {
                    if(result.Status != ProcessingStatus.OK)
                    {
                        //-- possibly cancel and present error
                        errorOccured = true;
                        currentFile++;
                        continue;
                    }
                    
                    await FinalizeBuildToDatabase(file, result, progress, database);
                    AppContext.Instance.isBusyProcessingClanchests = false;

                    result.ChestData.Clear();
                    result.RawData.Clear();
                    result = null;

                    currentFile++;
                }
                
                await Task.Delay(250);
            }

            if(errorOccured)
            {
                result.ChestData.Clear();
                result.RawData.Clear();
                result = null;
            }

            if (errorOccured == false)
            {
                var p2 = new BuildingChestsProgress($"Finished Building Clan Chests...", -1, filesMax, currentFile, true, false);
                progress.Report(p2);
            }
        }
        #endregion

        #region Validate Temporary Chest Data
        public void ValidateTemporaryChestData()
        {

        }
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

        #region ProcessToCache
        public void ProcessToCache(List<string> result, ChestAutomation chestautomation, string filename = "")
        {
            string file = String.Empty;
            if (String.IsNullOrEmpty(filename))
            {
                var clandb = ClanManager.Instance.ClanDatabaseManager.ClanDatabase;
                var clanfolder = $"{ClanManager.Instance.CurrentProjectDirectory}";
                var cacheFolder = $"{clanfolder}\\cache";
                DirectoryInfo di = new DirectoryInfo(cacheFolder);
                if (di.Exists == false)
                {
                    di.Create();
                }

                var forcedDate = AppContext.Instance.ForcedDateFormat;
                var prefile = $"\\clanchests_cache_{DateTime.Now.ToString(forcedDate)}.txt";
                file = $"{cacheFolder}{prefile}";
            }

            //-- Just for now until settings call for Ingore No Gifts
            if (result[0].ToLower().Contains("no gifts"))
            {
                ChangeProcessingState(ChestProcessingState.IDLE);
                AppContext.Instance.isAnyGiftsAvailable = false;
                chestautomation.InvokeChestProcessed(new Automation.AutomationChestProcessedEventArguments(new ClanChestProcessResult("No Gifts", 404, ClanChestProcessEnum.NO_GIFTS)));
                return;
            }

            ChangeProcessingState(ChestProcessingState.PROCESSING);

            AppContext.Instance.isAnyGiftsAvailable = true;
            ProcessingTextResult rawResult = ProcessScreenText(result, true);

            if (rawResult.Status != ProcessingStatus.OK)
            {
                chestautomation.InvokeChestProcessingFailed(new AutomationChestProcessingFailedEventArguments($"{rawResult.Message}", 0));
                return;
            }

            var rawChestData = rawResult.RawData;
            var writeResult = Write(file, rawChestData.ToArray());
            if(writeResult)
            {
                ChangeProcessingState(ChestProcessingState.COMPLETED);
                AppContext.Instance.isBusyProcessingClanchests = false;

                Automation.AutomationChestProcessedEventArguments args =
                    new AutomationChestProcessedEventArguments(new ClanChestProcessResult("200", 200, ClanChestProcessEnum.SUCCESS));

                chestautomation.InvokeChestProcessed(args);
            }
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

        #region FilterOCRResults 
        private ProcessingTextResult FilterOCRResults(List<string> result)
        {
            ProcessingTextResult ProcessingTextResult = new ProcessingTextResult();
            bool bFilteringError = false;
            string sFilteringErrorMessage = String.Empty;

            lastFilterString = String.Empty;
            filteringErrorOccurred = false;
            var filter_words = SettingsManager.Instance.Settings.OCRSettings.Tags.ToArray(); // new string[] { "Chest", "From", "Source", "Gift" };

            //-- OCR Result exception from filtering resulted in:
            //-- From : [player_name]
            //-- Patched 1/22/24

            FilterChestData(ref result, filter_words, errorResult =>
            {
                if (!String.IsNullOrEmpty(errorResult))
                {
                    Consolio.Write(errorResult, "Invalid OCR", LogType.INFO);
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

            ProcessingTextResult.Status = ProcessingStatus.OK;
            ProcessingTextResult.Message = "Success";
            ProcessingTextResult.RawData = result;

            return ProcessingTextResult;
        }
        #endregion

        #region BuildChestBoxesFromCache
        private List<ChestBox> BuildChestBoxesFromCache(List<string> result, IProgress<BuildingChestsProgress> progress)
        {
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

            return chestboxes;
        }
        #endregion
        
        #region ProcessScreenText 
        private ProcessingTextResult ProcessScreenText(List<string> result, bool outputAsRaw = false)
        {
            ProcessingTextResult ProcessingTextResult = new ProcessingTextResult();
            ProcessingTextResult FilteringTextResult = FilterOCRResults(result);

            if (FilteringTextResult != null)
            {
                var filtered_result = FilteringTextResult.RawData;

                ProcessingTextResult.Status = ProcessingStatus.OK;
                ProcessingTextResult.Message = "Success";
                ProcessingTextResult.ChestData = null;
                ProcessingTextResult.RawData = filtered_result;
                return ProcessingTextResult;
                //var chestboxes = BuildChestBoxes(filtered_result);
                //ProcessingTextResult = ProcessChestBoxes(ref chestboxes);
            }

            return null;
        }
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
