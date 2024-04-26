using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    [System.Serializable]
    public class MarkerCollection : ICollection<Marker>
    {
        private List<Marker> markers = new List<Marker>();

        public MarkerCollection() 
        {
            
        }
        public MarkerCollection(int size)
        {
            for(int i = 0; i < size; i++)
            {
                markers.Add(new Marker());
            }

        }

        public Marker this[int index]
        {
            get
            {
                return markers[index];
            }
            set
            {
                markers.Insert(index, value);
            }
        }
        public int Count => ((ICollection<Marker>)markers).Count;

        public bool IsReadOnly => ((ICollection<Marker>)markers).IsReadOnly;

        public void Add(Marker item)
        {
            ((ICollection<Marker>)markers).Add(item);
        }

        public void Clear()
        {
            ((ICollection<Marker>)markers).Clear();
        }

        public bool Contains(Marker item)
        {
            return ((ICollection<Marker>)markers).Contains(item);
        }

        public void CopyTo(Marker[] array, int arrayIndex)
        {
            ((ICollection<Marker>)markers).CopyTo(array, arrayIndex);
        }

        public IEnumerator<Marker> GetEnumerator()
        {
            return ((IEnumerable<Marker>)markers).GetEnumerator();
        }

        public bool Remove(Marker item)
        {
            return ((ICollection<Marker>)markers).Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)markers).GetEnumerator();
        }
    }
}
