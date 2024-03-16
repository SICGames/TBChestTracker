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
        private ClanRequirements pClanRequirements = null;
        private ChestPointsSettings pChestPointsSettings = null;

        public ClanRequirements ClanRequirements
        {
            get
            {
                return pClanRequirements;
            }
            set
            {
                pClanRequirements = value;
            }
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
            if(pChestRequirements == null)
                pChestRequirements = new ChestRequirements();
            if(pClanRequirements == null)
                pClanRequirements = new ClanRequirements();
            if(pChestPointsSettings == null)
                pChestPointsSettings = new ChestPointsSettings();
        }
        public void Clear()
        {
            ChestRequirements.ChestConditions.Clear();
        }
        public void InitSettings()
        {
            ChestRequirements.useChestConditions = false;
            ChestRequirements.useNoChestConditions = true;
            ChestRequirements.ChestConditions = new System.Collections.ObjectModel.ObservableCollection<ChestConditions>();

            ClanRequirements.UseNoClanRequirements = true;
            ClanRequirements.UseSpecifiedClanRequirements = false;
            ClanRequirements.ClanSpecifiedRequirements = new System.Collections.ObjectModel.ObservableCollection<ClanSpecifiedRequirements>();

            ChestPointsSettings.UseChestPoints = false;
            ChestPointsSettings.DontUseChestPoints = true;
            ChestPointsSettings.ChestPoints = new System.Collections.ObjectModel.ObservableCollection<ChestPoints> { };
        }
        public bool LoadSettings(string file)
        {
            using (StreamReader sr = File.OpenText(file))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                var clanChestSettings = new ClanChestSettings();
                clanChestSettings = (ClanChestSettings)serializer.Deserialize(sr, typeof(ClanChestSettings));
                this.pClanRequirements = clanChestSettings.ClanRequirements;
                this.pChestRequirements = clanChestSettings.ChestRequirements;
                this.pChestPointsSettings = clanChestSettings.ChestPointsSettings;

                if (pChestRequirements != null || pClanRequirements != null || pChestPointsSettings != null)
                    return true;
                else
                    return false;
            }
        }
        public void SaveSettings(string file)
        {
            using (System.IO.StreamWriter sw = System.IO.File.CreateText(file))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                //serializer.Serialize(sw, ChestRequirements);
                serializer.Serialize(sw, this);
                sw.Close();
                sw.Dispose();
            }
        }
    }
}
