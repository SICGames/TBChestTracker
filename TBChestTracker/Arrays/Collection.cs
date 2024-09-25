using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker.Arrays
{
    public class ClanmateCollection : IDisposable
    {
        private List<Clanmate> _collection;
        public ClanmateCollection()
        {
            _collection = new List<Clanmate>();
        }
        public void Add(Clanmate clanmate)
        {
            _collection.Add(clanmate);
        }
        public void AddRange(List<Clanmate> list)
        {
            _collection.AddRange(list);
        }

        public void Remove(Clanmate clanmate)
        {
            _collection.Remove(clanmate);
        }
        public void Clear()
        {
            _collection?.Clear();
        }
        public void RemoveAt(int index)
        {
            _collection.RemoveAt(index);
        }

        public void Dispose()
        {
            _collection.Clear();
            _collection = null;
        }

        public Clanmate this[int index]
        {
            get
            {
                return _collection[index];
            }
            set
            {
                _collection[index] = value;
            }
        }

        public Clanmate this[string name]
        {
            get
            {
                return _collection.Select(c => c).Where(cn => cn.Name.ToLower().Equals(name.ToLower())).FirstOrDefault();  
            }
            set
            {
                var _clanmate = _collection.Select(c=>c).Where(cn => cn.Name.Equals(name.ToLower())).FirstOrDefault();  
                _collection[_collection.IndexOf(_clanmate)] = value; 
            }
        }
    }
}
