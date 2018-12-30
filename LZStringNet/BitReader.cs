using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LZStringNet
{
    public class BitReader
    {
        private IEnumerator<int> RawData;

        private int Buffer;

        private int BitsInBuffer;

        private int BitsInBufferMax;

        public BitReader(string input, DataEncoding encoding)
        {
            var rawData = new int[input.Length];
            BitsInBufferMax = encoding.BitsPerChar;
            var lookUpTable = BitReversalTable.Get(encoding.BitsPerChar);
            Parallel.For(0, input.Length, (i) => rawData[i] = lookUpTable[encoding.ReverseCodePage[input[i]]]);
            RawData = ((IEnumerable<int>)rawData).GetEnumerator();
        }

        public int ReadBits(int _numBits)
        {
            var numBits = Convert.ToInt32(_numBits);
            int ret = 0;
            for(int bitsRead = 0; bitsRead != numBits;)
            {
                if(BitsInBuffer == 0 && !FetchBits())
                {
                    throw new EndOfStreamException();
                }
                var needToRead = numBits - bitsRead;
                if(BitsInBuffer <= needToRead)
                {
                    ret |= Buffer << bitsRead;
                    bitsRead += BitsInBuffer;
                    BitsInBuffer -= BitsInBuffer;
                }
                else
                {
                    var mask = 0;
                    for(int i = 0; i < needToRead; ++i)
                    {
                        mask |= 1 << i;
                    }
                    ret |= (Buffer & mask) << bitsRead;
                    BitsInBuffer -= needToRead;
                    bitsRead += needToRead;
                    Buffer >>= needToRead;
                }
            }
            return ret;
        }

        private bool FetchBits()
        {
            if(RawData.MoveNext())
            {
                Buffer = RawData.Current;
                BitsInBuffer = BitsInBufferMax;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
