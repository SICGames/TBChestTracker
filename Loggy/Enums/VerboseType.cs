using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.HellStormGames.Diagnostics
{

    /// <summary>
    /// MINIMAL outputs message, log type and date<br/>
    /// NORMAL outputs message, log type, date, and origin class.<br/>
    /// FULL outputs message, log type, date, origin class, function name and line number.
    /// </summary>
    public enum VerboseLevel
    {
        MINIMAL = 0,
        NORMAL = 1,
        FULL = 2
    }
}
