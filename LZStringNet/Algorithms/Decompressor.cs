using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LZStringNet.IO;

namespace LZStringNet.Algorithms
{
    using static Masks;
    public class Decompressor
    {
        private Dictionary<int, string> reverseDictionary =
            new Dictionary<int, string>();

        private int codePointWidth = 2;
        private int dictionaryCapacity = 4;
        private int actualDicionaryCount => reverseDictionary.Count + 3;

        private StringBuilder output;

        public Decompressor(StringBuilder output)
        {
            this.output = output ?? throw new ArgumentNullException();
        }

        public void Decompress(IDecoder input)
        {
            this.input = input ?? throw new ArgumentNullException();
            endOfStreamReached = false;
            w = ReadNextSegment(out _);
            if (!endOfStreamReached)
            {
                output.Append(w);
                AddToDictionary(w);
                for (var entry = ReadNextSegment(out var isCharEntry);
                    !endOfStreamReached;
                    entry = ReadNextSegment(out isCharEntry))
                {
                    if (isCharEntry)
                    {
                        AddToDictionary(entry);
                    }
                    output.Append(entry);
                    AddToDictionary(w + entry[0]);
                    w = entry;
                }
            }
            this.input = null;
        }

        private string w = "";

        private void AddToDictionary(string segment)
        {
            reverseDictionary.Add(actualDicionaryCount, segment);
            if (actualDicionaryCount == dictionaryCapacity)
            {
                codePointWidth++;
                dictionaryCapacity *= 2;
            }
        }

        private bool endOfStreamReached;

        private string ReadNextSegment(out bool isCharEntry)
        {
            var codePoint = input.ReadBits(codePointWidth);
            var ret = "";
            switch (codePoint)
            {
                case Char8Bit:
                    ret = Convert.ToChar(input.ReadBits(8)).ToString();
                    isCharEntry = true;
                    break;
                case Char16Bit:
                    ret = Convert.ToChar(input.ReadBits(16)).ToString();
                    isCharEntry = true;
                    break;
                case EndOfStream:
                    ret = default;
                    isCharEntry = default;
                    endOfStreamReached = true;
                    break;
                default:
                    if (reverseDictionary.TryGetValue(codePoint, out ret))
                    {
                        isCharEntry = false;
                    }
                    else if (codePoint == actualDicionaryCount)
                    {
                        ret = w + w[0];
                        isCharEntry = false;
                    }
                    else
                    {
                        throw new InvalidDataException();
                    }
                    break;
            }
            return ret;
        }

        private IDecoder input;
    }
}
