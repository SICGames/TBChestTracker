﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Newtonsoft.Json;

namespace TBChestTracker
{
    [System.Serializable]
    public class RecentlyOpenedListManager
    {
        public ObservableCollection<RecentClanDatabase> RecentClanDatabases { get; set; }
        public static RecentlyOpenedListManager Instance { get; private set; }
        
        private bool LoadRecentList(string file = "recent.db")
        {
            var filePath = $"{AppContext.Instance.CommonAppFolder}{file}";
            if (File.Exists(filePath) == false)
                return false;

            using (StreamReader sr = File.OpenText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;

                RecentClanDatabases = (ObservableCollection<RecentClanDatabase>)serializer.Deserialize(sr, typeof(ObservableCollection<RecentClanDatabase>));
            }

            return true;
        }
        private bool SaveRecentList(string file = "recent.db")
        {
            //var saveFilePath = $"Settings.json";
            var savePath = $"{AppContext.Instance.CommonAppFolder}{file}";
            try
            {
                using (System.IO.StreamWriter sw = System.IO.File.CreateText(savePath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(sw, RecentClanDatabases);
                    sw.Close();
                    sw.Dispose();
                }

            }
            catch (IOException ex)
            {
                if (MessageBox.Show($"You'll need adminstration privileges to save recently oppened databases file. Run Application as Administration.") == MessageBoxResult.OK)
                    return false;
            }

            return true;
        }


        public bool Build()
        {
            if (File.Exists(AppContext.Instance.RecentOpenedClanDatabases))
            {
                 return LoadRecentList();
                /*
                foreach (var recent in _recentDatabase.RecentClanDatabases)
                {
                    RecentClanDatabases.Add(recent);
                }
                return true;
                */

            }
            return false;
        }

        public void Save()
        {
            SaveRecentList();
        }
        public void Delete()
        {
            ClearList();
            var file = AppContext.Instance.RecentOpenedClanDatabases;
            if(File.Exists(file))
            {
                try
                {
                    File.Delete(file);
                }
                catch(IOException ex)
                {

                }
            }
        }

        public void ClearList()
        {
            RecentClanDatabases.Clear();
        }
        public void Release()
        {
            RecentClanDatabases.Clear();
            RecentClanDatabases = null;
        }
        public RecentlyOpenedListManager()
        {
            RecentClanDatabases = new ObservableCollection<RecentClanDatabase>();
            
            if (Instance == null)
            {
                Instance = this;
            }

        }
    }
}