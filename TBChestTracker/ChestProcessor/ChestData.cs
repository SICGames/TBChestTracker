using com.HellStormGames.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TBChestTracker
{
    [System.Serializable]
    public class ChestData
    {
        public string Clanmate { get; set; }
        public Chest Chest { get; set; }
        public static ChestData Parse(string text)
        {
            List<string> strings = new List<string>();
            if (text.Contains("\n"))
            {
                strings = text.Split('\n').ToList();
            }
            else if (text.Contains("\r\n"))
            {
                var newText = text.Replace("\r\n", "\n");
                strings = text.Split('\n').ToList();
            }
            int clanmateIndex, chestNameIndex, sourceIndex;
            clanmateIndex = chestNameIndex = sourceIndex = 0;
            for (var index = 0; index < strings.Count; index++)
            {
                if (String.IsNullOrEmpty(strings[index]) == false)
                {
                    if (strings[index].ToLower().Contains("contains") == false) //-- we don't care about rewards anymore.
                    {
                        if (strings[index].ToLower().Contains(TBChestTracker.Resources.Strings.From.ToLower()))
                        {
                            clanmateIndex = index;
                            continue;
                        }
                        else if (strings[index].ToLower().Contains(TBChestTracker.Resources.Strings.Source.ToLower()))
                        {
                            sourceIndex = index;
                            continue;
                        }
                        else
                        {
                            chestNameIndex = index;
                            continue;
                        }
                    }
                }
            }

            var clanmate = strings[clanmateIndex];
            var chestname = strings[chestNameIndex];
            var chestsource = strings[sourceIndex];

            //-- if necessary, we may need to ensure there isn't another From:
            //-- (\bFrom\b.\s)+ 
            Loggio.Warn("Chest Data", $"Clanmate Original => {clanmate}");
            clanmate = Regex.Replace(clanmate, $@"(\b{TBChestTracker.Resources.Strings.From}\b.\s)+", String.Empty, RegexOptions.IgnoreCase);
            Loggio.Warn("Chest Data", $"Clanmate Cleaned Up => {clanmate}");

            chestsource = Regex.Replace(chestsource, $@"{TBChestTracker.Resources.Strings.Source}.\s", String.Empty, RegexOptions.IgnoreCase);
            var chestcontent = $"{chestname}\n{chestsource}";
            Chest chest = null;
            Chest.TryParse(chestcontent, out chest);
            return new ChestData(clanmate, chest);
        }
        public static bool TryParse(string chestcontent, out ChestData chest)
        {
            try
            {
                chest = Parse(chestcontent);
                return true;
            }
            catch (Exception ex)
            {
                chest = null;
                return false;
            }
        }

        public ChestData(string clanmate, Chest chest)
        {
            Clanmate = clanmate;
            Chest = chest;
        }

        public Chest ToChest()
        {
            return new Chest(Chest.Name, Chest.Type, Chest.Source, Chest.Level);
        }
        /*
        public int GetPointValue(ObservableCollection<ChestPoints> ChestPoints)
        {
            int chestpoint = 0;
            var m_chest = Chest;
            var pointvalues = ChestPoints;

            foreach (var pointvalue in pointvalues)
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
            return chestpoint;
        }
        */
    }

}
