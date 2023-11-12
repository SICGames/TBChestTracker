using com.HellStormGames.Logging;
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
using Windows.Media.Ocr;
using Windows.Networking.Proximity;

namespace TBChestTracker
{

    public class ClanChestManager
    {
        public List<ClanChestData> clanChestData { get; set; }

        public ClanmateManager ClanmateManager { get; set; }

        public Dictionary<String, List<ClanChestData>> ClanChestDailyData;
        
        public ClanChestManager()
        {

            clanChestData = new List<ClanChestData>();  
            ClanChestDailyData = new Dictionary<string, List<ClanChestData>>();
            ClanmateManager = new ClanmateManager(); 
        }
        public void ClearData()
        {
            ClanChestDailyData.Clear();
            clanChestData.Clear();
            ClanmateManager.Clanmates.Clear();  
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
                        Debug.WriteLine($"{name} was successfully removed from clanchestdata.");
                    }
                }
            }
            //--- we're done.
            SaveData();
            return;
        }
        private List<ChestData> ProcessText(OcrResult result)
        {
            List<ChestData> tmpchests = new List<ChestData>();

            for (var x = 0; x < result.Lines.Count; x += 3)
            {
                var word = result.Lines[x].Text;
                if (word == null)
                    continue;

                if (!word.Contains("Clan"))
                {
                    var chestName = result.Lines[x + 0].Text;
                    var clanmate = result.Lines[x + 1].Text;
                    var chestobtained = result.Lines[x + 2].Text;

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
                        if (levelStartPos > 0)
                        {
                            chestobtained = chestobtained.Substring(levelStartPos).ToLower();
                            ChestType type = ChestType.COMMON;

                            if (chestobtained.Contains("epic"))
                                type = ChestType.EPIC;
                            else if (chestobtained.Contains("rare"))
                                type = ChestType.RARE;
                            else if (chestobtained.Contains("heroic"))
                                type = ChestType.HEROIC;
                            else if (chestobtained.Contains("citadel"))
                                type = ChestType.CITADEL;
                            else
                                type = ChestType.COMMON;

                            int level = 0;

                            if (chestobtained.Contains("io"))
                            {
                                //--- OCR reads 10 as io.Until perm fix. 
                                level = 10;
                            }
                            else if (chestobtained.Contains("-"))
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

                            tmpchests.Add(new ChestData(clanmate, new Chest(chestName, type, level)));
                        }
                        else
                        {
                            if (chestobtained.Contains("arena"))
                            {
                                tmpchests.Add(new ChestData(clanmate, new Chest(chestName, ChestType.ARENA, 5)));
                                continue;
                            }
                            else if (chestobtained.Contains("bank"))
                            {
                                tmpchests.Add(new ChestData(clanmate, new Chest(chestName, ChestType.BANK, 0)));
                                continue;
                            }
                            else if (chestobtained.Contains("union"))
                            {
                                tmpchests.Add(new ChestData(clanmate, new Chest(chestName, ChestType.UNION_TRIUMPH, 0)));
                                continue;
                            }
                            else if (chestobtained.Contains("mimic"))
                            {
                                tmpchests.Add(new ChestData(clanmate, new Chest(chestName, ChestType.MIMIC, 0)));
                                continue;
                            }
                            else if (chestobtained.Contains("rise"))
                            {
                                tmpchests.Add(new ChestData(clanmate, new Chest(chestName, ChestType.ANCIENT_EVENT, 0)));
                            }
                            else if (chestobtained.Contains("story"))
                            {
                                tmpchests.Add(new ChestData(clanmate, new Chest(chestName, ChestType.STORY, 0)));
                            }
                            else if (chestobtained.Contains("wealth"))
                                tmpchests.Add(new ChestData(clanmate, new Chest(chestName, ChestType.WEALTH, 0)));
                            else
                            {
                                //-- good chance this could be an uncounted chest.
                                tmpchests.Add(new ChestData(clanmate, new Chest(chestName, ChestType.OTHER, 0)));
                            }
                        }
                    }
                }
            }

            return tmpchests;
        }

        private void ProcessChestConditions(ref List<ChestData> chestdata)
        {
            //--- if chest requirements are using conditions, we use those conditions. If not, we continue.
            if (ClanChestSettings.ChestRequirements.useChestConditions)
            {
                List<ChestData> validated = new List<ChestData>();

                foreach (var ttmpChest in chestdata.ToList())
                {
                    var isValidated = false;
                    foreach (var condition in ClanChestSettings.ChestRequirements.ChestConditions)
                    {
                        var sChestType = ttmpChest.Chest.Type.ToString().ToLower();

                        if (sChestType.Equals(condition.ChestType, StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (condition.Comparator.ToLower() == "greater than")
                            {
                                if (ttmpChest.Chest.Level >= condition.level)
                                {
                                    validated.Add(new ChestData(ttmpChest.Clanmate, ttmpChest.Chest));
                                    Debug.WriteLine($"[[Level {ttmpChest.Chest.Level} ({ttmpChest.Chest.Type.ToString()}) {ttmpChest.Chest.Name} from {ttmpChest.Clanmate} validated.]]");
                                    Loggy.Write($"[[Level {ttmpChest.Chest.Level} ({ttmpChest.Chest.Type.ToString()}) {ttmpChest.Chest.Name} from {ttmpChest.Clanmate} validated.]]", LogType.LOG);

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

        public async void ProcessChestData(OcrResult result)
        {

            /*
             * TmpChests gather processed chests.
             * Stuffs into temporary clanchestdata
             * Need to make sure we don't need to start new entry in ClanChestDaily.
             * ClanChestData should be obsolete.
            */

            GlobalDeclarations.isBusyProcessingClanchests = true;
            var resulttext = result.Text;

            //- doesn't handle expired gifts. Will patch soon.
            if (!resulttext.Contains("No gifts"))
            {
                GlobalDeclarations.isAnyGiftsAvailable = true;
                List<ChestData> tmpchests = ProcessText(result);
                ProcessChestConditions(ref tmpchests);
                List<ClanChestData> tmpchestdata = CreateClanChestData(tmpchests);  

                var names = ClanmateManager.Clanmates.Select(n => n.Name);

                //-- do we need to insert new entry?
                //-- causes midnight bug.
                var currentdate = DateTime.Now.ToString(@"MM-dd-yyyy");
                var dates = ClanChestDailyData.Where(x => x.Key.Equals(currentdate));

                if (dates.Count() == 0)
                {
                    clanChestData.Clear();
                    foreach (var name in names)
                    {
                        clanChestData.Add(new ClanChestData(name, null));
                    }

                    ClanChestDailyData.Add(DateTime.Now.ToString(@"MM-dd-yyyy"), clanChestData);
                }

                //-- make sure clanmate exists.
                foreach (var tmpchest in tmpchestdata)
                {
                    bool exists = names.Contains(tmpchest.Clanmate, StringComparer.OrdinalIgnoreCase);
                    if (!exists)
                    {
                        Debug.WriteLine($"{tmpchest.Clanmate} doesn't exist within clanmates database.");
                        clanChestData.Add(new ClanChestData(tmpchest.Clanmate, tmpchest.chests));
                        ClanmateManager.Add(tmpchest.Clanmate);
                        ClanmateManager.Save(ClanDatabaseManager.ClanDatabase.ClanmateDatabaseFile);
                    }
                    else
                        continue;
                }

                //-- update clanchestdata
                foreach (var chestdata in clanChestData)
                {
                    var _chestdata = tmpchestdata.Where(name => name.Clanmate.Equals(chestdata.Clanmate,
                        StringComparison.CurrentCultureIgnoreCase)).Select(chests => chests).ToList();

                    if (_chestdata.Count > 0)
                    {
                        var m_chestdata = _chestdata[0];

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
            }
            else
            {
                GlobalDeclarations.isAnyGiftsAvailable = false;
                Debug.WriteLine("No gifts available.");
            }

            GlobalDeclarations.isBusyProcessingClanchests = false;
            GlobalDeclarations.canCaptureAgain = true;

            return;
        }
        
        public async void BuildData()
        {
            var clanmatefile = ClanDatabaseManager.ClanDatabase.ClanmateDatabaseFile;
            var clanchestfile = ClanDatabaseManager.ClanDatabase.ClanChestDatabaseFile;
            var chestrequirementfile = ClanDatabaseManager.ClanDatabase.ClanChestRequirementsFile;

            string[] clanmates = null;

            if (ClanmateManager.Clanmates != null)
                ClanmateManager.Clanmates.Clear();

            if (!System.IO.File.Exists(clanmatefile))
            {
                GlobalDeclarations.hasClanmatesBeenAdded = false;
                return;
            }
            else
            {
                using (var sr = File.OpenText(clanmatefile))
                {
                    string data = await sr.ReadToEndAsync();
                    if(data.Contains("\r\n")) 
                    {
                        data = data.Replace("\r\n", ",");
                    }
                    else 
                        data = data.Replace("\n", ",");

                    clanmates = data.Split(',');
                }

                foreach (var clanmate in clanmates)
                {
                    if (!String.IsNullOrEmpty(clanmate))
                    {
                        ClanmateManager.Add(clanmate);
                    }
                }
            }

            //--- build blank clanchestdata 
            foreach (var clanmate in clanmates)
            {
                if (!String.IsNullOrEmpty(clanmate))
                {
                    clanChestData.Add(new ClanChestData(clanmate, null));
                }
            }

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

            //--- midnight bug may occur here.
            var lastDate = ClanChestDailyData.Keys.Last();
            var dateStr = DateTime.Now.ToString(@"MM-dd-yyyy");
            if (lastDate.Equals(dateStr))
            {
                clanChestData = ClanChestDailyData[lastDate];
            }
            else
            {
                ClanChestDailyData.Add(DateTime.Now.ToString(@"MM-dd-yyyy"), clanChestData);
            }
            GlobalDeclarations.hasClanmatesBeenAdded = true;

            //-- load chest requirements
            if(!System.IO.File.Exists(chestrequirementfile))
            {
                if(ClanChestSettings.ChestRequirements == null)
                {
                    ClanChestSettings.ChestRequirements = new ChestRequirements();
                    ClanChestSettings.InitSettings();   
                }
                ClanChestSettings.SaveSettings(chestrequirementfile);
            }
            else
            {
                if(ClanChestSettings.ChestRequirements == null)
                    ClanChestSettings.ChestRequirements = new ChestRequirements();

                ClanChestSettings.LoadSettings(chestrequirementfile);
            }

            return;
        }

        public void SaveData()
        {
            //-- write to file.

            string file = ClanDatabaseManager.ClanDatabase.ClanChestDatabaseFile;
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
                Debug.WriteLine(ex.Message);
            }
        }
        public void CreateBackup()
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.Now;
            string file = $"{ClanDatabaseManager.ClanDatabase.ClanDatabaseBackupFolderPath}//clanchest_backup_{dateTimeOffset.ToUnixTimeSeconds()}.db";
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
                Debug.WriteLine(ex.Message);
            }
        }
        public Task SaveDataTask()
        {
            return Task.Run(()=>SaveData());
        }
        public Task ExportDataAsync(string filename, SortType sortType) => Task.Run(() =>  ExportData(filename, sortType));
        public void ExportData(string filename, SortType sortType = SortType.NONE)
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
                    foreach (var clanmate in ClanmateManager.Clanmates)
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

                    foreach (var chestcount in chestcountdata)
                    {
                        //--- 180 chests bug. Game doesn't like 180 number posted. Not sure why.
                        var chests = chestcount.Count == 180 ? chestcount.Count+1 : chestcount.Count;
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
