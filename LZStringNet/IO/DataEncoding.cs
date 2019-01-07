using System;
using System.Collections.Generic;

namespace LZStringNet.IO
{
    public sealed class DataEncoding
    {
        public int BitsPerChar { get; }

        public string CodePage { get; }

        public IReadOnlyDictionary<char, int> ReverseCodePage { get; }

        public BitReversalTable BitReversalTable { get; }

        public DataEncoding(string alphabet, int bitsPerChar):
            this(bitsPerChar, alphabet, GetReverseCodePage(alphabet), BitReversalTable.Get(bitsPerChar))
        {

        }

        public DataEncoding(
            int bitsPerChar, 
            string codepage, 
            IReadOnlyDictionary<char, int> reverseCodePage, 
            BitReversalTable bitReversalTable)
        {
            BitsPerChar = bitsPerChar;
            CodePage = codepage;
            ReverseCodePage = reverseCodePage;
            BitReversalTable = bitReversalTable;
        }

        private static Dictionary<string, IReadOnlyDictionary<char, int>> ReverseCodePages { get; }
            = new Dictionary<string, IReadOnlyDictionary<char, int>>();

        public static IReadOnlyDictionary<char, int> GetReverseCodePage(string alphabet)
        {
            if (ReverseCodePages.TryGetValue(alphabet, out var ret))
            {
                return ret;
            }
            else
            {
                var tmp = new Dictionary<char, int>();
                for (int i = 0; i < alphabet.Length; ++i)
                {
                    tmp[alphabet[i]] = Convert.ToUInt16(i);
                }
                ReverseCodePages[alphabet] = tmp;
                return tmp;
            }
        }
    }
}
