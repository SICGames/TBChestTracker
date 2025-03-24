using com.HellStormGames.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TBChestTracker.Managers;

namespace TBChestTracker
{
    public class TempChestDataBuilder : IChestDataBuilder
    {
        readonly IChestDataBuildConfiguration configuration;
        public IChestDataBuildConfiguration BuildConfiguration => configuration;
        public TempChestDataBuilder( IChestDataBuildConfiguration config)
        {
            this.configuration = config;
        }

        public async Task Build()
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var chestdatafile = $"{ClanManager.Instance.CurrentProjectDirectory}\\Chests\\ChestData.csv";
            //--- somehow for whatever reason writes duplicates
            var errorOccured = false;
            ProcessingTextResult result = null;

            var chestcollection = new List<ChestsDatabase>();
            if (File.Exists(chestdatafile))
            {
                ClanManager.Instance.ChestDataManager.Load(chestdatafile);
                chestcollection = ClanManager.Instance.ChestDataManager.GetDatabase();
            }
            
            foreach (var file in configuration.Files.ToList())
            {
                result = await BuildFromFile(file, configuration.BuildingProgress);
                if (result != null)
                {
                    if (result.Status != ProcessingStatus.OK)
                    {
                        //-- possibly cancel and present error
                        errorOccured = true;
                        continue;
                    }

                    if (errorOccured == false)
                    {
                        var temp_chestcollection = await FinalizeChestBuilding(file, result, configuration.BuildingProgress);

                        chestcollection.AddRange(temp_chestcollection);
                        AppContext.Instance.isBusyProcessingClanchests = false;
                        result.ChestData.Clear();
                        result.RawData.Clear();
                        result = null;

                    }
                }
                await Task.Delay(5);
            }

            if (errorOccured)
            {
                if (result != null)
                {
                    result?.ChestData?.Clear();
                    result?.RawData?.Clear();
                    result = null;
                }
            }

            if (errorOccured == false)
            {
               
                await SaveChestDataToFile(chestdatafile, configuration.BuildingProgress, chestcollection);
                await Task.Delay(100);
                var p2 = new BuildingChestsProgress($"Finished Building Clan Chests...", -1, 1, 1, true, false);
                configuration.BuildingProgress.Report(p2);
            }
        }


