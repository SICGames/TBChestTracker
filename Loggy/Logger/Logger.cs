using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.HellStormGames.Diagnostics.Logging
{
    public sealed class Logger : ILogger, ILoggioListener, IDisposable
    {
        readonly ILoggioListener listener;
        readonly Action? dispose;
        public Logger(ILoggioListener Listener, Action dispose)
        {
            this.listener = Listener;
            this.dispose = dispose;
        }

        public void Info(string message)
        {
            Write(LoggioEventType.INFO, message);
        }

        public void Info(string tag, string message)
        {
            Write(LoggioEventType.INFO, tag, message);
        }

        public void Info(Exception exception, string tag, string message)
        {
            Write(LoggioEventType.INFO, exception, tag, message);
        }

        public void Warn(string message)
        {
            Write(LoggioEventType.WARNING, message);
        }

        public void Warn(string tag, string message)
        {
            Write(LoggioEventType.WARNING, tag, message);
        }

        public void Warn(Exception exception, string tag, string message)
        {
            Write(LoggioEventType.WARNING, exception, tag, message);
        }

        public void Error(string message)
        {
            Write(LoggioEventType.ERROR, message);
        }

        public void Error(string tag, string message)
        {
            Write(LoggioEventType.ERROR, tag, message);
        }

        public void Error(Exception exception, string tag, string message)
        {
            Write(LoggioEventType.ERROR, exception, tag, message);
        }

        public void Fatal(string message)
        {
            Write(LoggioEventType.FATAL, message);
        }

        public void Fatal(string tag, string message)
        {
            Write(LoggioEventType.FATAL, tag, message);
        }

        public void Fatal(Exception exception, string tag, string message)
        {
            Write(LoggioEventType.FATAL, exception, tag, message);

        }
        
        public void Write(LoggioEventType eventtype, string message)
        {
            var loggioEvent = new LoggioEvent(DateTimeOffset.Now, eventtype, "", message, null);
            Send(loggioEvent);
        }

        public void Write(LoggioEventType eventtype, string tage, string message)
        {
            var loggioEvent = new LoggioEvent(DateTimeOffset.Now, eventtype, tage, message, null);
            Send(loggioEvent);
        }
        
        public void Write(LoggioEventType eventtype, Exception exception, string tag, string message)
        {
            var loggioEvent = new LoggioEvent(DateTimeOffset.Now, eventtype, tag, message, exception);
            Send(loggioEvent);
        }

        void ILoggioListener.Invoke(LoggioEvent loggioEvent)
        {
            Send(loggioEvent);
        }

        public void Send(LoggioEvent loggioevent) 
        {
            listener.Invoke(loggioevent);
        }

        public void Dispose()
        {
            dispose?.Invoke();
        }

    }
}
