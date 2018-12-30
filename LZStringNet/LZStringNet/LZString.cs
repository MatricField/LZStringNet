using System;
using System.Collections.Generic;
using System.Text;

namespace LZStringNet
{
    public static class LZString
    {
        public static string DecompressFromBase64(string input) =>
            Decompress(input, Predefined.Base64Encoding);

        private static string Decompress(string input, DataEncoding encoding)
        {
            var decoder = new Decoder(encoding);
            return decoder.Decode(input);
        }
    }
}
