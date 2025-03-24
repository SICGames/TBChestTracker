using com.HellStormGames.Diagnostics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBChestTracker.Managers;

namespace TBChestTracker
{
    public class ClanSettings
    {
        private OCRProfileManager _OcrProfileManager;
        public OCRProfileManager OcrProfileManager => _OcrProfileManager;
        public ClanSettings() 
        { 
            _OcrProfileManager = new OCRProfileManager();
        }
        public bool Load(string filename = "ClanSettings.json")
        {
            if (ClanManager.Instance != null)
            {
                var clanpath = ClanManager.Instance.CurrentProjectDirectory;
                var clanSettingsPath = $@"{clanpath}\Settings";
                var filePath = $@"{clanSettingsPath}\{filename}";
                if(File.Exists(filePath) == false)
                {
                    Loggio.Warn("Clan Settings", $"{filePath} does not exist.");
                    return false;
                }
                OCRProfileManager tmpOcrProfiles = new OCRProfileManager();
                if (JsonHelper.TryLoad(filePath, out tmpOcrProfiles) == false)
                {
                    Loggio.Warn("Clan Settings", $"{filename} has issues and couldn't load correctly.");
                    return false;
                }

                _OcrProfileManager = tmpOcrProfiles;

                return true;
            }

            return false;
        }

        public bool Save(string filename = "ClanSettings.json")
        {
            var savePath = $"{ClanManager.Instance.CurrentProjectDirectory}\\Settings";
            var di = new DirectoryInfo(savePath);
            if (di.Exists == false)
            {
                di.Create();
            }

            var filepath = $"{savePath}\\{filename}";
            
            try
            {
                using (var sw = new StreamWriter(filepath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(sw, OcrProfileManager);
                    sw.Close();
                    serializer = null;
                }
                return true;
            }
            catch (Exception ex)
            {
                Loggio.Error(ex, "Clan Settings", "Issue occured upon saving ClanSettings.json");
                return false;
            }
        }
    }
}
