using System;
using System.Collections.Generic;

namespace LZStringNet.Algorithms
{
    internal class ShiftedDictionary<TKey, TValue>
    {
        private const int INITIAL_SEGDICT_COUNT = 3;

        private Dictionary<TKey, TValue> data = new Dictionary<TKey, TValue>();

        private int dictCapacity = 4;

        public int CodePointWidth { get; private set; } = 2;

        public TValue this[TKey key]
        {
            get => data[key];
            set => throw new NotSupportedException();
        }

        public int Count => data.Count + INITIAL_SEGDICT_COUNT;

        public void Add(TKey key, TValue value)
        {
            data.Add(key, value);
        }

        public void CheckCapacity()
        {
            if (Count == dictCapacity)
            {
                CodePointWidth++;
                dictCapacity *= 2;
            }
        }

        public bool ContainsKey(TKey key)
        {
            return data.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return data.TryGetValue(key, out value);
        }
    }
}
