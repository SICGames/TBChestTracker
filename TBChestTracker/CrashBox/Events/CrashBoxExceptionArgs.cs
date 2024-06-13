using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;

namespace TBChestTracker
{
    public class CrashBoxExceptionArgs : EventArgs
    {
        private bool _RestartApplicaiton = true;
        public bool RestartApplication 
        {
            get => _RestartApplicaiton;
            set => _RestartApplicaiton = value;
        } 
        public StackTrace StackTrace { get; private set; }
        public Exception Exception { get; private set; }

        public CrashBoxExceptionArgs(StackTrace stackTrace, Exception exception, bool restartApplication) 
        { 
            StackTrace = stackTrace;
            Exception = exception;
            RestartApplication = restartApplication;
        }
    }
}
