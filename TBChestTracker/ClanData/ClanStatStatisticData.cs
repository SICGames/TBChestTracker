using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class ClanStatisticData
    {
        public string Clanmate { get; set; }
        public int CommonCryptsTotal { get; set; }
        public int RareCryptsTotal { get; set; }
        public int EpicCryptsTotal { get; set; }
        public int CitadelsTotal { get; set; }
        public int ArenasTotal { get; set; }
        public int UnionTriumphsTotal { get; set; }
        public int VaultAncientsTotal { get; set; }
        public int HeroicsTotal { get; set; }
        public int AncientChestsTotal { get; set; }
        public int JormungandrShopChestsTotal { get; set; }
        public int StoryChestsTotal { get; set; }
        public int BanksTotal { get; set; }
        public int Total { get; set; }
        public int Points { get; set; }
        public ClanStatisticData(string clanmate, int commoncryptstotal, int rarecryptstotal, int epicryptstotal, int citadelstotal,
            int arenastotal, int uniontriumphcheststotal, int vaultancienttotal, int heroicstotal, int ancientchesttotal, int jormungandrshop, int storycheststotal, int bankstotal, int total, int points)
        {
            Clanmate = clanmate;
            CommonCryptsTotal = commoncryptstotal;
            RareCryptsTotal = rarecryptstotal;
            EpicCryptsTotal = epicryptstotal;
            CitadelsTotal = citadelstotal;
            ArenasTotal = arenastotal;
            UnionTriumphsTotal = uniontriumphcheststotal;
            VaultAncientsTotal = vaultancienttotal;
            HeroicsTotal = heroicstotal;
            AncientChestsTotal = ancientchesttotal;
            JormungandrShopChestsTotal = jormungandrshop;
            StoryChestsTotal = storycheststotal;
            BanksTotal = bankstotal;
            Total = total;
            Points = points;
        }
    }
}
