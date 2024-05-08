using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;

namespace TBChestTracker 
{
    [System.Serializable]
    public class RecentDatabase
    {
        
        public List<RecentClanDatabase> RecentClanDatabases = new List<RecentClanDatabase>();
        public bool Load(string file = "recent.db")
        {
            var filePath = $"{GlobalDeclarations.CommonAppFolder}{file}";
            if (File.Exists(filePath) == false)
                return false;

            using (StreamReader sr = File.OpenText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;

                RecentClanDatabases = (List<RecentClanDatabase>)serializer.Deserialize(sr, typeof(List<RecentClanDatabase>));
            }

            return true;
        }
        public bool Save(string file = "recent.db")
        {
            //var saveFilePath = $"Settings.json";
            var savePath = $"{GlobalDeclarations.CommonAppFolder}{file}";
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
    }
}
