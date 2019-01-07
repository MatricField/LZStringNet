using System;
using System.Collections.Generic;
using LZStringNet.IO;

namespace LZStringNet.Algorithms
{
    public class Compressor
    {
        private const int INITIAL_SEGDICT_COUNT = 3;

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

        private Dictionary<string, int> segmentDict = new Dictionary<string, int>();

        private int SegmentDictCount => segmentDict.Count + INITIAL_SEGDICT_COUNT;

        private int dictCapacity = 4;

        private int codePointWidth = 2;

        private string segment = "";

        private Dictionary<string, int> newChars = new Dictionary<string, int>();

        private void CompressNext(char c)
        {
            var cstr = c.ToString();
            if(!segmentDict.ContainsKey(cstr))
            {
                newChars.Add(cstr, codePointWidth);
                AddToDictionary(cstr);
            }
            var newSeg = segment + cstr;
            if(segmentDict.ContainsKey(newSeg))
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

        private void AddToDictionary(string newSeg)
        {
            segmentDict.Add(newSeg, SegmentDictCount);
            if (SegmentDictCount == dictCapacity)
            {
                codePointWidth++;
                dictCapacity *= 2;
            }
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
                encoder.WriteBits(segmentDict[segment], codePointWidth);
            }
        }

        public void MarkEndOfStream()
        {
            encoder.WriteBits(Masks.EndOfStream, codePointWidth);
            encoder.Flush();
        }
    }
}
