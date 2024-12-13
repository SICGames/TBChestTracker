using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using TBChestTracker.Managers;


namespace TBChestTracker
{
    
    public class ClanDatabaseManager
    {
        private ClanDatabase _clandatabase = null;
        public ClanDatabase ClanDatabase
        {
            get
            {
                if (_clandatabase == null)
                    _clandatabase = new ClanDatabase();

                return _clandatabase;
            }
            set
            {
                if (_clandatabase == null) 
                    _clandatabase = new ClanDatabase();

                _clandatabase = value;
            }
        }

        public void Create(System.Action<bool> result)
        {
            var clanname = ClanDatabase.Clanname;
            var mainpath = SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder;

            if (String.IsNullOrEmpty(clanname) || clanname.Length < 3)
            {
                MessageBox.Show("Clan name must be more than three characters.");
                result(false);
                return;
            }

            var clanrootfolder = $"{mainpath}{clanname}";

            ClanDatabase.ClanFolderPath = clanname;
            ClanDatabase.ClanChestDatabaseExportFolderPath = $"\\exports";
            ClanDatabase.ClanDatabaseBackupFolderPath = $"\\backups";

            System.IO.Directory.CreateDirectory($"{clanrootfolder}");
            System.IO.Directory.CreateDirectory($"{clanrootfolder}{ClanDatabase.ClanDatabaseFolder}");
            System.IO.Directory.CreateDirectory($"{clanrootfolder}{ClanDatabase.ClanChestDatabaseExportFolderPath}");
            System.IO.Directory.CreateDirectory($"{clanrootfolder}{ClanDatabase.ClanDatabaseBackupFolderPath}");
            result( true ); 
        }
        public void Update()
        {

        }

        
        public void Move(string oldClanRoot,string newClanRoot)
        {
           // CopyDirectory(oldClanRoot, newClanRoot, true);
        }
        
