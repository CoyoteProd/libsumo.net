using System.Collections.Generic;

namespace LibSumo.Net.Helpers
{ 
    /// <summary>
    /// Mimic Python DefaultDict
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal class DefaultDict<TKey, TValue> : Dictionary<TKey, TValue> where TValue : new()
    {
        public new TValue this[TKey key]
        {
            get
            {
                if (!TryGetValue(key, out TValue val))
                {
                    val = new TValue();
                    Add(key, val);
                }
                return val;
            }
            set { base[key] = value; }
        }
    }
}
