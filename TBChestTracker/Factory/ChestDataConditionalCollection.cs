
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TBChestTracker.Managers;

namespace TBChestTracker
{
    public class ChestDataConditionalCollection : IChestDataCollection
    {
        readonly IList<ChestsDatabase> _ChestsCollection = null;
        readonly ChestRequirements chestrequirements = null;
        private Dictionary<String, Dictionary<string, ChestDataItem>> _ChestDataItems = new Dictionary<string, Dictionary<string, ChestDataItem>>();
        public Dictionary<string, Dictionary<string, ChestDataItem>> Items => _ChestDataItems;

        public ChestDataConditionalCollection(IList<ChestsDatabase> chestsDatabases, ChestRequirements ChestRequirements)
        {
            _ChestsCollection = chestsDatabases;
            chestrequirements = ChestRequirements;
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
                                var chestConditions = chestrequirements.ChestConditions;

                                foreach (var cd in clanmateData)
                                {
                                    var chestname = cd.ChestName;
                                    var type = cd.ChestType;
                                    var chest_level = cd.ChestLevel;
                                    points += cd.ChestPoints;

                                    foreach (var condition in chestConditions)
                                    {
                                        if (type.ToLower().Contains(condition.ChestType.ToLower()))
                                        {
                                            if (condition.ChestName.Equals("(Any)") == true)
                                            {
                                                if (condition.level.Equals("(Any)") == false)
                                                {
                                                    var level = int.Parse(condition.level);
                                                    if (level >= chest_level)
                                                    {
                                                        var c = new Chest(chestname, type, String.Empty, level);
                                                        chests.Add(c);
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    var c = new Chest(chestname, type, String.Empty, chest_level);
                                                    chests.Add(c);
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                if (chestname.ToLower().Equals(condition.ChestName.ToLower()))
                                                {
                                                    if (condition.level.Equals("(Any)") == false)
                                                    {
                                                        var level = int.Parse(condition.level);
                                                        if (chest_level >= level)
                                                        {
                                                            var c = new Chest(chestname, type, String.Empty, chest_level);
                                                            chests.Add(c);
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var c = new Chest(chestname, type, String.Empty, chest_level);
                                                        chests.Add(c);
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    chestdataitem.Points = 0;
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
