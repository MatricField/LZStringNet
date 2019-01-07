using System;
using System.Text;
using LZStringNet;
namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var str = @"A grapheme is represented by a base character followed by one or more combining characters. For example, the character ä is represented by a Char object whose code point is U+0061 followed by a Char object whose code point is U+0308. This character can also be defined by a single Char object that has a code point of U+00E4. As the following example shows, a culture-sensitive comparison for equality indicates that these two representations are equal, although an ordinary ordinal comparison does not. However, if the two strings are normalized, an ordinal comparison also indicates that they are equal. (For more information on normalizing strings, see the Normalization section.)";
            var c = LZStringOld.LZString.compressToBase64(null);
            LZString.DecompressFromBase64(c);
            //var bytes = Encoding.ASCII.GetBytes(str);
            //var builder = new StringBuilder();
            //var writer = new BitWriter(builder, Predefined.Base64Encoding);
            //foreach(var b in bytes)
            //{
            //    writer.WriteBits(b, 11);
            //}
            //writer.Flush();
            //bytes = new byte[bytes.Length];
            //var reader = new BitReader(builder.ToString(), Predefined.Base64Encoding);
            //for(var i = 0; i < bytes.Length; ++i)
            //{
            //    bytes[i] = (byte)reader.ReadBits(11);
            //}
            //Console.WriteLine(Encoding.ASCII.GetString(bytes));

            //var compressed = LZStringOld.LZString.compressToBase64(str);
            //var compressed = LZString.CompressToBase64(str);
            var compressed = LZStringOld.LZString.compressToBase64(str);
            var decompressed = LZString.DecompressFromBase64(compressed);
        }
    }
}
