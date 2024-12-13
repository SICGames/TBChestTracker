using CefSharp.DevTools.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBChestTracker.Managers;

namespace TBChestTracker
{
    [Serializable]
    public class ClanChestDatabase : IDisposable
    {
        private int? _version;
        public int? Version
        {
            get
            {
                return _version.GetValueOrDefault(3);
            }
            set
            {
                _version = value;
            }
        }

        public Dictionary<string, IList<ClanChestData>> ClanChestData { get; set; }

        public ClanChestDatabase() 
        {
            if (ClanChestData == null)
            {
                ClanChestData = new Dictionary<string, IList<ClanChestData>>();
            }
        }
        public bool RequiresUpdate()
        {
            return _version.GetValueOrDefault() == 3 ? false : true;
        }

        public void NewEntry(string date)
        {
            List<ClanChestData> list = new List<ClanChestData>();
            var clanmembers = ClanManager.Instance.ClanmateManager.Database.Clanmates;
            foreach (var member in clanmembers)
            {
                if (String.IsNullOrEmpty(member.Name) == false)
                {
                    list.Add(new ClanChestData(member.Name, null));
                }
            }

            ClanChestData.Add(date, list);
            list.Clear();          
        }
        public void NewEntry(string date, IList<ClanChestData> data)
        {
            ClanChestData.Add(date, data);
        }

        public void RemoveEntry(string date)
        {
            ClanChestData.Remove(date);
        }

        public void RemoveAllEntries()
        {
            ClanChestData.Clear();
        }

        public void UpdateEntry(string date, IList<ClanChestData> data)
        {
            ClanChestData[date] = data;
        }

        public void ChangeEntry(string oldValue, string newValue)
        {
            ClanChestData.UpdateKey(oldValue, newValue);
        }

        public void Dispose()
        {
            if (ClanChestData != null)
            {
                ClanChestData.Clear();
                ClanChestData = null;
            }
        }

    }
}
