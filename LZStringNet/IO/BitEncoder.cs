using System.Text;

namespace LZStringNet.IO
{
    public class BitEncoder : IEncoder
    {
        private readonly StringBuilder output;

        private readonly DataEncoding encoding;

        private int bitsInBuffer;

        private int buffer;

        public BitEncoder(StringBuilder stream, DataEncoding encoding)
        {
            output = stream;
            this.encoding = encoding;
            bitsInBuffer = 0;
        }

        public void WriteBits(int data, int numBits)
        {
            //copy lower x bit to buffer and flip buffer
            for(var needToWrite = numBits; needToWrite != 0;)
            {
                var capacity = encoding.BitsPerChar - bitsInBuffer;
                if(capacity == 0)
                {
                    WriteBuffer();
                    capacity = encoding.BitsPerChar;
                }
                if (needToWrite <= capacity)
                {
                    buffer |= (data << bitsInBuffer);
                    bitsInBuffer += needToWrite;
                    needToWrite = 0;
                }
                else
                {
                    var mask = BitReversalTable.GetBitMask(capacity);
                    var portion = (data & mask);
                    var shifted = (portion << bitsInBuffer);
                    buffer |= shifted;
                    data >>= capacity;
                    bitsInBuffer += capacity;
                    needToWrite -= capacity;
                }
            }
        }

        public void Flush()
        {
            if(bitsInBuffer != 0)
            {
                WriteBuffer();
            }
        }

        private void WriteBuffer()
        {
            output.Append(encoding.CodePage[encoding.BitReversalTable[buffer]]);
            bitsInBuffer = 0;
            buffer = 0;
        }
    }
}
