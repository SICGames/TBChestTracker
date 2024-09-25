using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace TBChestTracker
{

    public static class MyZipFileExtensions
    {
        public static void ExtractToDirectory(this ZipArchive source, string destinationDirectoryName, IProgress<ZipProgress> progress)
        {
            ExtractToDirectory(source, destinationDirectoryName, progress, overwrite: false);
        }

        public static void ExtractToDirectory(this ZipArchive source, string destinationDirectoryName, IProgress<ZipProgress> progress, bool overwrite)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (destinationDirectoryName == null)
                throw new ArgumentNullException(nameof(destinationDirectoryName));


            // Rely on Directory.CreateDirectory for validation of destinationDirectoryName.

            // Note that this will give us a good DirectoryInfo even if destinationDirectoryName exists:
            DirectoryInfo di = Directory.CreateDirectory(destinationDirectoryName);
            string destinationDirectoryFullPath = di.FullName;

            int count = 0;
            foreach (ZipArchiveEntry entry in source.Entries)
            {
                count++;
                var entryFullName = entry.FullName;
                if (entryFullName.IndexOf("/") > 0)
                {
                    entryFullName = entryFullName.Substring(entryFullName.IndexOf("/") + 1);
                    if (String.IsNullOrEmpty(entryFullName) == false)
                    {
                        string fileDestinationPath = Path.GetFullPath(Path.Combine(destinationDirectoryFullPath, entryFullName));

                        if (!fileDestinationPath.StartsWith(destinationDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
                            throw new IOException("File is extracting to outside of the folder specified.");

                        var zipProgress = new ZipProgress(source.Entries.Count, count, entry.FullName);
                        progress.Report(zipProgress);

                        if (Path.GetFileName(fileDestinationPath).Length == 0)
                        {
                            // If it is a directory:

                            if (entry.Length != 0)
                                throw new IOException("Directory entry with data.");

                            Directory.CreateDirectory(fileDestinationPath);
                        }
                        else
                        {
                            // If it is a file:
                            // Create containing directory:
                            Directory.CreateDirectory(Path.GetDirectoryName(fileDestinationPath));
                            entry.ExtractToFile(fileDestinationPath, overwrite: overwrite);
                        }
                    }
                }
            }
        }
    }

    public class ArchiveManager
    {
        private static Task ExtractAsync(string archiveFile, string destinationFolder, IProgress<ZipProgress> progress, bool bOverwrite)
        {
            return Task.Run(() =>
            {
                using (var archive = ZipFile.OpenRead(archiveFile))
                {
                    archive.ExtractToDirectory(destinationFolder, progress, bOverwrite);
                    archive.Dispose();
                }
            });
        }
        public static async Task Extract(string archiveFile, string destinationFolder, IProgress<ZipProgress> progress, bool bOverwrite = true)
        {
            await ExtractAsync(archiveFile, destinationFolder, progress, bOverwrite).ConfigureAwait(false);
        }
    }
}
