using CsvHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TBChestTracker.Managers;
using com.HellStormGames.Diagnostics;
using com.HellStormGames.Logging;

namespace TBChestTracker
{
    public class ClanChestExporter
    {
        //-- build export data for CSV file. 
        public List<ChestExportData> ChestExportDataCollection { get; private set; }
        public List<ChestsDatabase> ChestCollection { get; private set; }

        public string ExportFilename { get; private set; }
        public ExportSettings ExportSettings { get; private set; }
        private int InconsistentDates = 0;
        private Dictionary<string, Dictionary<String,ChestDataItem>> ChestDataItems;

        #region ChestCountExporter constructor and deconstructor
        public ClanChestExporter(string exportfilename, ExportSettings exportSettings)
        {
            ExportFilename = exportfilename;
            ExportSettings = exportSettings;
        }

        ~ClanChestExporter()
        {

            ExportFilename = String.Empty;
            if (ChestExportDataCollection != null)
            {
                ChestExportDataCollection.Clear();
                ChestExportDataCollection = null;
            }
            ExportSettings = null;

        }
        #endregion

        private bool LoadChestCollection()
        {
            ChestCollection ??= new List<ChestsDatabase>();
            if(ClanManager.Instance.ChestDataManager.Load())
            {
                ChestCollection = ClanManager.Instance.ChestDataManager.GetDatabase();
                return true;
            }
            return false;
        }
        private void InitChestExportData(bool bUsePoints = false)
        {
            ChestExportDataCollection.Clear();
            var extraHeaders = ExportSettings.ExtraHeaders;
            foreach (var item in ChestCollection)
            {
                ChestExportData chestExportData = new ChestExportData();
                bool contains_clanmate = ChestExportDataCollection.Any(d => d.Clanmate.Equals(item.Clanmate));

                if (contains_clanmate == false)
                {
                    chestExportData.Clanmate = item.Clanmate;
                    if (extraHeaders != null && extraHeaders.Count > 0)
                    {
                        foreach (var header in extraHeaders)
                        {
                            chestExportData.ExtraHeaders.Add(header, 0);
                        }
                    }

                    chestExportData.Total = 0;

                    if (bUsePoints)
                    {
                        chestExportData.PointsHeader.Add("Points", 0);
                    }

                    ChestExportDataCollection.Add(chestExportData);
                }
            }
        }
        private Dictionary<string, IList<ChestDataItem>> BuildChestData()
        {
            Dictionary<String, IList<ChestDataItem>> ChestData = new Dictionary<string, IList<ChestDataItem>>();
            if (ChestCollection != null)
            {
                foreach (var item in ChestCollection)
                {
                    var date = item.Date;
                    if(ChestData.ContainsKey(date) == false)
                    {
                        var clanmateGroups = ChestCollection.GroupBy(g => g.Clanmate);
                        
                        IList<ChestDataItem> chestDataItems = new List<ChestDataItem>();
                        
                        foreach (var clanmateGroup in clanmateGroups)
                        {
                            var clanmate = clanmateGroup.Key;
                            IList<Chest> chests = new List<Chest>();
                            var points = 0;
                            foreach (var chestInformation in clanmateGroup)
                            {

                                var chestname = chestInformation.ChestName;
                                var type = chestInformation.ChestType;
                                var level = chestInformation.ChestLevel;
                                points += chestInformation.ChestPoints;
                                Chest chest = new Chest(chestname, type, String.Empty, level);
                                chests.Add(chest);
                            }
                            ChestDataItem chestdataitem = new ChestDataItem();
                            
                            chestdataitem.chests = chests;
                            if(ClanManager.Instance.ClanChestSettings.GeneralClanSettings.ChestOptions == ChestOptions.UsePoints)
                            {
                                chestdataitem.Points = points;
                            }
                            else
                            {
                                chestdataitem.Points = 0;
                            }

                            chestDataItems.Add(chestdataitem);
                        }
                        
                        ChestData.Add(date, chestDataItems);
                    }
                }
            }
            return ChestData;
        }
        public bool Export()
        {
            var sortOptions = ExportSettings.SortOption;
            var dateTo = ExportSettings.DateRangeTo;
            var dateFrom = ExportSettings.DateRangeFrom;
            var dateRange = ExportSettings.DateRange;
            var extraHeaders = ExportSettings.ExtraHeaders;
            var clanSettings = ClanManager.Instance.ClanChestSettings;
            
            var ext = Path.GetExtension(ExportFilename);
            if (ext.ToLower().Contains("json"))
            {
                //-- no longer supported.
            }

            ChestExportDataCollection = new List<ChestExportData>();

            var bUsePoints = clanSettings.GeneralClanSettings.ChestOptions == ChestOptions.UsePoints ? true : false;

            LoadChestCollection();
            InitChestExportData(bUsePoints);

            ChestDataCollection chestdatacollection = new ChestDataCollection(ChestCollection);
            chestdatacollection.Build();
            ChestDataItems = chestdatacollection.Items;

            if (clanSettings.GeneralClanSettings.ChestOptions == ChestOptions.UseConditions)
            {
                //-- we build according to clan specified conditions.
                BuildChestsByConditions();
            }
            else if (clanSettings.GeneralClanSettings.ChestOptions == ChestOptions.UsePoints)
            {
                //--- we build chest points 
                BuildChestsByChestPoints();
            }
            else if (clanSettings.GeneralClanSettings.ChestOptions == ChestOptions.None)
            {
                //-- we build everything. 
                BuildAllChests();
            }

            var sortedChestExportData = SortChestData();

            try
            {
                WriteFile(sortedChestExportData);
            }
            catch (Exception ex)
            {
                if (ex is IOException)
                {
                    MessageBox.Show($"Couldn't write '{ExportFilename}' because it is accessed by another program. Close the program accessing that file and try again.");
                    return false;
                }
            }
            return true;

        }

