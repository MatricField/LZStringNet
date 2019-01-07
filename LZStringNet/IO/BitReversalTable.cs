using System;
using System.Collections.Generic;
using System.Text;

namespace LZStringNet.IO
{
    public sealed class BitReversalTable
    {
        private readonly int[] Table;

        private readonly int BitWidth;

        private readonly int BitMask;

        private BitReversalTable(int bitWidth)
        {
            var tableSize = Convert.ToInt32(Math.Pow(2, bitWidth));
            var table = new int[tableSize];
            for (var i = 0; i < tableSize; ++i)
            {
                int reversed = 0;
                int tmp = i;
                for (int j = 0; j < bitWidth; ++j)
                {
                    reversed = (reversed << 1) | (tmp & 1);
                    tmp >>= 1;
                }
                table[i] = reversed;
            }
            var mask = 0;
            for (int i = 0; i < bitWidth; ++i)
            {
                mask |= 1 << i;
            }
            Table = table;
            BitWidth = bitWidth;
            BitMask = mask;
        }

        public int this[int val] => Table[val & BitMask];

        private static Dictionary<int, BitReversalTable> Cache = new Dictionary<int, BitReversalTable>();

        public static BitReversalTable Get(int bitWidth)
        {
            if (Cache.TryGetValue(bitWidth, out var ret))
            {
                return ret;
            }
            else
            {
                ret = new BitReversalTable(bitWidth);
                Cache[bitWidth] = ret;
                return ret;
            }
        }
    }
}
