using com.HellStormGames.Diagnostics.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.HellStormGames.Diagnostics
{
    public class LoggioConfiguration
    {
        #region Declaration Fields
        readonly List<ILoggioListener> listeners = new List<ILoggioListener>();
        bool createdLogger = false;

        public LoggioListenerConfiguration WriteTo { get; private set; }

        #endregion
        public LoggioConfiguration()
        {
            try
            {
                WriteTo = new LoggioListenerConfiguration(this, s => listeners.Add(s));
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong");
            }
        }

        public Logger CreateLogger()
        {
            if (createdLogger) throw new Exception("Only One Logger Can Exist.");

            createdLogger = true;

            ILoggioListener loggioListener = null;
            if (listeners.Count > 0)
            {
                loggioListener = new LoggioSafeListener(listeners);
            }
            var _disposeableListeners = listeners.Where(s => s is IDisposable).ToArray();

            void Dispose()
            {
                foreach (var listener in _disposeableListeners)
                {
                    (listener as IDisposable)?.Dispose();
                }
            }
            return new Logger(loggioListener, Dispose);
        }
    }
}
