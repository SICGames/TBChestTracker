using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.HellStormGames.Diagnostics.Logging
{
    public interface ILogger
    {
        void Write(LoggioEventType eventtype, string message);
        void Write(LoggioEventType eventtype, string tage,  string message);
        
        void Write(LoggioEventType eventtype, Exception? exception, string tag,  string message);

        void Info(string message);
        void Info(string tag, string message);
        void Info(Exception? exception, string tag, string message);

        void Warn(string message);
        void Warn(string tag, string message);
        void Warn(Exception? exception, string tag, string message);
        
        void Error(string message);
        void Error(string tag, string message);
        void Error(Exception? exception, string tag, string message);

        void Fatal(string message);
        void Fatal(string tag, string message);
        void Fatal(Exception? exception, string tag, string message);
    }
}
