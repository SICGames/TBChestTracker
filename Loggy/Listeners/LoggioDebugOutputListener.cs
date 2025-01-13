using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.HellStormGames.Diagnostics.Logging
{
    public class LoggioDebugOutputListener : ILoggioListener, IDisposable
    {
        public LoggioDebugOutputListener()
        {

        }
        public void Invoke(LoggioEvent loggioEvent)
        {
            string tag = String.IsNullOrEmpty(loggioEvent.Tag) ? "" : $"[{loggioEvent.Tag}]";
            string exceptionString = String.Empty;
            if (loggioEvent.Exception != null)
            {
                var loggioEx = loggioEvent.Exception;
                StackTrace stackTrace = new StackTrace(loggioEx, true);
                StackFrame frame = stackTrace.GetFrames().Where(f => f.GetFileName() != null).FirstOrDefault(); 

                exceptionString = $"\t Exception Information: \n\t\t Type: {loggioEx.GetType()}\n\t\t Message: {loggioEx.Message}\n\t\t" +
                    $" Method Name: {frame.GetMethod().Name}\n\t\t Line: {frame.GetFileLineNumber()}.\n\t\t Source File: {frame.GetFileName()}\n";
            }

            var str = $"[{loggioEvent.DateTimeOffset.ToString(@"yyyy-MM-dd HH:mm:ss")}] " +
                    $"[{loggioEvent.LogType.ToString()}] {tag} => {loggioEvent.Message}\n {exceptionString}";
            System.Diagnostics.Debug.Write(str);
        }

        public void Dispose()
        {
            
        }

    }
}
