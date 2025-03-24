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
using TBChestTracker.Pages;
using CsvHelper;
using System.Windows;

namespace TBChestTracker
{
    /*
      New as of 11/20/2024
    */
    public class ChestProcessor : IDisposable
    {
        #region Declarations
        bool filteringErrorOccurred = false;
        string lastFilterString = String.Empty;
        List<AffectedWords> OcrCorrectionWords = new List<AffectedWords>();
        Object chestsLock { get; set; }
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

        #region ChestProcessor Constructor 
        public ChestProcessor()
        {

        }
        #endregion

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

        public bool LoadOcrCorrectionWords()
        {
            try
            {
                if(OcrCorrectionWords != null)
                {
                    OcrCorrectionWords.Clear();
                }

                var ocrCorrectionListFIle = $"{ClanManager.Instance.CurrentProjectDirectory}\\db\\OcrCorrectionList.json";
                if (File.Exists(ocrCorrectionListFIle))
                {
                     bool loaded = JsonHelper.TryLoad(ocrCorrectionListFIle, out OcrCorrectionWords);
                    return loaded;
                }
            }
            catch (Exception e)
            {
                Loggio.Error(e, "Chest Processor", "Failed loading OcrCorrectionList.json");
                return false;
            }

            return false;
        }
        #region Init 
        public ChestDataBuildResult Init()
        {
            LoadOcrCorrectionWords();
            return ChestDataBuildResult.OK;
        }
        #endregion

        #region WriteBlock
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
                        if(string.IsNullOrEmpty(r) == false)
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
                            await Task.Delay(5);
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

        public void ProcessToTempFile(List<string> result, ChestAutomation chestautomation, string filename = "")
        {
            string file = String.Empty;
            if (String.IsNullOrEmpty(filename))
            {
                var clandb = ClanManager.Instance.ClanDatabaseManager.ClanDatabase;
                var clanfolder = $"{ClanManager.Instance.CurrentProjectDirectory}";
                var date = DateTime.Now.ToString(@"yyyy-MM-dd");
                var chestsFolder = $"{clanfolder}\\Chests\\Temp";
                DirectoryInfo di = new DirectoryInfo(chestsFolder);
                if (di.Exists == false)
                {
                    di.Create();
                }
                var forcedDate = AppContext.Instance.ForcedDateFormat;
                var prefile = $"\\chests_{DateTime.Now.ToString(forcedDate)}.txt";
                file = $"{chestsFolder}{prefile}";
            }
            else
            {
                file = filename;
            }
            Process(result, chestautomation, file);
        }

        #region ProcessToCache
        public void ProcessToFile(List<string> result, ChestAutomation chestautomation, string filename = "")
        {
            string file = String.Empty;
            if (String.IsNullOrEmpty(filename))
            {
                var clandb = ClanManager.Instance.ClanDatabaseManager.ClanDatabase;
                var clanfolder = $"{ClanManager.Instance.CurrentProjectDirectory}";
                var date = DateTime.Now.ToString(@"yyyy-MM-dd");
                var chestsFolder = $"{clanfolder}\\Chests\\Data";
                DirectoryInfo di = new DirectoryInfo(chestsFolder);
                if (di.Exists == false)
                {
                    di.Create();
                }
                var forcedDate = AppContext.Instance.ForcedDateFormat;
                var prefile = $"\\chests_{DateTime.Now.ToString(forcedDate)}.txt";
                file = $"{chestsFolder}{prefile}";
            }
            else
            {
                file = filename;
            }

            Process(result, chestautomation, file);
        }


