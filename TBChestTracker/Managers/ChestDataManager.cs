using com.HellStormGames.Diagnostics;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBChestTracker.Managers;

namespace TBChestTracker
{
    public class ChestDataManager : IDisposable
    {
        private List<ChestsDatabase> Database { get; set; }

        readonly string DefaultChestDatabaseFile;

        public ChestDataManager() 
        { 
            Database = new List<ChestsDatabase>();
            DefaultChestDatabaseFile = $"{ClanManager.Instance.CurrentProjectDirectory}\\Chests\\ChestData.csv";
            //$"{ClanManager.Instance?.CurrentProjectDirectory}\\Chests\\ChestData.csv";
        }
        public List<ChestsDatabase> GetDatabase() 
        { 
            return Database; 
        }

        public bool Load(String filename = "")
        {
            if (String.IsNullOrEmpty(filename))
            {
                filename = DefaultChestDatabaseFile;
            }
            if (System.IO.File.Exists(filename) == false)
            {
                return false;
            }
            try
            {
                Database?.Clear();
                using (var reader = new StreamReader(filename))
                {
                    using (var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        Database = csv.GetRecords<ChestsDatabase>().ToList();
                    }
                    reader.Close();
                }
                ValidateChestPoints(ClanManager.Instance.ClanChestSettings, result =>
                {
                    if(result)
                    {
                        Loggio.Info("Chest Points have been fixed.");
                    }
                    else
                    {
                        Loggio.Info("Chest Points not being used or looking pretty good.");
                    }
                });

                return true;
            }
            catch(Exception ex)
            {
                Loggio.Error(ex, "Chest Data Manager", "An issue occurred while attempting to load Chest Data.");
                return false;
            }
        }

        public void Save(string filename = "", List<ChestsDatabase> database = null)
        {
            if (String.IsNullOrEmpty(filename)) { 
                filename = DefaultChestDatabaseFile;
            }
            if(database != null)
            {
                Database = database;
            }

            try
            {
                using (var writer = new StreamWriter(filename))
                {
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(Database);
                    }
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Loggio.Error(ex, "Chest Data Manager", "Couldn't save chest data.");
            }
        }
        public void CreateBackup()
        {
            if (File.Exists(DefaultChestDatabaseFile) == false)
            {
                return;
            }
            try
            {
                var backupfile = DefaultChestDatabaseFile;
                var ext = Path.GetExtension(backupfile);
                backupfile = backupfile.Replace(ext, ".old");
                using (var streamreader = new StreamReader(DefaultChestDatabaseFile))
                {
                    using(var streamwriter  = new StreamWriter(backupfile))
                    {
                        streamwriter.WriteLine(streamreader.ReadToEnd());
                        streamwriter.Close();
                    }
                    streamreader.Close();
                }

            }
            catch (Exception ex)
            {
            }
        }
        public void ValidateChestPoints(ClanChestSettings clanChestSettings, System.Action<bool> fixedResult)
        {
            var usePoints = clanChestSettings.GeneralClanSettings.ChestOptions == ChestOptions.UsePoints;
            var fixedCount = 0;

            if (usePoints)
            {
                if(Database != null &&  Database.Count > 0)
                {
                    foreach(var databaseItem in Database)
                    {
                        var chestpoint = ChestsDatabase.GetChestPointValue(databaseItem.ToChest(), clanChestSettings);
                        if (databaseItem.ChestPoints != chestpoint.Value)
                        {
                            databaseItem.ChestPoints = chestpoint.Value;
                            fixedCount++;
                        }
                    }
                }
                if (fixedCount > 0)
                {
                    fixedResult(true);
                    return;
                }
            }
            fixedResult(false);
        }
        public void Dispose()
        {
            Database?.Clear();
            Database = null;
        }
    }
}
