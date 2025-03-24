using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    [System.Serializable]
    public class OCRProfileManager : IDisposable
    {
        public string CurrentOcrProfileName { get; set; }
        private Dictionary<string, AOIRect> _OCRProfiles = new Dictionary<string, AOIRect>();
        public Dictionary<string, AOIRect> OCRProfiles => _OCRProfiles;

        public void SetProfiles(Dictionary<string, AOIRect> OCRProfiles)
        {
            _OCRProfiles.Clear();
            _OCRProfiles = OCRProfiles;
        }
        public void SetCurrentOcrProfile(string profileName)
        {
            CurrentOcrProfileName = profileName;
        }
        public void UpdateOcrProfile(string profilename, AOIRect value)
        {
            _OCRProfiles[profilename] = value;  
        }
        public void AddProfile(string profileName, AOIRect roi)
        {
            _OCRProfiles.Add(profileName, roi);
        }
        public void RemoveProfile(string profileName) {
            _OCRProfiles.Remove(profileName);
        }
        public AOIRect GetCurrentOcrProfile()
        {
            return _OCRProfiles[CurrentOcrProfileName];
        }

        public AOIRect GetProfile(string profileName) 
        {
            return _OCRProfiles[profileName]; 
        }

        public void Dispose()
        {
            _OCRProfiles.Clear();
        }
    }
}
