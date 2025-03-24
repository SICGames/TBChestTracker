using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public class ChestLevel
    {
        public int Value { get; private set; }
        public int Min {  get; private set; }
        public int Max { get; private set; }
        public int[] Indexes { get; private set; }

        public static ChestLevel Parse(string text, bool useLocale = false)
        {
            Regex r = new Regex(@"(\d+)");
            var matches = r.Matches(text);
            var min = 0;
            var max = 0;
            List<int> indexes = new List<int>();

            if (matches?.Count > 0)
            {
                GroupCollection? groups = matches[0]?.Groups;
                Int32.TryParse(groups[0]?.Value, out min);
                Int32.TryParse(groups[1]?.Value, out max);
                for (var i = 0; i < groups.Count; i++)
                {
                    var index = groups[i].Index;
                    indexes.Add(index);
                }
            }

            return new ChestLevel(min, max, indexes.ToArray());
        }
        
        public static bool TryParse(string text, out ChestLevel result)
        {
            try
            {
                result = Parse(text);
                return true;
            }
            catch (Exception ex)
            {
                result = null;
                return false;
            }
        }

        public ChestLevel()
        {
            Value = 0;
            Min = 0;
            Max = 0;
        }
        public ChestLevel(int min, int max, int[] indexes = null)
        {
            Value = min;
            Min = min;
            Max = max;
            Indexes = indexes;
        }

    }
    [Serializable]
    public class Chest
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public int Level { get; set; }
        public static Chest Parse(string chestcontent, bool useLocale = false)
        {
            /*
             Barbarian Chest
             Source: Level 15 Crypt

            foreign language prevent speeding this up.
            Spanish - Cripta de nivel 10  (18 characters) 18 / 2 = 9
            English - Level 10 Crypt (15 Characters)  15 / 2 = 7.5 (8)

            First approach was extract type of crypt and phase out chesttype completely. 
            Source: Cripta de 
            Level of crypt: 10
            */
            string name, source, chesttype;
            name = source = chesttype = String.Empty;

            int level = 0;

            string[] content = null;
            content = chestcontent.Split('\n');

            if (content == null) 
                return null;

            name = content[0];
            source = content[1];
            var modified_Source = source;
            if(modified_Source.ToLower().Contains(TBChestTracker.Resources.Strings.lvl.ToLower())) {

                modified_Source = Regex.Replace(modified_Source, $@"{TBChestTracker.Resources.Strings.lvl}\s", String.Empty, RegexOptions.IgnoreCase);
            }
            else if(modified_Source.ToLower().Contains(TBChestTracker.Resources.Strings.Level.ToLower()))
            {
                //-- Level 10 Crypt
                //--  10 Crypt
                modified_Source = Regex.Replace(modified_Source, $@"{TBChestTracker.Resources.Strings.Level}\s", String.Empty, RegexOptions.IgnoreCase);
            }

            ChestLevel chestLevel;
            ChestLevel.TryParse(modified_Source, out chestLevel);
            if (chestLevel != null)
            {
                level = chestLevel.Value;
                chesttype = Regex.Replace(modified_Source, @"(\d+.)", String.Empty);
                if(chesttype.StartsWith(TBChestTracker.Resources.Strings.OnlyCrypt))
                {
                    chesttype = chesttype.Insert(0, $"{TBChestTracker.Resources.Strings.Common} ");
                }
            }
            else
            {
                chesttype = modified_Source;
                level = 0;
            }
            return new Chest(name, chesttype, source, level);
        }
        public static bool TryParse(string source, out Chest chest)
        {
            try
            {
                chest = Parse(source);
                return true;
            }
            catch(Exception ex)
            {
                chest = null;
                return false;
            }
        }
        public Chest(string name, string type, string source, int level)
        {
            Name = name;
            Type = type;
            Source = source;
            Level = level;
        }
    }
}
