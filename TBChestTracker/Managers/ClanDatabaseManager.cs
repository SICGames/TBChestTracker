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
using Windows.Media.Ocr;

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
                    m_ClanChestManager.BuildData();
                    GlobalDeclarations.hasNewClanDatabaseCreated = true;
                    CommandManager.InvalidateRequerySuggested();
                    result(true);
                }
                else
                    result(false);
            }
        }
        public ClanDatabaseManager() { }
    }
}
