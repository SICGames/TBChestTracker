using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.HellStormGames
{
    namespace Logging 
    { 
        public class Loggy
        {
            public static void Write(string message, LogType logType)
            {
                Write(message, logType, "log.txt");
            }
            public static void Write(string message, LogType type, string file)
            {
                using(TextWriter tw = File.AppendText(file))
                {
                    DateTime dateTime = DateTime.Now;
                    string datestr = $"{dateTime.ToString(@"MM/dd/yy @ hh:mm:ss")}";
                    string typeStr = $"{type.ToString()}";
                    string text = $"[{typeStr}] [{datestr}] -> {message}";
                    Console.Write(text);
                    tw.WriteLine($"{text}");
                    tw.Close();
                    tw.Dispose();
                }
            }

        }
    }


}