        private void Process(List<string> result, ChestAutomation chestautomation, string filename)
        {
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
            var automationClicks = SettingsManager.Instance.Settings.AutomationSettings.AutomationClicks;

            var extraVal = rawChestData.Contains("Contains") == true ? 4 : 3;
            var remaining = rawChestData.Count % extraVal;

            //-- remaining should be 0. If there's a issue, it would be over 1.
            if (remaining > 0)
            {
                //-- there's a error.
                //-- we need to go through and find out what is causing the issue. 
                //-- Address it in the log file for the user. 
                var issueString = String.Empty;
                foreach (var word in rawChestData.ToArray())
                {
                    issueString += $"{word}\r\n";
                }
                Loggio.Warn("Ocr Issue", $"Unfortunately, Ocr Results turned up to have a problem. This can cause Chest Builder not building correctly. \n\tBelow is the results of the problematic area:\n\t\t {issueString}\n. This Rip Cord is to prevent further issues.");
                var errmsg = "Unfortunately, we need to stop automation. \n\tOcr ran into an issue with incorrect amount of lines required to be valid. \nIf there are 12 lines of Ocr text divided by 3 or 4 lines, the remaining should be 0.\n The other good Ocr reads were saved. Be sure to check out the latest log file for more information.";
                chestautomation.InvokeChestProcessingFailed(new AutomationChestProcessingFailedEventArguments(errmsg, 0));
                return;
            }

            var writeResult = Write(filename, rawChestData.ToArray());
            if (writeResult)
            {
                ChangeProcessingState(ChestProcessingState.COMPLETED);
                AppContext.Instance.isBusyProcessingClanchests = false;
                AppContext.Instance.HasCollectedChests = true;
                Automation.AutomationChestProcessedEventArguments args =
                    new AutomationChestProcessedEventArguments(new ClanChestProcessResult("200", 200, ClanChestProcessEnum.SUCCESS));

                chestautomation.UpdateOcrLinesRead(rawChestData.Count);

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

            FilterChestData(ref result, filter_words, errorResult =>
            {
                if (!String.IsNullOrEmpty(errorResult))
                {
                    Loggio.Warn("Invalid OCR", errorResult);
                    bFilteringError = true;
                    sFilteringErrorMessage = errorResult;
                }
            });

            ProcessingTextResult.Status = ProcessingStatus.OK;
            ProcessingTextResult.Message = "Success";
            ProcessingTextResult.RawData = result;

            return ProcessingTextResult;
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
                var filtered_result_string = String.Empty;

                foreach (var fr in filtered_result) 
                {
                    filtered_result_string += $"{fr}\r\n";
                }

                if (OcrCorrectionWords != null && OcrCorrectionWords.Count > 0)
                {
                    foreach (var correctword in OcrCorrectionWords)
                    {
                        var correct = correctword.Word;
                        foreach (var incorrectword in correctword.IncorrectWords)
                        {
                            var incorrect = incorrectword.Word;
                            var escaped_IncorrectWord = Regex.Escape(incorrect);
                            var match_pattern = $@"({escaped_IncorrectWord})+";
                            var hasMatches = false;

                            //var fake_result_string = "JÃ³rmungandr's Chest\r\nFrom: Maximus not Jeff\r\nSource: Jormungandr Shop\r\nJÃ³rmungandr's Chest\r\nFrom: Maximus not Jeff\r\nSource: Jormungandr Shop\r\nJÃ³rmungandr's Chest\r\nFrom: Maximus not Jeff\r\nSource: Jormungandr Shop\r\nJÃ³rmungandr's Chest\r\nFrom: Maximus not Jeff\r\nSource: Jormungandr Shop\r\n";
                            //var r = Regex.Matches(fake_result_string, match_pattern, RegexOptions.Multiline);

                            var r = Regex.Matches(filtered_result_string, match_pattern, RegexOptions.Multiline);
                            if (r.Count > 0)
                            {
                                hasMatches = true;
                                foreach (Match m in r)
                                {
                                    var matchstring = m.Value;
                                    Loggio.Info("Ocr Correction", $"Directed incorrect word => {matchstring}");
                                }

                                if(hasMatches)
                                {
                                    filtered_result_string = Regex.Replace(filtered_result_string, match_pattern, correct);
                                    //fake_result_string = Regex.Replace(fake_result_string, match_pattern, correct);
                                    hasMatches = false;
                                }
                            }
                        }
                    }
                }

                //-- rebuild filtered_result_string and assign to filtered_result
                filtered_result_string = filtered_result_string.Replace("\r\n", "\n");
                filtered_result = filtered_result_string.Split('\n').ToList();
                filtered_result.Remove(String.Empty); 
                //-- let's ensure it follows how it should be read.
                ProcessingTextResult.Status = ProcessingStatus.OK;
                ProcessingTextResult.Message = "Success";
                ProcessingTextResult.ChestData = null;
                ProcessingTextResult.RawData = filtered_result;
                return ProcessingTextResult;
            }

            return null;
        }
        #endregion

        #region Dipose
        public void Dispose()
        {
            if(OcrCorrectionWords != null)
            {
                OcrCorrectionWords.Clear();
                OcrCorrectionWords = null;
            }
        }
        #endregion

    }
}
