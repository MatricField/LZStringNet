using System.Text;

namespace LZStringNet.IO
{
    public class BitEncoder : IEncoder
    {
        private readonly StringBuilder OutputStream;

        private readonly DataEncoding Encoding;

        private int BufferCapacity;

        private int Buffer;

        public BitEncoder(StringBuilder stream, DataEncoding encoding)
        {
            OutputStream = stream;
            Encoding = encoding;
            BufferCapacity = encoding.BitsPerChar;
        }

        public void WriteBits(int data, int numBits)
        {
            for (int bitsWritten = 0; bitsWritten != numBits;)
            {
                var needToWrite = numBits - bitsWritten;
                var bitsInBuffer = Encoding.BitsPerChar - BufferCapacity;
                if (needToWrite < BufferCapacity)
                {
                    Buffer |= data << bitsInBuffer;
                    BufferCapacity -= needToWrite;
                    bitsWritten += needToWrite;
                }
                else
                {
                    var mask = 0;
                    for (int i = 0; i < BufferCapacity; ++i)
                    {
                        mask |= 1 << i;
                    }
                    
                    Buffer |= (data & mask) << bitsInBuffer;
                    data >>= BufferCapacity;
                    bitsWritten += BufferCapacity;
                    WriteBuffer();
                }
            }
        }

        public void Flush()
        {
            if(BufferCapacity != Encoding.BitsPerChar)
            {
                WriteBuffer();
            }
        }

        private void WriteBuffer()
        {
            OutputStream.Append(Encoding.CodePage[Encoding.BitReversalTable[Buffer]]);
            BufferCapacity = Encoding.BitsPerChar;
            Buffer = 0;
        }
    }
}
