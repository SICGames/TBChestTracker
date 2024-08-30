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

            ClanDatabase.ClanFolderPath = clanrootfolder;
            ClanDatabase.ClanDatabaseFolder = $"\\db";
            ClanDatabase.ClanChestReportFolderPath = $"\\reports";
            ClanDatabase.ClanChestDatabaseExportFolderPath = $"\\exports";
            ClanDatabase.ClanDatabaseBackupFolderPath = $"\\backups";

            System.IO.Directory.CreateDirectory($"{ClanDatabase.ClanFolderPath}");
            System.IO.Directory.CreateDirectory($"{ClanDatabase.ClanFolderPath}{ClanDatabase.ClanDatabaseFolder}");
            System.IO.Directory.CreateDirectory($"{ClanDatabase.ClanFolderPath}{ClanDatabase.ClanChestReportFolderPath}");
            System.IO.Directory.CreateDirectory($"{ClanDatabase.ClanFolderPath}{ClanDatabase.ClanChestDatabaseExportFolderPath}");
            System.IO.Directory.CreateDirectory($"{ClanDatabase.ClanFolderPath}{ClanDatabase.ClanDatabaseBackupFolderPath}");
            result( true ); 
        }
        public void Update()
        {

        }
        public void Move()
        {

        }
        public void Delete()
        {

        }
        public void Save()
        {
            var saveFilePath = $"{ClanDatabase.ClanFolderPath}\\clan.cdb";
            using (System.IO.StreamWriter sw = System.IO.File.CreateText(saveFilePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(sw, ClanDatabase);
                sw.Close();
                sw.Dispose();
            }

            //--- in addition, we add it to recent files. 
            var recentdb = new RecentDatabase();
            recentdb.Load();
            if (recentdb.RecentClanDatabases.Count > 0)
            {
                var recent_files = recentdb.RecentClanDatabases.Where(f => f.FullClanRootFolder.Equals(saveFilePath)).ToList();
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
                    recentdb.RecentClanDatabases.Add(recentClanDatabase);
                    recentdb.Save();
                }
            }
        }

        public void Load(string file, ClanChestManager m_ClanChestManager, Action<bool> result)
        {
            m_ClanChestManager.ClearData();

            using (StreamReader sr = File.OpenText(file))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                ClanDatabase = (ClanDatabase)serializer.Deserialize(sr, typeof(ClanDatabase));
                if (ClanDatabase != null)
                {
                    //-- parse prefixes if used 
                    //--- %MY_DOCUMENTS% - User Documents
                    
                    m_ClanChestManager.BuildData();
                    GlobalDeclarations.hasNewClanDatabaseCreated = true;
                    CommandManager.InvalidateRequerySuggested();
                    result(true);
                }
                else
                    result(false);
            }
        }

        public Dictionary<string, bool> ClanDatabasesToUpgrade()
        {
            var clanroot = SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder;
            var directories = System.IO.Directory.GetDirectories(clanroot);
            bool needsUpgrade = false;
            Dictionary<string, bool> DatabasesRequireUpgrade = new Dictionary<string, bool>();

            foreach (var directory in directories)
            {
                var clandatabasefile = $@"{directory}\clan.cdb";
                var tmp_Clandatabase = new ClanDatabase();

                using (StreamReader sr = File.OpenText(clandatabasefile))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;

                    tmp_Clandatabase = (ClanDatabase)serializer.Deserialize(sr, typeof(ClanDatabase));

                    if (tmp_Clandatabase != null)
                    {
                        if(tmp_Clandatabase.Version != 2)
                        {
                            DatabasesRequireUpgrade.Add(clandatabasefile, true);
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
                    var clanroot = SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder;

                    using (StreamReader sr = File.OpenText(clandatabasefile))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Formatting = Formatting.Indented;

                        tmp_Clandatabase = (ClanDatabase)serializer.Deserialize(sr, typeof(ClanDatabase));
                        clanroot = $"{clanroot}{tmp_Clandatabase.Clanname}";
                        if (tmp_Clandatabase != null)
                        {
                            if (tmp_Clandatabase.ClanChestDatabaseExportFolderPath.ToLower().Contains(clanroot.ToLower()))
                            {
                                tmp_Clandatabase.ClanChestDatabaseExportFolderPath = tmp_Clandatabase.ClanChestDatabaseExportFolderPath.Replace(clanroot, "");
                            }
                            if (tmp_Clandatabase.ClanChestReportFolderPath.ToLower().Contains(clanroot.ToLower()))
                            {
                                tmp_Clandatabase.ClanChestReportFolderPath= tmp_Clandatabase.ClanChestReportFolderPath.Replace(clanroot, "");
                            }
                            if (tmp_Clandatabase.ClanDatabaseBackupFolderPath.ToLower().Contains(clanroot.ToLower()))
                            {
                                tmp_Clandatabase.ClanDatabaseBackupFolderPath = tmp_Clandatabase.ClanDatabaseBackupFolderPath.Replace(clanroot, "");
                            }
                            if (tmp_Clandatabase.ClanDatabaseFolder.ToLower().Contains(clanroot.ToLower()))
                            {
                                tmp_Clandatabase.ClanDatabaseFolder = tmp_Clandatabase.ClanDatabaseFolder.Replace(clanroot, "");
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
