using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace TBChestTracker
{
    public class ClanChestSettings 
    {
        
        private ChestRequirements pChestRequirements  = null;
       // private ClanRequirements pClanRequirements = null;
        private ChestPointsSettings pChestPointsSettings = null;
        private GeneralClanSettings generalClanSettings = null;

        public int Version { get; set; }
       
        public GeneralClanSettings GeneralClanSettings
        {
            get => generalClanSettings;
            set => generalClanSettings = value;
        }
        public ChestRequirements ChestRequirements 
        { 
            get
            {
                return pChestRequirements;
            }
            set
            {
                pChestRequirements = value;
            }
        }

        public ChestPointsSettings ChestPointsSettings
        {
            get
            {
                return pChestPointsSettings;
            }
            set
            {
                pChestPointsSettings = value;
            }
        }

        public ClanChestSettings() 
        {
            if (generalClanSettings == null)
            {
                generalClanSettings = new GeneralClanSettings();
            }

            if(pChestRequirements == null)
                pChestRequirements = new ChestRequirements();
            
            if(pChestPointsSettings == null)
                pChestPointsSettings = new ChestPointsSettings();
        }
        public void Clear()
        {
            ChestRequirements.ChestConditions.Clear();
        }
        public void InitSettings()
        {
            GeneralClanSettings.ChestOptions = ChestOptions.None;
            
            ChestRequirements.ChestConditions = new System.Collections.ObjectModel.ObservableCollection<ChestConditions>();
            ChestPointsSettings.ChestPoints = new System.Collections.ObjectModel.ObservableCollection<ChestPoints> { };
        }
        public bool LoadSettings(string file)
        {
            var clanChestSettings = new ClanChestSettings();
            if(JsonHelper.TryLoad(file, out clanChestSettings))
            {

                this.pChestRequirements = clanChestSettings.ChestRequirements;
                this.pChestPointsSettings = clanChestSettings.ChestPointsSettings;
                this.generalClanSettings = clanChestSettings.GeneralClanSettings;

                if (pChestRequirements != null || generalClanSettings != null || pChestPointsSettings != null)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }
        public void SaveSettings(string file)
        {
            using (System.IO.StreamWriter sw = System.IO.File.CreateText(file))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(sw, this);
                sw.Close();
                sw.Dispose();
            }
        }
       
    }
}
