using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{

    [System.Serializable]
    public enum ChestType
    {
        COMMON = 0,
        RARE = 1,
        EPIC = 2,
        HEROIC = 3,
        BANK = 4,
        ARENA = 5,
        CITADEL = 6,
        UNION_TRIUMPH = 7,
        EVENT = 8,
        MIMIC = 9,
        VAULT = 10,
        EPIC_BOSS = 11,
        ANCIENT_EVENT = 12,
        STORY = 13,
        WEALTH = 14,
        RUNIC = 15,
        JORMUNGANDR = 16,
        CUSTOM = 17,
        OTHER = 18
    }
}
