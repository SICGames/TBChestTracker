using CefSharp.DevTools.IndexedDB;
using com.HellStormGames.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class ClanMigration
    {
        readonly string OldClanPath = String.Empty;
        readonly string NewClanPath = String.Empty;
        readonly CancellationTokenSource CancellationSource = null;
        public ClanMigration(string oldpath, string newpath) 
        {
            OldClanPath = oldpath;
            NewClanPath = newpath;
            CancellationSource = new CancellationTokenSource();
        }

        private async Task MigrateAsync(string sourceDir, string destinationDir, bool recursive, CancellationToken token)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists)
            {

                Loggio.Warn("Clan Migration", $"'{sourceDir}' doesn't exist.");
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            if (Directory.Exists(destinationDir) == false)
            {
                Directory.CreateDirectory(destinationDir);
            }

            var index = 0;
            
            foreach (var file in Directory.EnumerateFiles(sourceDir))
            {
                using (FileStream sourceStream = File.Open(file, FileMode.Open))
                {
                    var destFile = $"{destinationDir}{file.Substring(file.LastIndexOf("\\"))}";
                    var status = $"Copying - {destFile}";
                    var current = index;
                    var total = Directory.EnumerateFiles(sourceDir).Count();
            
                    using (FileStream destinationStream = File.OpenWrite(destFile))
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                    }
                }
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }

                index++;
                await Task.Delay(20);
            }

            index = 0;
            
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    await MigrateAsync(subDir.FullName, newDestinationDir, true, token);
                }
            }
        }

        public async Task<bool> Migrate(SettingsManager settingsmanager = null)
        {
            try
            {
                await MigrateAsync(OldClanPath, NewClanPath, true, CancellationSource.Token);
                if (settingsmanager != null)
                {
                    settingsmanager.Settings.GeneralSettings.ClanRootFolder = NewClanPath;
                    settingsmanager.Save();
                }
                Loggio.Info("Clans Migration", "Clan root has been changed successfully.");
                //-- we need to update Recently Opened list.
                var recentlyopened = new RecentlyOpenedListManager();
                recentlyopened.Build();
                foreach (var recent in recentlyopened.RecentClanDatabases.ToList())
                {
                    recent.FullClanRootFolder = recent.FullClanRootFolder.Replace(OldClanPath,NewClanPath);
                    var recentfile = $@"{recent.FullClanRootFolder}\\clan.cdb";
                    var position = StringHelpers.findNthOccurance(recent.FullClanRootFolder, Convert.ToChar(@"\"), 3);
                    var truncated = StringHelpers.truncate_file_name(recent.FullClanRootFolder, position);
                    recent.ShortClanRootFolder = truncated;
                }

                recentlyopened.Save();
                recentlyopened = null;
                Loggio.Info("Clans Migration", "Recently Opened Clans modified and saved.");
            }
            catch (Exception ex)
            {
                if(ex is DirectoryNotFoundException)
                {
                    //-- Since ClanManager creates [Documents\TotalBattleChestTracker]. This isn't a big deal. 
                    //-- User could be new.
                    return true;
                }

                return false;
            }

            return true;
        }

        public static async Task<bool> Cleanup(string destinationDir)
        {
            try
            {
                var di = new DirectoryInfo(destinationDir);
                if(di.Exists)
                {
                    di.Delete(true);
                    return true;
                }
                
                return false;
            }
            catch(Exception ex)
            {
                Loggio.Error(ex, "Clan Migration", @"Issue removing folders\files.");
                return false;
            }
        }
    }
}
