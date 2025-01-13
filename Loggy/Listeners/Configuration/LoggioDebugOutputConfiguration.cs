using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.HellStormGames.Diagnostics.Logging
{
    public static class DebugOutputLoggioConfiguration
    {
        public static LoggioConfiguration DebugOutput(this LoggioListenerConfiguration config)
        {
            //--- causes stackoverflow.
            return CreateDebugOutput(config.Sink);
        }
        static LoggioConfiguration CreateDebugOutput(this Func<ILoggioListener, LoggioConfiguration> addListener)
        {
            ILoggioListener listener = new LoggioDebugOutputListener();

            return addListener(listener);
        }
    }
}