        public void Delete()
        {

        }
        public void Save()
        {
            var root = $"{SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder}";
            var saveFilePath = $"{root}{ClanDatabase.ClanFolderPath}\\clan.cdb";
            using (System.IO.StreamWriter sw = System.IO.File.CreateText(saveFilePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(sw, ClanDatabase);
                sw.Close();
                sw.Dispose();
            }

            //--- in addition, we add it to recent files. 
            var recentdb = RecentlyOpenedListManager.Instance.RecentClanDatabases;
            
            //recentdb.Load();
            if (recentdb.Count > 0)
            {
                var recent_files = recentdb.Where(f => f.FullClanRootFolder.Equals(saveFilePath)).ToList();
                if(recent_files.Count>0)
                {
                    recent_files[0].LastOpened = DateTime.Now.ToFileTimeUtc().ToString();
                }
                else
                {
                    RecentClanDatabase recentClanDatabase = new RecentClanDatabase();
                    recentClanDatabase.ClanAbbreviations = ClanDatabase.ClanAbbreviations;
                    recentClanDatabase.ClanName = ClanDatabase.Clanname;

                    var position = StringHelpers.findNthOccurance(saveFilePath, Convert.ToChar(@"\"), 3);
                    var truncated = StringHelpers.truncate_file_name(saveFilePath, position);

                    recentClanDatabase.ShortClanRootFolder = truncated;
                    recentClanDatabase.FullClanRootFolder = saveFilePath;
                    recentClanDatabase.LastOpened = DateTime.Now.ToFileTimeUtc().ToString();
                    recentdb.Add(recentClanDatabase);
                    RecentlyOpenedListManager.Instance.Save();
                    //recentdb.Save();
                }
            }
        }

        public void Load(string file, ClanChestManager m_ClanChestManager, Action<bool> result)
        {
            //m_ClanChestManager.ClearData();
            m_ClanChestManager.Clear();

            if (File.Exists(file) == false)
            {
                MessageBox.Show($@"'{file}' does not exist.", "Clan Database Not Found");
                result(false);
            }

            var _clandatabase = new ClanDatabase();
            if(JsonHelper.TryLoad<ClanDatabase>(file, out _clandatabase))
            {
                ClanDatabase = _clandatabase;
                if(ClanDatabase != null)
                {
                    var r = m_ClanChestManager.BuildData();

                    if (r == ChestDataBuildResult.DATA_CORRUPT)
                    {
                        MessageBox.Show("Clan Chest Data is corrupted. Please run Validate Chest Data Integrity to fix this issue. Very important.", "Chest Data Corrupted");
                        result(true);
                    }
                    else if (r == ChestDataBuildResult.OK)
                    {
                        AppContext.Instance.NewClandatabaseBeenCreated = true;
                        CommandManager.InvalidateRequerySuggested();
                        if(AppContext.Instance.IsClanChestDatabaseUpgradeRequired)
                        {
                            if(MessageBox.Show("Clan Chest Database requires an update. To continue with updating the database, click on 'Ok'.") == MessageBoxResult.OK)
                            {
                                ClanChestDatabaseUpdaterWindow clanChestDatabaseUpdaterWindow = new ClanChestDatabaseUpdaterWindow();
                                clanChestDatabaseUpdaterWindow.Show();
                            }
                            else
                            {

                            }
                        }
                        result(true);
                    }
                }
                else
                {
                    result(false);
                }
            }
            else
            {
                result(false);
            }
            /*
            using (StreamReader sr = File.OpenText(file))
            {
                try
                {

                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    ClanDatabase = (ClanDatabase)serializer.Deserialize(sr, typeof(ClanDatabase));
                    if (ClanDatabase != null)
                    {
                        //-- parse prefixes if used 
                        //--- %MY_DOCUMENTS% - User Documents

                        m_ClanChestManager.BuildData();
                        AppContext.Instance.NewClandatabaseBeenCreated = true;
                        CommandManager.InvalidateRequerySuggested();
                        result(true);
                    }
                    else
                        result(false);
                }
                catch (Exception ex)
                {
                    result(false);
                }
            }
            */

        }

        public Dictionary<string, bool> ClanDatabasesToUpgrade()
        {
            var clanroot = SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder;
            var directories = System.IO.Directory.GetDirectories(clanroot);
            bool needsUpgrade = false;

            Dictionary<string, bool> DatabasesRequireUpgrade = new Dictionary<string, bool>();

            foreach (var directory in directories)
            {
                var files = Directory.GetFiles(directory, "clan.cdb");
                if(files.Length < 1)
                {
                    //-- we can not load this file because it doesn't exist.
                    continue;
                }

                var clandatabasefile = files[0];
                
                var tmp_Clandatabase = new ClanDatabase();

                using (StreamReader sr = File.OpenText(clandatabasefile))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;

                    tmp_Clandatabase = (ClanDatabase)serializer.Deserialize(sr, typeof(ClanDatabase));

                    if (tmp_Clandatabase != null)
                    {
                        //--- if tmp_Clandatabase doesn't have new changes applied. 
                        var version = tmp_Clandatabase.Version;

                        //-- was [documents folder]\TotalBattleChestTracker\AwesomeClan\
                        //-- now is AwesomeClan\\
                        var clanNameFolder = tmp_Clandatabase.ClanFolderPath;
                        var clanName = tmp_Clandatabase.Clanname;

                        if (DatabasesRequireUpgrade.ContainsKey(clandatabasefile) == false)
                        {
                            if (clanNameFolder.IndexOf(clanName) > 0)
                            {
                                DatabasesRequireUpgrade.Add(clandatabasefile, true);
                            }
                        }

                        if (DatabasesRequireUpgrade.ContainsKey(clandatabasefile) == false)
                        {
                            if (tmp_Clandatabase.Version != 2)
                            {
                                DatabasesRequireUpgrade.Add(clandatabasefile, true);
                            }
                        }
                    }
                    sr.Close();
                    sr.Dispose();
                }
                
            }
            return DatabasesRequireUpgrade;
        }

        public void UpgradeClanDatabases(Dictionary<string, bool> databases)
        {
            
            foreach(var database in databases)
            {
                var clandatabasefile = database.Key;
                if (databases[clandatabasefile] == true)
                {

                    var tmp_Clandatabase = new ClanDatabase();
                    var root = SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder;

                    using (StreamReader sr = File.OpenText(clandatabasefile))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Formatting = Formatting.Indented;

                        tmp_Clandatabase = (ClanDatabase)serializer.Deserialize(sr, typeof(ClanDatabase));

                        var clanroot = $"{root}{tmp_Clandatabase.Clanname}";

                        if (tmp_Clandatabase != null)
                        {
                            var clanname = tmp_Clandatabase.Clanname;
                            if(tmp_Clandatabase.ClanFolderPath.IndexOf(clanname) > 0) {
                              tmp_Clandatabase.ClanFolderPath =  tmp_Clandatabase.ClanFolderPath.Substring(tmp_Clandatabase.ClanFolderPath.LastIndexOf("\\") + 1);
                            }

                            if (tmp_Clandatabase.ClanChestDatabaseExportFolderPath.ToLower().Contains(clanroot.ToLower()))
                            {
                                tmp_Clandatabase.ClanChestDatabaseExportFolderPath = tmp_Clandatabase.ClanChestDatabaseExportFolderPath.Replace(clanroot, "");
                            }
                            if (tmp_Clandatabase.ClanDatabaseBackupFolderPath.ToLower().Contains(clanroot.ToLower()))
                            {
                                tmp_Clandatabase.ClanDatabaseBackupFolderPath = tmp_Clandatabase.ClanDatabaseBackupFolderPath.Replace(clanroot, "");
                            }
                            tmp_Clandatabase.Version = 2;
                        }
                        sr.Close();
                        sr.Dispose();
                    }

                    //-- now save the data.
                    using (StreamWriter sw = File.CreateText(clandatabasefile))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Formatting = Formatting.Indented;
                        serializer.Serialize(sw, tmp_Clandatabase);
                        sw.Close();
                        sw.Dispose();
                    }
                }
            }
        }
        public ClanDatabaseManager() { }
    }
}
