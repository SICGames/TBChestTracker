using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBChestTracker.Managers;

namespace TBChestTracker
{
    public class ChestDataCollection : IChestDataCollection
    {
        readonly IList<ChestsDatabase> _ChestsCollection;
        private Dictionary<String, Dictionary<String,ChestDataItem>> _ChestDataItems = new Dictionary<string, Dictionary<string, ChestDataItem>>();
        public Dictionary<string, Dictionary<String, ChestDataItem>> Items => _ChestDataItems;

        public ChestDataCollection(IList<ChestsDatabase> chestsCollection)
        {
            _ChestsCollection = chestsCollection;
        }

        public void Build()
        {
            if (_ChestsCollection != null)
            {
                var dates = new List<String>();

                var dategroups = _ChestsCollection.GroupBy(x => x.Date);
                foreach (var group in dategroups)
                {
                    var date = group.Key;
                    if (dates.Contains(date) == false)
                    {
                        dates.Add(date);
                    }
                }

                foreach (var date in dates)
                {
                    if (_ChestDataItems.ContainsKey(date) == false)
                    {
                        Dictionary<String, ChestDataItem> chestDataItems = new Dictionary<string, ChestDataItem>();
                        var dateEntry = _ChestsCollection.Select(x => x).Where(d => d.Date.Equals(date)).ToList();

                        foreach (var data in dateEntry)
                        {
                            var clanmate = data.Clanmate;
                            if (chestDataItems.ContainsKey(clanmate) == false)
                            {
                                IList<Chest> chests = new List<Chest>();
                                ChestDataItem chestdataitem = new ChestDataItem();
                                var points = 0;
                                var clanmateData = dateEntry.Select(d => d).Where(c => c.Clanmate.ToLower().Equals(clanmate.ToLower())).ToList();

                                foreach (var cd in clanmateData)
                                {
                                    var chestname = cd.ChestName;
                                    var type = cd.ChestType;
                                    var level = cd.ChestLevel;
                                    points += cd.ChestPoints;

                                    Chest chest = new Chest(chestname, type, String.Empty, level);
                                    chests.Add(chest);

                                    if (ClanManager.Instance.ClanChestSettings.GeneralClanSettings.ChestOptions == ChestOptions.UsePoints)
                                    {
                                        chestdataitem.Points = points;
                                    }
                                    else
                                    {
                                        chestdataitem.Points = 0;
                                    }
                                }
                                chestdataitem.chests = chests;
                                chestDataItems.Add(clanmate, chestdataitem);
                            }
                        }

                        _ChestDataItems.Add(date, chestDataItems);
                    }
                }
            }
        }
    }
}
