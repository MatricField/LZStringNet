using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LZStringNet
{
    public static class LZString
    {
        public static string DecompressFromBase64(string input)
        {
            switch (input)
            {
                case null:
                    return "";
                case "":
                    return null;
                default:
                    return DecompressImpl(input, Predefined.Base64Encoding).ToString();
            };
        }


        private static StringBuilder DecompressImpl(string inputStream, DataEncoding encoding)
        {
            var bitReader = new BitReader(inputStream, encoding);

            var reverseDictionary = new Dictionary<int, string>()
            {
                { Masks.Char8Bit, null},
                { Masks.Char16Bit, null},
                { Masks.EndOfStream, null}
            };
            var codePointWidth = 2; // width of code point in bits
            var dictionaryCapacity = 4; // possible number of code points under current width, value = 2 ^ codePointWidth

            void AddToDictionary(string value)
            {
                reverseDictionary.Add(reverseDictionary.Count, value);
                if (reverseDictionary.Count == dictionaryCapacity)
                {
                    codePointWidth++;
                    dictionaryCapacity *= 2;
                }
            }

            var w = "";
            var result = new StringBuilder();
            bool ReadNextSegment(out string ret, out bool isCharEntry)
            {
                var codePoint = bitReader.ReadBits(codePointWidth);
                switch (codePoint)
                {
                    case Masks.Char8Bit:
                        ret = Convert.ToChar(bitReader.ReadBits(8)).ToString();
                        isCharEntry = true;
                        return true;
                    case Masks.Char16Bit:
                        ret = Convert.ToChar(bitReader.ReadBits(16)).ToString();
                        isCharEntry = true;
                        return true;
                    case Masks.EndOfStream:
                        ret = default;
                        isCharEntry = default;
                        return false;
                    default:
                        isCharEntry = false;
                        if (reverseDictionary.TryGetValue(codePoint, out ret))
                        {
                            return null != ret ? true : throw new InvalidDataException();
                        }
                        else if (codePoint == reverseDictionary.Count)
                        {
                            ret = w + w[0];
                            return true;
                        }
                        else
                        {
                            throw new InvalidDataException();
                        }
                }
            }

            //Main logic
            if (ReadNextSegment(out w, out var _))
            {
                result.Append(w);
                AddToDictionary(w);

                while (ReadNextSegment(out var entry, out var isCharEntry))
                {
                    if (isCharEntry)
                    {
                        AddToDictionary(entry);
                    }
                    result.Append(entry);
                    AddToDictionary(w + entry[0]);
                    w = entry;
                }
            }
            return result;
        }
    }
}
