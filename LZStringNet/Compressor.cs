using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace LZStringNet
{
    internal class Compressor
    {
        private const int INITIAL_SEGDICT_COUNT = 3;

        private DataEncoding dataEncoding;

        private string uncompressed;

        private StringBuilder compressed = new StringBuilder();

        private BitWriter bitWriter;

        public string Compressed { get; private set; }

        public Compressor(string input, DataEncoding encoding)
        {
            if(string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("input is empty", nameof(input));
            }
            uncompressed = input;
            dataEncoding = encoding;
            bitWriter = new BitWriter(compressed, encoding);
        }

        public string Compress()
        {
            if(null != Compressed)
            {
                throw new InvalidOperationException();
            }
            foreach (var c in uncompressed)
            {
                CompressNext(c);
            }
            Flush();
            Compressed = compressed.ToString();
            return Compressed;
        }

        private Dictionary<string, int> segmentDict = new Dictionary<string, int>();

        private int SegmentDictCount => segmentDict.Count + INITIAL_SEGDICT_COUNT;

        private int dictCapacity = 2;

        private int codePointWidth = 4;

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
                    bitWriter.WriteBits(Masks.Char8Bit, width);
                    bitWriter.WriteBits(charCode, 8);
                }
                else
                {
                    bitWriter.WriteBits(Masks.Char16Bit, width);
                    bitWriter.WriteBits(charCode, 16);
                }
                newChars.Remove(segment);
                Console.WriteLine($"Char: {Regex.Escape(c.ToString())}");
            }
            else
            {
                bitWriter.WriteBits(segmentDict[segment], codePointWidth);
                Console.WriteLine($"Segment: {Regex.Escape(segment)}");
            }
        }

        private void Flush()
        {
            throw new NotImplementedException();
        }
    }
}
