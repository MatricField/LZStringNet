using System.IO;
using System.Collections.Generic;

namespace LZStringNet.IO
{
    public class BitDecoder:
        IDecoder
    {
        private readonly IEnumerable<int> RawData;

        private readonly int BitsInBufferMax;

        private IEnumerator<int> DataIter;

        private int Buffer;

        private int BitsInBuffer;

        public BitDecoder(string input, DataEncoding encoding)
        {
            var rawData = new List<int>(input.Length);
            BitsInBufferMax = encoding.BitsPerChar;
            var bitReversalTable = encoding.BitReversalTable;
            foreach (var c in input)
            {
                rawData.Add(bitReversalTable[encoding.ReverseCodePage[c]]);
            }
            RawData = rawData;
            DataIter = ((IEnumerable<int>)rawData).GetEnumerator();
        }

        public int ReadBits(int numBits)
        {
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
            if(DataIter.MoveNext())
            {
                Buffer = DataIter.Current;
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
