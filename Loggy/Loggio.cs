using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using CsvHelper;
using com.HellStormGames.Diagnostics.Logging;

namespace com.HellStormGames.Diagnostics
{
    /// <summary>
    /// Usages:
    /// // Configure() alone sets everything to default.
    /// var l = new Loggio().Configure().AllowAppending(true).SetVerboseLevel(VerboseLevel.MINIMAL).SetLogFileType(FileType.JSON);
    /// l.Info("Hello World!");
    /// try 
    /// {
    ///     l.Info("Trying to be cool just like you!");
    /// } 
    /// catch(Exception ex) 
    /// {
    ///     // Error Replaces Verbose to FULL when writing out.
    ///     l.Error("Arggg! I died a horrible death!");
    /// }
    /// Can use Configure().SetLogTargets(LogTargets.File | LogTargets.Console) to write both to file and console.
    /// </summary>
    
    public class Loggio 
    {
        static ILogger _logger = null;
        public static ILogger Logger
        {
            get => _logger;
            set => _logger = value; 
        }

        public static void Shutdown()
        {
            (_logger as  IDisposable).Dispose();    
        }

        public static void Write(LoggioEventType eventType, string message)
        {
            _logger.Write(eventType, message);
        }
        public static void Write(LoggioEventType eventType, string tag, string message)
        {
            _logger.Write(eventType, tag, message);
        }
        public static void Write(LoggioEventType eventType, string tag, string message, Exception exception)
        {
            _logger.Write(eventType, exception, tag, message);
        }

        public static void Info(string message)
        {
            _logger.Info(message);  
        }
        public static void Info(string tag, string message)
        {
            _logger.Info(tag, message);
        }
        public static void Info(Exception exception,string tag, string message)
        {
            _logger.Info(exception, tag, message);
        }

        public static void Warn(string message)
        {
            _logger.Warn(message);
        }
        public static void Warn(string tag, string message)
        {
            _logger.Warn(tag, message);
        }
        public static void Warn(Exception exception,  string tag, string message)
        {
            _logger.Warn(exception, tag, message);  
        }

        public static void Error(string message)
        {
            _logger?.Error(message);    
        }
        public static void Error(string tag, string message)
        {
            _logger?.Error(tag, message);
        }
        public static void Error(Exception exception, string tag, string message)
        {
            _logger?.Error(exception, tag, message);
        }

        public static void Fatal(string message)
        {
            _logger?.Fatal(message);
        }
        public static void Fatal(string tag, string message)
        {
            _logger?.Fatal(tag, message);
        }
        public static void Fatal(Exception exception, string tag, string message)
        {
            _logger?.Fatal(exception, tag, message);
        }
    }
}