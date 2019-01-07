using System;
using System.IO;
using System.Text;
using System.Runtime.CompilerServices;

using LZStringNet.IO;
using static LZStringNet.Algorithms.Masks;

namespace LZStringNet.Algorithms
{
    public class Decompressor
    {
        private ShiftedDictionary<int, string> reverseDictionary =
            new ShiftedDictionary<int, string>();

        private StringBuilder output;

        public Decompressor(StringBuilder output)
        {
            this.output = output ?? throw new ArgumentNullException();
        }

        private IDecoder input;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddToDictionary(string segment)
        {
            reverseDictionary.Add(reverseDictionary.Count, segment);
        }

        private bool endOfStreamReached;

        private string ReadNextSegment(out bool isCharEntry)
        {
            var codePoint = input.ReadBits(reverseDictionary.CodePointWidth);
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
                    else if (codePoint == reverseDictionary.Count)
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
    }
}
