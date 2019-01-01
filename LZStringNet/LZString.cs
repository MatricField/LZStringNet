using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

        public static string CompressToBase64(string input)
        {
            if (input == "")
            {
                return null;
            }
            var compressed = CompressImpl(input, Predefined.Base64Encoding);
            switch(compressed.Length % 4)
            {
                case 1:
                    compressed.Append("===");
                    break;
                case 2:
                    compressed.Append("==");
                    break;
                case 3:
                    compressed.Append("=");
                    break;
            }
            return compressed.ToString();
        }

        private static StringBuilder CompressImpl(string inputStream, DataEncoding encoding)
        {
            var output = new StringBuilder("");
            if(null == inputStream)
            {
                return output;
            }
            var dictionary = new Dictionary<string, int>();
            int GetActualDictionaryCount() => dictionary.Count + 3; //0-2 are reserved for markers
            var codePointWidth = 2; // width of code point in bits
            var dictionaryCapacity = 4; // possible number of code points under current width, value = 2 ^ codePointWidth

            var charsToCode = new Dictionary<string, int>();
            void AddToDictionary(string newSegment)
            {
                void DoAdd(string str)
                {
                    dictionary.Add(str, GetActualDictionaryCount());
                    if (GetActualDictionaryCount() == dictionaryCapacity)
                    {
                        codePointWidth++;
                        dictionaryCapacity *= 2;
                    }
                }
                //if (newSegment.Length != 1)
                //{
                //    var c = newSegment[newSegment.Length - 1];
                //    if (!codedChars.Contains(c))
                //    {
                //        WriteChar(c);
                //        DoAdd(c.ToString());
                //    }
                //    DoAdd(newSegment);
                //}
                //else
                //{
                //    DoAdd(newSegment);
                //}
                DoAdd(newSegment);
            }

            var writer = new BitWriter(output, encoding);

            void WriteChar(string str)
            {
                var c = str[0];
                var charCode = Convert.ToInt32(c);
                if (charCode >= byte.MaxValue)
                {
                    writer.WriteBits(Masks.Char16Bit, charsToCode[str]);
                    writer.WriteBits(charCode, 16);
                }
                else
                {
                    writer.WriteBits(Masks.Char8Bit, charsToCode[str]);
                    writer.WriteBits(charCode, 8);
                }
                charsToCode.Remove(str);
                Console.WriteLine($"Char: {Regex.Escape(c.ToString())}");
            }

            void WriteSegment(string segment)
            {
                //if (dictionary.TryGetValue(segment, out var codePoint))
                //{
                //    writer.WriteBits(codePoint, codePointWidth);
                //    Console.WriteLine($"Segment: {Regex.Escape(segment)}");
                //}
                //else if(segment.Length == 1)
                //{
                //    AddToDictionary(segment);
                //}
                //else
                //{
                //    throw new InvalidOperationException();
                //}
                //switch(segment.Length)
                //{
                //    case 0:
                //        return;
                //    case 1:
                //        if(dictionary.TryGetValue(segment, out var code))
                //        {
                //            writer.WriteBits(code, codePointWidth);
                //            Console.WriteLine($"Segment: {Regex.Escape(segment)}");
                //        }
                //        else
                //        {
                //            WriteChar(segment[0]);
                //            AddToDictionary(segment);
                //        }
                //        break;
                //    default:
                //        //foreach(var c in segment)
                //        //{
                //        //    if(!codedChars.Contains(c))
                //        //    {
                //        //        WriteChar(c);
                //        //        AddToDictionary(c.ToString());
                //        //    }
                //        //}
                //        writer.WriteBits(dictionary[segment], codePointWidth);
                //        break;
                //}
                writer.WriteBits(dictionary[segment], codePointWidth);
                Console.WriteLine($"Segment: {Regex.Escape(segment)}");
            }

            //var cstr = inputStream[0].ToString();
            //WriteChar(cstr[0]);
            //AddToDictionary(cstr);
            //var seg = cstr;
            //bool charCodedLastRound = true;
            //foreach (var c in inputStream.Skip(1))
            //{
            //    cstr = c.ToString();
            //    var charCoded = false;
            //    if(!dictionary.ContainsKey(cstr))
            //    {
            //        WriteChar(c);
            //        AddToDictionary(cstr);
            //        charCoded = true;
            //    }
            //    var newSeg = seg + c;
            //    if (dictionary.ContainsKey(newSeg))
            //    {
            //        seg = newSeg;
            //    }
            //    else
            //    {
            //        if(!charCodedLastRound)
            //        {
            //            WriteSegment(seg);
            //        }
            //        AddToDictionary(newSeg);
            //        seg = cstr;
            //    }
            //    charCodedLastRound = charCoded;
            //}
            var cstr = "";
            var seg = "";
            foreach (var c in inputStream)
            {
                cstr = c.ToString();
                if (!dictionary.ContainsKey(cstr))
                {
                    charsToCode[cstr] = codePointWidth;
                    AddToDictionary(cstr);
                }
                var newSeg = seg + c;
                if (dictionary.ContainsKey(newSeg))
                {
                    seg = newSeg;
                }
                else
                {
                    if (charsToCode.ContainsKey(seg))
                    {
                        WriteChar(seg);
                    }
                    else
                    {
                        WriteSegment(seg);
                    }
                    AddToDictionary(newSeg);
                    seg = cstr;
                }
            }
            if (seg != "")
            {
                WriteSegment(seg);
            }
            writer.WriteBits(Masks.EndOfStream, codePointWidth);
            writer.Flush();
            foreach(var kvp in dictionary)
            {
                Console.WriteLine($"{kvp.Key}, {kvp.Value}");
            }
            return output;
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

            void AddToDictionary(string segment)
            {
                reverseDictionary.Add(reverseDictionary.Count, segment);
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
