using System.Text;
using LZStringNet.IO;
using LZStringNet.Algorithms;

namespace LZStringNet
{
    public static class LZString
    {
        public static string DecompressFromBase64(string input)
        {
            return DoDecompress(input, PredefinedEncodings.Base64Encoding)?.ToString();
        }

        public static string CompressToBase64(string input)
        {
            var builder = DoCompress(input, PredefinedEncodings.Base64Encoding);
            switch (builder?.Length % 4)
            {
                case null:
                    break;
                case 1:
                    builder.Append("===");
                    break;
                case 2:
                    builder.Append("==");
                    break;
                case 3:
                    builder.Append("=");
                    break;
            }
            return builder?.ToString();
        }

        private static StringBuilder DoCompress(string input, DataEncoding encoding)
        {
            if (input == "")
            {
                return null;
            }
            else
            {
                var result = new StringBuilder();
                if (null == input)
                {
                    return result;
                }
                else
                {
                    var encoder = new BitEncoder(result, encoding);
                    var compressor = new Compressor(encoder);
                    compressor.Compress(input);
                    compressor.MarkEndOfStream();
                    return result;
                }
            }
            
        }


        private static StringBuilder DoDecompress(string inputStream, DataEncoding encoding)
        {
            if("" == inputStream)
            {
                return null;
            }
            else
            {
                var result = new StringBuilder();
                if (null == inputStream)
                {
                    return result;
                }
                else
                {
                    var decompressor = new Decompressor(result);
                    var decoder = new BitDecoder(inputStream, encoding);
                    decompressor.Decompress(decoder);
                    return result;
                }
            }
            
        }
    }
}
