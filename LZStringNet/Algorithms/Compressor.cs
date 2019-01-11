using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using LZStringNet.IO;

namespace LZStringNet.Algorithms
{
    public class Compressor
    {
        // TODO: Investigate error in string of form 'G`%cabaa'
        private IEncoder encoder;

        public Compressor(IEncoder encoder)
        {
            this.encoder = encoder ?? throw new ArgumentNullException();
        }

        public void Compress(string uncompressed)
        {
            if(null == uncompressed)
            {
                throw new ArgumentNullException();
            }
            else if ("" != uncompressed)
            {
                foreach (var c in uncompressed)
                {
                    CompressNext(c);
                }
                WriteSegment();
            }
        }

        private ShiftedDictionary<string, int> dictionary = new ShiftedDictionary<string, int>();

        private string segment = "";

        private Dictionary<string, int> newChars = new Dictionary<string, int>();

        private void CompressNext(char c)
        {
            var cstr = c.ToString();
            if(!dictionary.ContainsKey(cstr))
            {
                newChars.Add(cstr, dictionary.CodePointWidth);
                AddToDictionary(cstr);
            }
            var newSeg = segment + cstr;
            if(dictionary.ContainsKey(newSeg))
            {
                segment = newSeg;
            }
            else
            {
                WriteSegment();
                AddToDictionary(newSeg);
                segment = cstr;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddToDictionary(string newSeg)
        {
            dictionary.Add(newSeg, dictionary.Count);
            dictionary.CheckCapacity();
        }

        private void WriteSegment()
        {
            if(newChars.TryGetValue(segment, out var width))
            {
                var c = segment[0];
                var charCode = Convert.ToInt32(c);
                if (charCode < byte.MaxValue)
                {
                    encoder.WriteBits(Masks.Char8Bit, width);
                    encoder.WriteBits(charCode, 8);
                }
                else
                {
                    encoder.WriteBits(Masks.Char16Bit, width);
                    encoder.WriteBits(charCode, 16);
                }
                newChars.Remove(segment);
            }
            else
            {
                encoder.WriteBits(dictionary[segment], dictionary.CodePointWidth);
            }
        }

        public void MarkEndOfStream()
        {
            encoder.WriteBits(Masks.EndOfStream, dictionary.CodePointWidth);
            encoder.Flush();
        }
    }
}
