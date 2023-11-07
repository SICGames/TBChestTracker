using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class GlobalDeclarations
    {
        public static bool hasNewClanDatabaseCreated {  get; set; }
        public static bool hasClanmatesBeenAdded { get; set; }
        public static bool hasHotkeyBeenPressed { get; set; }
        public static bool isAnyGiftsAvailable { get; set; }
        public static bool isBusyProcessingClanchests { get; set; }
        public static bool canCaptureAgain { get; set; }
    }
}
