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

using Octokit;
using Newtonsoft.Json;
using System.Configuration;
using System.Security.Principal;

namespace TBChestTracker
{

    //-- ApplicationManager 
    //-- In charge of passing data throughout entire application.
    //-- Localization, ChestTypes, etc...

    public class ApplicationManager : IDisposable
    {
        private bool disposedValue;

        public GitHubClient client {  get; private set; }   
        public List<GameChest> Chests { get; private set; }

        public static ApplicationManager Instance { get; private set; }
        private CultureInfo Culture => System.Globalization.CultureInfo.CurrentCulture;
        public string Language { get; private set; }
        public string LocalePath { get; private set; }
        public Process ServerProcess { get; private set; }  

        public Release LatestReleaseInfo { get; private set; }

        private Manifest UpdateManifest = null;
       
        public ApplicationManager() 
        { 
            if (Instance == null)
                Instance = this;
        
            this.Chests = new List<GameChest>();
            client = new GitHubClient(new ProductHeaderValue("TBChestTracker"));
            UpdateManifest = new Manifest();
        }
        public bool IsAdministrator()
        {
            var windowsPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        public void SetChests(List<GameChest> chests)
        {
            this.Chests = chests;
        }
        public bool Build()
        {
            //-- builds all necessary data for the application to use.
            //-- loaded from C:\ProgramData\SICGames\TotalBattleChestTracker\locale\ and user's language.

            if (SettingsManager.Instance != null)
            {
                var generalSettings = SettingsManager.Instance.Settings.GeneralSettings;
                if (generalSettings != null)
                {
                    if(generalSettings.UILanguage == null || generalSettings.UILanguage.Equals("English"))
                    {
                        Language = "en-US";
                    }
                }
            }
            var localeFolder = $@"{AppContext.Instance.LocalApplicationPath}locale";
            var di = new DirectoryInfo(localeFolder);

            if(di.Exists == false)
            {
                di.Create();
                var languageFolder = $@"{di.FullName}\{Language}";
                var di2 = new DirectoryInfo(languageFolder);
                if(!di2.Exists)
                {
                    di2.Create();
                    //-- need to acquire old Chests.csv and stuff them in new folder.
                    var oldLocalePath = $@"{AppContext.Instance.CommonAppFolder}locale\{Language}\";
                    var oldChestsPath = $@"{oldLocalePath}Chests.csv";
                    if (System.IO.File.Exists(oldChestsPath))
                    {
                        var newLocalePath = di2.FullName;
                        var newChestsPath = $@"{newLocalePath}\Chests.csv";
                        using (System.IO.FileStream fileStream = File.Open(oldChestsPath, System.IO.FileMode.Open))
                        {
                            using (System.IO.FileStream destinationStream = File.OpenWrite(newChestsPath))
                            {
                                fileStream.CopyTo(destinationStream);
                            }
                        }

                        var oldLocaleDirInfo = new DirectoryInfo($@"{AppContext.Instance.CommonAppFolder}locale");
                        if(oldLocaleDirInfo.Exists)
                        {
                            File.Delete(oldChestsPath);
                            oldLocaleDirInfo.Delete(true);
                        }
                    }
                    
                }
            }

            LocalePath = $@"{AppContext.Instance.LocalApplicationPath}locale\{Language}\";
          
            string ChestsFile = $"{LocalePath}Chests.csv";
          
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };
            
            //--- Chests
            //-- Mandatory this file exists. Binds everything together.
            if (!System.IO.File.Exists(ChestsFile))
            {
                //-- file doesn't exist. Need to throw it. 
                //-- allow user to rebuild it.
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
            var working_Directory = $@"{AppContext.Instance.CommonAppFolder}ClanInsights\";
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

        private bool LoadManifest()
        {
            var manifestFile = $@"{AppContext.Instance.LocalApplicationPath}Manifest.json";
            if(!File.Exists(manifestFile))
            {
                return false;
            }

            using (StreamReader sr = File.OpenText(manifestFile))
            {
                var serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                UpdateManifest = (Manifest)serializer.Deserialize(sr, typeof(Manifest));
                serializer = null;
                sr.Close();
            }
            return true;
        }
        public void SaveManifest()
        {
            var manifestFile = $@"{AppContext.Instance.LocalApplicationPath}Manifest.json";

            using (StreamWriter sw = File.CreateText(manifestFile))
            {
                var serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(sw, UpdateManifest);
                serializer = null;
                sw.Close();
            }
        }
        public async Task<bool> IsUpdateAvailable()
        {
            try
            {
                var releases = await client.Repository.Release.GetAll("SICGames", "TBChestTracker");
                LatestReleaseInfo = releases[0];
                var tagHash = MD5Helper.Create(Manifest.Tag);
                //-- v2.0 Preview 2 - Hotfix1 - v2.0-preview-2-patch1
                var hashMatches = MD5Helper.Verify($"{LatestReleaseInfo.TagName}", tagHash);

                return hashMatches == true ? false : true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OCTOKIT EXCEPTION => {ex.Message}");
            }
            return false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    LatestReleaseInfo = null;
                    client = null;
                    ServerProcess.Dispose();
                    Chests.Clear();
                    Chests = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ApplicationManager()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
