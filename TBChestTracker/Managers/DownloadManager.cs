using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace TBChestTracker
{

    public static class HttpClientExtensions
    {
        public static async Task DownloadAsync(this HttpClient client, string requestUri, Stream destination, IProgress<double> progress, CancellationToken cancellationToken = default)
        {
            // Get the http headers first to examine the content length
            using (var response = client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead).Result)
            {
                try
                {
                    await Task.Delay(1000);

                    var responseCode = response.EnsureSuccessStatusCode();
                    var contentLength = response.Content.Headers.ContentLength;
                    using (var download = await response.Content.ReadAsStreamAsync())
                    {

                        // Ignore progress reporting when no progress reporter was 
                        // passed or when the content length is unknown

                        if (progress == null || !contentLength.HasValue)
                        {
                            await download.CopyToAsync(destination);
                            return;
                        }

                        // Convert absolute progress (bytes downloaded) into relative progress (0% - 100%)
                        var relativeProgress = new Progress<long>(totalBytes => progress.Report((float)totalBytes / contentLength.Value));
                        // Use extension method to report progress while downloading

                        await download.CopyToAsync(destination, 81920, relativeProgress, cancellationToken).ConfigureAwait(false);

                        progress.Report(1);

                    }

                    response.Dispose();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }
    }

    public static class StreamExtensions
    {
        public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long> progress, CancellationToken cancellationToken = default)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (!source.CanRead)
                throw new ArgumentException("Has to be readable", nameof(source));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (!destination.CanWrite)
                throw new ArgumentException("Has to be writable", nameof(destination));
            if (bufferSize < 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            var buffer = new byte[bufferSize];
            long totalBytesRead = 0;
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                totalBytesRead += bytesRead;
                progress?.Report(totalBytesRead);
            }
        }
    }

   

    public class DownloadManager
    {
        public static async Task UpdateProgress(string message, double progress, Window window)
        {
            if (window is SplashScreen splashScreen)
            {
                await splashScreen.Dispatcher.BeginInvoke(new Action(() =>
                {
                    splashScreen.UpdateStatus(message, progress);
                }));
            }
        }

        public static async Task Download(Window window, string url, string file, IProgress<double> progress, CancellationToken token)
        {
            if (window is SplashScreen splashScreen)
            {

                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(10);
                
                //-- Sometimes new user agent will help get download progress bar rolling.
                List<string> userAgents = new List<string>
                {
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:130.0) Gecko/20100101 Firefox/130.0",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36 Edg/128.0.0.0",
                };
                var random = new Random();
                int randomIndex = random.Next(userAgents.Count);

                // Select a random UA using the randomIndex
                string randomUserAgent = userAgents[randomIndex];
                client.DefaultRequestHeaders.UserAgent.ParseAdd(randomUserAgent);
                

                using (var filestream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None))
                {

                    await client.DownloadAsync(url, filestream, progress, token);
                    filestream.Close();
                    filestream.Dispose();
                };
            }
        }
    }
}
