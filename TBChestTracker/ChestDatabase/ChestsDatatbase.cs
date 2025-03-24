using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    [System.Serializable]
    public class ChestsDatabase
    {
        public string Date {  get; set; }
        public string Clanmate { get; set; }
        public string ChestName { get; set; }
        public int ChestLevel { get; set; }
        public string ChestType { get; set; }
        private int? _ChestPoints;

        public int ChestPoints
        {
            get => _ChestPoints.GetValueOrDefault(0);
            set => _ChestPoints = value;
        }
        public Chest ToChest()
        {
            return new Chest(ChestName, ChestType, "", ChestLevel);
        }

        public static int? GetChestPointValue(Chest chest,  ClanChestSettings clanchestsettings)
        {
            int chestpoint = 0;
            var chestpoints = clanchestsettings?.ChestPointsSettings?.ChestPoints;

            if (chestpoints != null)
            {
                var m_chest = chest;
                
                foreach (var pointvalue in chestpoints)
                {
                    var chest_type = m_chest.Type.ToString();
                    var chest_name = m_chest.Name.ToString();
                    var level = m_chest.Level;

                    if (chest_type.ToLower().Contains(pointvalue.ChestType.ToLower()))
                    {
                        if (pointvalue.ChestName.Equals("(Any)"))
                        {
                            if (pointvalue.Level.Equals("(Any)"))
                            {
                                chestpoint = pointvalue.PointValue;
                                break;
                            }
                            else
                            {
                                var chestlevel = Int32.Parse(pointvalue.Level.ToString());
                                if (level == chestlevel)
                                {
                                    chestpoint = pointvalue.PointValue;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (chest_name.ToLower().Equals(pointvalue.ChestName.ToLower()))
                            {
                                if (pointvalue.Level.Equals("(Any)"))
                                {
                                    chestpoint = pointvalue.PointValue;
                                    break;
                                }
                                else
                                {
                                    var chestlevel = Int32.Parse(pointvalue.Level.ToString());
                                    if (level == chestlevel)
                                    {
                                        chestpoint = pointvalue.PointValue;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return chestpoint;
            
        }
    }
}
