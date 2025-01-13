using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.HellStormGames.Diagnostics
{
    public class BenchmarkerEventArgs : EventArgs
    {
        public bool isRunning {  get; private set; }
        public TimeSpan Elapsed { get; private set; }
        public string Message { get; private set; }
        public bool bShowFriendly { get; private set; }

        public BenchmarkerEventArgs(bool isrunning,  TimeSpan elapsed, string message = "", bool bshowfriendly = false)  
        {
            isRunning = isrunning;
            Elapsed = elapsed;
            Message = message;
            bShowFriendly = bshowfriendly;
        }
    }

    public class Benchmarker
    {
        private static Stopwatch _watch = null;
        public static event EventHandler<BenchmarkerEventArgs> onStarted;
        public static event EventHandler<BenchmarkerEventArgs> onStopped;
        public static event EventHandler<BenchmarkerEventArgs> onElapsed;
        
        public static void Start()
        {
           _watch = Stopwatch.StartNew();
            if (onStarted != null)
            {
                onStarted(null, new BenchmarkerEventArgs(true, _watch.Elapsed)); 
            }
        }
        public static void Stop()
        {
            _watch.Stop();
            if (onStopped != null)
            {
                onStopped(null, new BenchmarkerEventArgs(false, _watch.Elapsed));
            }
            if(onElapsed != null)
            {
                string msg = $"{_watch.Elapsed.ToString("mm\\:ss\\.fff")} ms";
                onElapsed(null, new BenchmarkerEventArgs(false, _watch.Elapsed, msg));
            }
        }
    }
}