        #region Build
        private async Task<ProcessingTextResult> BuildFromFile(string file, IProgress<BuildingChestsProgress> progress)
        {
            var chestboxes = await PrepareChestBoxesFromFile(file, progress);
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

        private async Task<List<ChestBox>> PrepareChestBoxesFromFile(string filename, IProgress<BuildingChestsProgress> progress)
        {
            bool errorOccurred = false;

            if (File.Exists($"{AppContext.Instance.AppFolder}ChestBoxBuilder.exe") == false)
            {
                var ee = new BuildingChestsProgress("ChestBoxBuilder.exe can not be located. Ensure it is inside program's directory.", -1, 0, 0, false, true);
                progress.Report(ee);
                return null;
            }
            List<ChestBox> boxes = new List<ChestBox>();

            if (File.Exists(filename) == false)
            {
                var ee = new BuildingChestsProgress($"{filename} does not exist as it should. Will not continue.", -1, 0, 0, false, true);
                progress.Report(ee);
                await Task.Delay(50);
                return null;
            }

            var e = new BuildingChestsProgress("Starting ChestBoxBuilder....", -1, 0, 0, false);
            progress.Report(e);
            await Task.Delay(100);

            var outputFile = filename;

            outputFile = outputFile.Replace(Path.GetExtension(outputFile), ".chestbox");

            var outputfilepath = $"{ClanManager.Instance.CurrentProjectDirectory}\\Chests\\ChestBoxes";
            var di = new DirectoryInfo(outputfilepath);
            if (di.Exists == false)
            {
                di.Create();
            }

            var outputFilename = outputFile.Substring(outputFile.LastIndexOf("\\") + 1);
            outputFile = $"{outputfilepath}\\temp_{outputFilename}";

            var chestboxbuilderPath = $"{AppContext.Instance.AppFolder}ChestBoxBuilder.exe";

            string infile = "\"" + filename + "\"";
            string outfile = "\"" + outputFile + "\"";
            string language = "\"en_US\"";

            var chestboxbuilderArgs = $@"-i {infile} -o {outfile} -L {language}";
            ConsoleInterop consoleInterop = new ConsoleInterop(chestboxbuilderPath, chestboxbuilderArgs, AppContext.Instance.AppFolder);

            consoleInterop.Completed += (s, e) =>
            {
                bool outputFileExists = false;
                if (e.isCompleted == true)
                {
                    outputFileExists = File.Exists(outputFile);

                    if (outputFileExists == false)
                    {
                        var ef = new BuildingChestsProgress($"Error occured. Somehow the output file '{outputFile}' does not exist or can not be accessed.", -1, 0, 0, false, true);
                        progress.Report(ef);
                        Task.Delay(100);
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
                if (String.IsNullOrEmpty((data)) == false)
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
                Task.Delay(100);
                return;

            };

            CancellationTokenSource cts = new CancellationTokenSource();

            bool result = await consoleInterop.Run(cts.Token);
            if (result == false)
            {
                var x = new BuildingChestsProgress("Failed Starting ChestBoxBuilder....", -1, 0, 0, false, true);
                progress.Report(x);
                await Task.Delay(100);
                return null;
            }
            if (errorOccurred)
            {
                var ea = new BuildingChestsProgress(error_msg, -1, 0, 0, false, true);
                progress.Report(ea);
                await Task.Delay(250);
                return null;
            }

            return boxes;
        }
        #endregion

        private bool CleanTemporaryChests()
        {
            try {
                var Tempfolder = $"{ClanManager.Instance.CurrentProjectDirectory}\\Chests\\Temp";
                var tempDi = new DirectoryInfo(Tempfolder);
                if (tempDi.Exists)
                {
                    tempDi.Delete(true);
                }
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        private bool MergeChestDataFile()
        {
            try
            {
                var chestsFolder = $"{ClanManager.Instance.CurrentProjectDirectory}\\Chests\\Data\\";
                foreach(var files in configuration.Files.ToList())
                {
                    var filename = files.Substring(files.LastIndexOf("\\") + 1);
                    var masterFile = $"{chestsFolder}{filename}";
                    using (var reader = new StreamReader(files))
                    {
                        var data = reader.ReadToEnd();
                        using (var writer = new StreamWriter(masterFile, true))
                        {
                            writer.Write(data);
                            writer.Close();
                        }
                        reader.Close();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private async Task SaveChestDataToFile(string file, IProgress<BuildingChestsProgress> progress, List<ChestsDatabase> chestcollection)
        {
            //-- we overwrite the ChestData.csv file. 
            await UpdateUI(new BuildingChestsProgress($"Saving Chest Data to ChestData.csv.", -1, 1, 0, false),
                    progress);
            await Task.Delay(100);
            ClanManager.Instance.ChestDataManager ??= new ChestDataManager();

            ClanManager.Instance.ChestDataManager.Save(file, chestcollection);

            await UpdateUI(new BuildingChestsProgress($"Cleaning up temporary chests.", -1, 1, 0, false),
                    progress);
            await Task.Delay(250);

            MergeChestDataFile();

            await UpdateUI(new BuildingChestsProgress($"Cleaning up temporary chests.", -1, 1, 0, false),
                    progress);

            await Task.Delay(250);
            
            CleanTemporaryChests();

            await UpdateUI(new BuildingChestsProgress($"Saved Chest Data to ChestData.csv.", -1, 1, 1, true),
                    progress);
            await Task.Delay(100);
        }

        private async Task<List<ChestsDatabase>> FinalizeChestBuilding(string file, ProcessingTextResult result, IProgress<BuildingChestsProgress> progress)
        {
            //-- we no longer depend on using JSON or the older Database since it's flawed. 
            var tmpchestsdata = result.ChestData;
            int currentTempChestDataCount = 0;
            int maxTempChestDataCount = tmpchestsdata.Count;

            var datestring = file.Substring(file.LastIndexOf("_") + 1);
            datestring = datestring.Substring(0, datestring.LastIndexOf("."));
            List<ChestsDatabase> chestcollection = new List<ChestsDatabase>();
            foreach (var chestdata in tmpchestsdata)
            {
                var clanmate = chestdata.Clanmate;
                var chestName = chestdata.Chest.Name;
                var chestType = chestdata.Chest.Type;
                var chestSource = chestdata.Chest.Source;
                var chestLevel = chestdata.Chest.Level;

                await UpdateUI(new BuildingChestsProgress($"Finializing Chest Data for {datestring}", -1, tmpchestsdata.Count, currentTempChestDataCount, false),
                    progress);

                ChestsDatabase tmpdatabase = new ChestsDatabase();
                tmpdatabase.Date = datestring;
                tmpdatabase.Clanmate = clanmate;
                tmpdatabase.ChestName = chestName;
                tmpdatabase.ChestLevel = chestLevel;
                tmpdatabase.ChestType = chestType;
                bool useChestPoints = ClanManager.Instance.ClanChestSettings.GeneralClanSettings.ChestOptions == ChestOptions.UsePoints;
                int chestpoint = 0;
                if (useChestPoints)
                {
                    chestpoint = ChestsDatabase.GetChestPointValue(chestdata.Chest, ClanManager.Instance.ClanChestSettings).Value;
                }
                tmpdatabase.ChestPoints = chestpoint;

                chestcollection?.Add(tmpdatabase);

                currentTempChestDataCount++;
                await Task.Delay(50);
            }
            return chestcollection;
        }

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

        #endregion
        private async Task<ProcessingTextResult> ProcessChestBoxes(List<ChestBox> chestboxes, IProgress<BuildingChestsProgress> progress = null)
        {
            var progress_result = new ProcessingTextResult();
            bool isError = false;

            var currentChestbox = 0;

            var tmpchests = new List<ChestData>();

            //-- last element is empty. Clean it up.
            foreach (var cb in chestboxes.ToList())
            {
                if (cb == null || cb.Content == null || cb.Content.Count == 0)
                {
                    chestboxes.Remove(cb);
                }
            }

            var maxChestBox = chestboxes.Count;

            foreach (var chestbox in chestboxes)
            {
                var pra = new BuildingChestsProgress($"Processing Chest Boxes ({currentChestbox}/{maxChestBox})...", -1, maxChestBox, currentChestbox, false);
                progress.Report(pra);
                var chestdatastr = chestbox.ToString();
                if (String.IsNullOrEmpty(chestdatastr))
                {
                    //-- shouldn't really get this far
                    progress_result.Status = ProcessingStatus.UNKNOWN_ERROR;
                    progress_result.Message = $"There's a snag and it shouldn't have happened. Chest Box Content is empty. Not possible.";
                    Loggio.Warn("Chest Processor", "ChestBox Content Is Null. This shouldn't have happened.");
                    isError = true;
                    break;
                }

                ChestData chestdata = null;
                ChestData.TryParse(chestdatastr, out chestdata);
                if (chestdata != null)
                {
                    Loggio.Info("OCR Result", $"'{chestdata.Clanmate}' has obtained '{chestdata.Chest.Name}' from level {chestdata.Chest.Level} '{chestdata.Chest.Type}' chest.");
                    tmpchests.Add(chestdata);
                }

                currentChestbox++;
                await Task.Delay(50);
            }
            if (isError)
            {
                progress_result.ChestData = tmpchests;
                return progress_result;
            }

            progress_result.Status = ProcessingStatus.OK;
            progress_result.Message = "Success";
            progress_result.ChestData = tmpchests;

            return progress_result;
        }

        public void Dispose()
        {
            configuration.Dispose();
        }
    }
}
