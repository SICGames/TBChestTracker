using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker.Arrays
{
    public class DataCollection<T>
    {
        T[] arr = null;
        public DataCollection() 
        { 
            arr = new T[1]; 
        }

        public T this[int index] 
        { 
            get
            {
                return arr[index];
            }
            set
            {
                arr[index] = value;
            }
        }
        public T this[string name]
        {
            get
            {
             return arr[Array.IndexOf(arr, name)];
            }
            set
            {
                arr[Array.IndexOf(arr, value)] = value; 
            }
        }
        public int Count => arr.Length - 1;
        public bool IsReadOnly => arr.IsReadOnly;
    }

}
