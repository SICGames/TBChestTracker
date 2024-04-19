using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker.ClanData
{
    public class Clan
    {
        private string _name;
        private int _members;
        public string Abbreviations { get; set; }
        public string Name { get { return _name; } set { _name = value; } }
        public int Members { get { return _members; } set { _members = value; } }
        public string FolderPath { get; set; }
    }
}
