using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;

namespace com.HellStormGames.Diagnostics
{
    public class StackInfo : IDisposable
    {
        public System.Diagnostics.StackTrace StackTrace { get; private set; }
        public StackInfo()
        {
            StackTrace = new System.Diagnostics.StackTrace();
        }

        public void Dispose()
        {
            StackTrace = null;
        }
    }
}