using com.HellStormGames.Diagnostics;
using com.HellStormGames.Diagnostics.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TBChestTracker.Crashhandling
{
    [System.Serializable]
    public class CrashReport
    {
        
        public CrashException CrashException { get; private set; }
        //public string CrashMessage { get; private set; }
        private Exception exception;
        public string CrashLog { get; private set; }
        public CrashReport(string crashLog)
        {
            CrashException = new CrashException();
            CrashLog = crashLog;
        }

        public void Generate()
        {
            /*
            var stacktrace = CrashException.StackTrace;
            var frames = stacktrace?.GetFrames();
            var issue = frames.Where(frame => frame?.GetFileName() is not null).FirstOrDefault();
            */
            var dateoffset = DateTimeOffset.Now;
            var genericCrashMessage = "Drats! Seems like there was a crash. More details about where and what caused the crash should be below: \n";

            var header = $"[{dateoffset.ToString(@"yyyy-MM-dd HH:mm:ss")}] " +
                        $"[CRASH] => {genericCrashMessage}";

            // CrashMessage = $"{header}\t Exception Information: \n\t\t Type: {CrashException.ExceptionType} \n\t\t Message: {CrashException.Message} \n\t\t Stack Trace: {CrashException.StackTrace} \n";
            using (var sw = new StreamWriter("Crash.dump"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(sw, this);
                sw.Close();
            }
        }
    }
}
