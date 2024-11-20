using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    [Serializable]
    public class ClanChestDatabase : IDisposable
    {
        private int? _version;
        public int Version
        {
            get
            {
                return _version.GetValueOrDefault(0);
            }
            set
            {
                _version = value;
            }
        }

        public Dictionary<string, IList<ClanChestData>> ClanChestData { get; private set; }

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
        public void AddEntry(string date, IList<ClanChestData> data)
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
        public void UpdateEntry(string oldValue, string newValue)
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
