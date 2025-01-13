using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.HellStormGames.Diagnostics.Logging
{
    public class LoggioListenerConfiguration 
    {
        readonly LoggioConfiguration _loggioConfiguration;
        readonly Action<ILoggioListener> _loggioListener;
        public LoggioListenerConfiguration(LoggioConfiguration loggioConfiguration, Action<ILoggioListener> loggioListener)
        {
            _loggioConfiguration = loggioConfiguration;
            _loggioListener = loggioListener;
        }
        public LoggioConfiguration Sink(ILoggioListener listener)
        {
            _loggioListener(listener);
            return _loggioConfiguration;
        }
    }
}
