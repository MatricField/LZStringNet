using System;
using System.Collections.Generic;
using System.Text;

namespace LZStringNet
{
    public class BitWriter
    {
        private readonly StringBuilder OutputStream;

        private readonly DataEncoding Encoding;

        private int BitsInBuffer;

        private int Buffer;

        public BitWriter(StringBuilder stream, DataEncoding encoding)
        {
            OutputStream = stream;
            Encoding = encoding;
            BitsInBuffer = encoding.BitsPerChar;
        }

        public void WriteBits(int data, int count)
        {
            throw new NotImplementedException();
        }

        public void Flush() => WriteBuffer();

        private void WriteBuffer()
        {
            OutputStream.Append(Encoding.CodePage[Encoding.BitReversalTable[Buffer]]);
            BitsInBuffer = Encoding.BitsPerChar;
            Buffer = 0;
        }
    }
}
