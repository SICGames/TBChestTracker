using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.HellStormGames.Diagnostics.Logging
{
    public class LoggioFileListener : ILoggioListener, IDisposable
    {
        readonly StreamWriter StreamWriter;
        readonly string File;
        readonly object syncroot = new object();

        public LoggioFileListener(string file)
        {
            if (String.IsNullOrEmpty(file)) throw new ArgumentNullException(nameof(file));
            var directory = Path.GetDirectoryName(file);
            if (!String.IsNullOrEmpty(directory))
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }

            Stream stream = System.IO.File.Open(file, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            stream.Seek(0, SeekOrigin.End); //-- go to end of file.
            StreamWriter = new StreamWriter(stream, Encoding.UTF8);
        }

        public void Dispose()
        {
            lock (syncroot)
            {
                StreamWriter.Close();
                StreamWriter.Dispose();
            }
        }

        public void Invoke(LoggioEvent loggioEvent)
        {
            if (loggioEvent == null) throw new ArgumentNullException(nameof(loggioEvent));

            //-- write to file.
            lock (syncroot)
            {
                var str = $"[{loggioEvent.DateTimeOffset.ToString(@"yyyy-MM-dd HH:mm:ss")}]" +
                    $"[{loggioEvent.LogType.ToString()}][{loggioEvent.Tag}] {loggioEvent.Message}\n";
                StreamWriter.Write(str, 0, str.Length);

            }
        }
    }
}