        private void WriteFile(List<ChestExportData> chestdata)
        {
            var file = ExportFilename;

            using (var writer = new StreamWriter(file))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    var headers = new List<String>();
                    headers.Add("Clanmate");

                    if (chestdata[0].ExtraHeaders.Count > 0)
                    {

                        foreach (var extraheader in chestdata[0].ExtraHeaders)
                        {
                            headers.Add(extraheader.Key.ToString());
                        }
                    }

                    headers.Add("Total");
                    if (ClanManager.Instance.ClanChestSettings.GeneralClanSettings.ChestOptions == ChestOptions.UsePoints)
                    {
                        headers.Add("Points");
                    }

                    foreach (var heading in headers)
                    {
                        csv.WriteField(heading);
                    }

                    csv.NextRecord();

                    foreach (var item in chestdata)
                    {
                        csv.WriteField(item.Clanmate);

                        if (item.ExtraHeaders.Count > 0)
                        {
                            foreach (var extraHeader in item.ExtraHeaders)
                            {
                                var value = item.ExtraHeaders[extraHeader.Key];
                                csv.WriteField(value);
                            }
                        }

                        csv.WriteField(item.Total);
                        if (ClanManager.Instance.ClanChestSettings.GeneralClanSettings.ChestOptions == ChestOptions.UsePoints)
                        {
                            csv.WriteField(item.PointsHeader["Points"]);
                        }
                        csv.NextRecord();
                    }

                }
                writer.Close();
            }
        }

        private List<String> BuildDates()
        {
            var datesToSubtract = 0;
            var Dates = new List<string>();
            var isCustom = false;
            if (ExportSettings.DateRange == DateRangeEnum.Today)
            {
                datesToSubtract = 1;
            }
            else if (ExportSettings.DateRange == DateRangeEnum.Week)
            {
                datesToSubtract = 7;
            }
            else if (ExportSettings.DateRange == DateRangeEnum.Month)
            {
                datesToSubtract = 30;
            }
            else if (ExportSettings.DateRange == DateRangeEnum.Custom)
            {
                isCustom = true;
                TimeSpan diff = ExportSettings.DateRangeTo - ExportSettings.DateRangeFrom;
                datesToSubtract = diff.Days;

                var fromDate = ExportSettings.DateRangeFrom;
                var toDate = ExportSettings.DateRangeTo;

                if (datesToSubtract == 0)
                {
                    var dateStr = fromDate.ToString(AppContext.Instance.ForcedDateFormat);
                    Dates.Add(dateStr);
                }
                else
                {
                    for (var da = 0; da <= datesToSubtract; da++)
                    {
                        var d = toDate;
                        d = toDate.AddDays(-da);
                        var shortD = d.ToString(AppContext.Instance.ForcedDateFormat);
                        Dates.Add(shortD);
                    }
                }
            }

            if (isCustom == false)
            {
                var Today = DateTime.Now;
                for (var d = 0; d < datesToSubtract; d++)
                {
                    var previousDate = Today;
                    previousDate = Today.AddDays(-d);
                    var shortDateString = previousDate.ToString(AppContext.Instance.ForcedDateFormat);
                    Dates.Add(shortDateString);
                }
            }

            isCustom = false;
            return Dates;
        }
        
        private void BuildChestExportData(Dictionary<String,ChestDataItem> ChestDataItems, bool bUsePoints = false)
        {
            if (ChestDataItems != null)
            {
                foreach (var chestexportdata in ChestExportDataCollection.ToList())
                {
                    
                    var chestdataCollection = ChestDataItems.Where(name => name.Key.ToLower().Equals(chestexportdata.Clanmate.ToLower())).ToList();

                    if (chestdataCollection.Count() > 0)
                    {
                        var chestdata = chestdataCollection[0].Value;
                        
                        if (chestdata.chests != null)
                        {
                            foreach (var extraHeader in chestexportdata.ExtraHeaders.ToList())
                            {
                                foreach (var chest in chestdata.chests)
                                {
                                    var chestType = chest.Type;
                                    if (chestType.ToLower().Contains(extraHeader.Key.ToLower()))
                                    {
                                        var k = extraHeader.Key;
                                        chestexportdata.ExtraHeaders[k] += 1;
                                    }
                                }
                            }

                            chestexportdata.Total += chestdata.chests.Count();
                            if (bUsePoints)
                            {

                                chestexportdata.PointsHeader["Points"] += chestdata.Points;
                            }
                        }
                        else
                        {
                            chestexportdata.Total += 0;
                            if (bUsePoints)
                            {
                                chestexportdata.PointsHeader["Points"] += 0;
                            }
                        }
                        
                    }
                    
                }
            }
        }
        
        private void BuildAllChests()
        {
            var Dates = BuildDates();

            foreach (var date in Dates)
            {
                Dictionary<String, ChestDataItem> ChestDataItem = null;
                try
                {
                    ChestDataItem = ChestDataItems[date];
                }
                catch (Exception ex)
                {
                    InconsistentDates++;
                    var msg = $"{date} doesn't exist inside Clan Chest Data. Skipping.";
                    Loggio.Info("Invalid Dates", msg);
                    break;
                }

                BuildChestExportData(ChestDataItem);
            }

        }

        private void BuildChestsByConditions()
        {

            var conditionalchestdata = new ChestDataConditionalCollection(ChestCollection, ClanManager.Instance.ClanChestSettings.ChestRequirements);
            conditionalchestdata.Build();
            var conditionalchests = conditionalchestdata.Items;
            var Dates = BuildDates();

            foreach (var date in Dates)
            {
                Dictionary<String, ChestDataItem> ChestItems = null;
                try
                {
                    ChestItems = ChestDataItems[date];
                }
                catch (Exception ex)
                {
                    InconsistentDates++;
                    var msg = $"{date} doesn't exist inside Clan Chest Data. Skipping.";
                    Loggio.Info(ex, "Invalid Dates", msg);
                    break;
                }

                BuildChestExportData(ChestItems, false);
            }
        }

        private void BuildChestsByChestPoints()
        {
            var Dates = BuildDates();

            foreach (var date in Dates)
            {
                Dictionary<String, ChestDataItem> ChestItems = null;
                try
                {
                    ChestItems = ChestDataItems[date];
                }
                catch (Exception ex)
                {
                    InconsistentDates++;
                    var msg = $"{date} doesn't exist inside Clan Chest Data. Skipping.";
                    Loggio.Error(ex, "Invalid Dates", msg);
                    continue; //-- skip not break. Break exits for loop. 
                }

                if (ChestItems == null)
                {
                    //-- we have something critical and this should not be null. 
                    Loggio.Warn("DailyChestData shouldn't be null. There's a huge problem if it's getting to this point.");
                    throw new Exception("DailyChestData shouldn't be null. There's a huge problem if it's getting to this point.");
                }

                BuildChestExportData(ChestItems, true);
            }
        }

        private List<ChestExportData> SortChestData()
        {
            var result = new List<ChestExportData>();
            ClanChestDataComparator clanChestDataComparator = new ClanChestDataComparator();

            if (ExportSettings.SortOption == SortType.ASCENDING)
            {
                if (ClanManager.Instance.ClanChestSettings.GeneralClanSettings.ChestOptions == ChestOptions.UsePoints)
                {
                    result = ChestExportDataCollection.OrderBy(i => i.PointsHeader["Points"]).ToList();
                }
                else
                {
                    result = ChestExportDataCollection.OrderBy(i => i.Total).ToList();
                }
            }
            else if (ExportSettings.SortOption == SortType.DESENDING)
            {
                if (ClanManager.Instance.ClanChestSettings.GeneralClanSettings.ChestOptions == ChestOptions.UsePoints)
                {
                    result = ChestExportDataCollection.OrderByDescending(i => i.PointsHeader["Points"]).ToList();
                }
                else
                {
                    result = ChestExportDataCollection.OrderByDescending(i => i.Total).ToList();
                }
            }
            else if (ExportSettings.SortOption == SortType.NONE)
            {
                result = ChestExportDataCollection;
            }

            return result;
        }
    }        
}
