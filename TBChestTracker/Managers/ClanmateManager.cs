using Emgu.CV.Features2D;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Windows;
using TBChestTracker.Managers;

namespace TBChestTracker
{
    public class ClanmateManager 
    {
        public ClanmatesDatabase Database { get; set; }

        public void UpdateCount()
        {
            Database.NumClanmates = Database.Clanmates.Count;
        }
        public ClanmateManager() 
        {
            if (Database == null)
            {
                Database = new ClanmatesDatabase();
                Database.Clanmates = new ObservableCollection<Clanmate>();
            }
        }
        public void Add(string name)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                Database.Clanmates.Add(new Clanmate(name));
                UpdateCount();
            });
        }
        public void AddAliases(string name, List<string> aliases)
        {
            foreach(var mate in Database.Clanmates.ToList())
            {
                if(mate.Name.ToLower().Equals(name.ToLower()))
                {
                    mate.Aliases.InsertRange(mate.Aliases.Count, aliases);
                }
            }
            UpdateCount();
        }
        public void Remove(string name)
        {
            foreach(var c in Database.Clanmates.ToList())
            {
                if (c.Name == name)
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        Database.Clanmates.Remove(c);
                    });
                }
            }
            UpdateCount();  
        }
        public async void ImportFromFileAsync(string path)
        {
            using(TextReader  reader = new StreamReader(path)) 
            {
                string data = await reader.ReadToEndAsync();
                if (data.Contains("\r\n"))
                {
                    data = data.Replace("\r\n", ",");
                }
                else
                {
                    data = data.Replace("\n", ",");
                }

                data = data.Substring(0, data.LastIndexOf(","));

                string[] dataCollection = data.Split(',');
                Database.Clanmates.Clear();
                foreach (var c in dataCollection)
                {
                    if(!String.IsNullOrEmpty(c))
                        Database.Clanmates.Add(new Clanmate(c));
                }
                reader.Close();
                UpdateCount();
            }
        }


   
        public bool Load(string path)
        {
            bool requiresUpdate = false;
            string backuptext = String.Empty;

            Database.Clanmates.Clear();
            Database.NumClanmates = Database.Clanmates.Count;
            ClanmatesDatabase clanmatesDatabase = new ClanmatesDatabase();

            bool loaded = JsonHelper.TryLoad<ClanmatesDatabase>(path,out clanmatesDatabase);
            if(loaded)
            {
                Database = clanmatesDatabase;
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public void Save()
        {
            var clanFolder = $@"{ClanManager.Instance.CurrentProjectDirectory}";
            var clanmatedb = $@"{clanFolder}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanmateDatabaseFile}";
            using (StreamWriter sw = File.CreateText(clanmatedb))
            {
                string jsondata = StringHelpers.ConvertToUTF8(JsonConvert.SerializeObject(Database, Formatting.Indented));
                sw.Write(jsondata);
                sw.Close();
            }
        }
        
        private void SaveAs(string path)
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                string jsondata = StringHelpers.ConvertToUTF8(JsonConvert.SerializeObject(Database, Formatting.Indented));
                sw.Write(jsondata);
                sw.Close();
            }
        }
        public void CreateBackup()
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.Now;
            var root = $"{SettingsManager.Instance.Settings.GeneralSettings.ClanRootFolder}";
            var clanfolder = $"{root}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanFolderPath}";


            var clanmateBackupFolder = $"{clanfolder}{ClanManager.Instance.ClanDatabaseManager.ClanDatabase.ClanDatabaseBackupFolderPath}//Clanmates";
            var di = new DirectoryInfo(clanmateBackupFolder);   
            if(di.Exists == false)
            {
                di.Create();    
            }
            
            string file = $"{clanmateBackupFolder}//clanmates_backup_{dateTimeOffset.ToUnixTimeSeconds()}.db";
            SaveAs(file);   
        }
    }
}
