using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        public List<GameChest> Chests { get; private set; }

        public static ApplicationManager Instance { get; private set; }
        private CultureInfo Culture => System.Globalization.CultureInfo.CurrentCulture;
        public string Language => Culture.Name;
        public string LocalePath { get; private set; }

        public Process ServerProcess { get; private set; }  

        public ApplicationManager() 
        { 
            
            if (Instance == null)
                Instance = this;
            
            this.ChestTypes = new List<ChestTypes>();
            this.ChestNames = new List<ChestNames>();
            this.Chests = new List<GameChest>();
        }
        ~ApplicationManager()
        {
            this.ChestTypes.Clear();
            this.ChestNames.Clear();
        }

        public void SetChests(List<GameChest> chests)
        {
            this.Chests = chests;
        }
        public bool Build()
        {
            //-- builds all necessary data for the application to use.
            //-- loaded from C:\ProgramData\SICGames\TotalBattleChestTracker\locale\ and user's language.

            LocalePath = $@"{GlobalDeclarations.CommonAppFolder}locale\{Language}\";

            if(!System.IO.Directory.Exists(LocalePath))
            {
                //-- fall back on en-US.
                LocalePath = $@"{GlobalDeclarations.CommonAppFolder}locale\en-US\";
            }

            string ChestsFile = $"{LocalePath}Chests.csv";

            string ChestTypesFile = $"{LocalePath}ChestSources.csv";
            string ChestNamesFile = $"{LocalePath}ChestNames.csv";

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };

            //-- ChestTypes and ChestSources files will be absolute soon.


            //--- Chest Types 
            if (!System.IO.File.Exists(ChestTypesFile))
            {
                //-- file doesn't exist. Need to throw it. 
                throw new Exception($"{ChestTypesFile} doesn't exist.");
            }

            using (var reader = new StreamReader(ChestTypesFile))
            {
                using (var csv = new CsvReader(reader, config))
                {
                    ChestTypes = csv.GetRecords<ChestTypes>().ToList();
                }
                reader.Close();
            }

            //--- Chest Names
            if (!System.IO.File.Exists(ChestNamesFile))
            {
                //-- file doesn't exist. Need to throw it. 
                throw new Exception($"{ChestNamesFile} doesn't exist.");
            }

            using (var reader = new StreamReader(ChestNamesFile))
            {
                using (var csv = new CsvReader(reader, config))
                {
                    ChestNames = csv.GetRecords<ChestNames>().ToList();
                }

                reader.Close();
            }

            //--- Chests
            if (!System.IO.File.Exists(ChestsFile))
            {
                //-- file doesn't exist. Need to throw it. 
                //throw new Exception($"{ChestsFile} doesn't exist.");
            }
            else
            {
                using (var reader = new StreamReader(ChestsFile))
                {
                    using (var csv = new CsvReader(reader, config))
                    {
                        Chests = csv.GetRecords<GameChest>().ToList();
                    }

                    reader.Close();
                }
            }

            return true;
        }

        public bool StartNodeServer()
        {
            var working_Directory = $@"{GlobalDeclarations.CommonAppFolder}ClanInsights\";
            var WindowStyle = ProcessWindowStyle.Normal;
#if !DEBUG
    WindowStyle = ProcessWindowStyle.Hidden;
#endif
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.Verb = "runas";
            psi.WindowStyle = WindowStyle;
            psi.UseShellExecute = true;
            psi.WorkingDirectory = working_Directory;
            psi.LoadUserProfile = true;
            psi.FileName = "node.exe";
            psi.Arguments = "./NodeJS/app.js";
            ServerProcess = new Process();
            ServerProcess.StartInfo = psi;
            try
            {
                ServerProcess.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "NodeJS Execution Failed.");
                return false;
            }

            return true;
        }

        public bool KillNodeServer()
        {
            if (ServerProcess != null)
            {
                try
                {
                    ServerProcess.Kill();
                }
                catch(Exception e)
                {
                    //-- couldn't kill process.
                    //-- Should throw exception.
                    return false;
                }
            }
            return true;
        }
    }
}
