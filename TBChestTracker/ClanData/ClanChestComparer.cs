using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class ClanChestComparer : IEqualityComparer<ClanChestData>
    {
        public bool Equals(ClanChestData x, ClanChestData y)
        {
            if(Object.ReferenceEquals(x, y)) return true;   
            
            if(Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null)) 
                return false;


            return x.Clanmate == y.Clanmate;
        }

        public int GetHashCode(ClanChestData obj)
        {
            int chestsHash = obj.chests == null ? 0 : obj.chests.GetHashCode();
            return obj.Clanmate.GetHashCode();
        }
    }
}
