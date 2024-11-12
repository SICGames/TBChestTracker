using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    public static class DictionaryExtensions
    {
        public static void UpdateKey<TKey, TValue>(this IDictionary<TKey,TValue> dic, TKey fromKey, TKey toKey)
        {
            if(dic.ContainsKey(toKey))
            {
                throw new Exception("Dictionary keys must be unique");
            }

            TValue value = dic[fromKey];
            dic.Remove(fromKey);
            dic[toKey] = value;
        }
    }
}
