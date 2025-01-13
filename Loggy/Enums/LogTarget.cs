using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.HellStormGames.Diagnostics
{
    [Flags]
    public enum LogTargets
    {
        None = 0,
        File = 2,
        Console = 4
    }
}
