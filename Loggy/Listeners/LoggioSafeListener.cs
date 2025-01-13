using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.HellStormGames.Diagnostics.Logging
{
    public class LoggioSafeListener : ILoggioListener
    {
        readonly ILoggioListener[] _loggioListeners;

        public LoggioSafeListener(IEnumerable<ILoggioListener> loggioListener) 
        { 
            _loggioListeners = loggioListener.ToArray();
        }

        public void Invoke(LoggioEvent loggioEvent)
        {
            foreach(var loggioListener in _loggioListeners)
            {
                loggioListener.Invoke(loggioEvent);
            }
        }
    }
}
