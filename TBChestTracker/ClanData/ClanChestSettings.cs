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
    internal class ClanChestSettings
    {
        private static ChestRequirements pChestRequirements  = null;

        public static ChestRequirements ChestRequirements 
        { 
            get
            {
                if( pChestRequirements == null )
                    pChestRequirements = new ChestRequirements();

                
                return pChestRequirements;
            }
            set
            {

                if (pChestRequirements == null)
                    pChestRequirements = new ChestRequirements();
                pChestRequirements = value;
            }
        }

        public ClanChestSettings() 
        { 
            if(ChestRequirements == null)
                ChestRequirements = new ChestRequirements();
        }
        public static void Clear()
        {
            ChestRequirements.ChestConditions.Clear();
        }
        public static void InitSettings()
        {
            ChestRequirements.useChestConditions = false;
            ChestRequirements.useNoChestConditions = true;
            ChestRequirements.ChestConditions = new System.Collections.ObjectModel.ObservableCollection<ChestConditions>();
        }
        public static bool LoadSettings(string file)
        {
            using (StreamReader sr = File.OpenText(file))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                ChestRequirements = (ChestRequirements)serializer.Deserialize(sr, typeof(ChestRequirements));
                if (ChestRequirements != null)
                {
                    return true;
                }
                else
                    return false;
            }

            return true;
        }
        public static void SaveSettings(string file)
        {
            using (System.IO.StreamWriter sw = System.IO.File.CreateText(file))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(sw, ChestRequirements);
                sw.Close();
                sw.Dispose();
            }
        }
    }
}
