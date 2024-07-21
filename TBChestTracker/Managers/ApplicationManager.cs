using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using CsvHelper;
using CsvHelper.Configuration;
using Emgu.CV.Ocl;

namespace TBChestTracker
{

    //-- ApplicationManager 
    //-- In charge of passing data throughout entire application.
    //-- Localization, ChestTypes, etc...

    public class ApplicationManager
    {
        public  List<ChestTypes> ChestTypes {  get; private set; }
        public  List<ChestNames> ChestNames { get; private set; } 
        public static ApplicationManager Instance { get; private set; }
        private CultureInfo Culture => System.Globalization.CultureInfo.CurrentCulture;
        public string Language => Culture.Name;

        public ApplicationManager() 
        { 
            
            if (Instance == null)
                Instance = this;
            
            this.ChestTypes = new List<ChestTypes>();
            this.ChestNames = new List<ChestNames>();
        }
        ~ApplicationManager()
        {
            this.ChestTypes.Clear();
            this.ChestNames.Clear();
        }
        public bool Build()
        {
            //-- builds all necessary data for the application to use.
            //-- loaded from C:\ProgramData\SICGames\TotalBattleChestTracker\locale\ and user's language.

            string localeFolderPath = $@"{GlobalDeclarations.CommonAppFolder}locale\{Language}\";

            if(!System.IO.Directory.Exists(localeFolderPath))
            {
                //-- fall back on en-US.
                localeFolderPath = $@"{GlobalDeclarations.CommonAppFolder}locale\en-US\";
            }

            string ChestTypesFile = $"{localeFolderPath}ChestSources.csv";
            string ChestNamesFile = $"{localeFolderPath}ChestNames.csv";

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };
        
            //--- Chest Types 
            using (var reader = new StreamReader(ChestTypesFile))
            {
                using (var csv = new CsvReader(reader, config))
                {
                    ChestTypes = csv.GetRecords<ChestTypes>().ToList();
                }
                reader.Close();
            }

            //--- Chest Names
            using (var reader = new StreamReader(ChestNamesFile))
            {
                using (var csv = new CsvReader(reader, config))
                {
                    ChestNames = csv.GetRecords<ChestNames>().ToList();
                }

                reader.Close();
            }


            return true;
        }
    }
}
