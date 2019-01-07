module Algorithm_Logic_Tests

open System.IO
open System.Text
open System.Collections.Generic
open FsCheck
open FsCheck.Xunit
open LZStringNet.IO
open LZStringNet.Algorithms
open System.Net

let doTest input =
    let buffer =
        let buffer = List<int>()
        let encoder =
            {
                new IEncoder with
                    member x.WriteBits(data, numBits) =
                        buffer.Add(data)
                        ()
                    member x.Flush() = ()
            }
        let tmp = Compressor(encoder)
        tmp.Compress(input)
        tmp.MarkEndOfStream()
        buffer
    let decompressed =   
        let decoder =
            let mutable iter = buffer.GetEnumerator()
            {
                new IDecoder with     
                    member x.ReadBits(numBits) =
                        iter.MoveNext() |> ignore
                        iter.Current
            }
        let builder = StringBuilder()
        let tmp = Decompressor(builder);
        tmp.Decompress(decoder)
        builder.ToString()
    input = decompressed

[<Property>]
let ``can process random wiki main page``() =
    let ok, text =
        let url = "https://en.wikipedia.org/wiki/Main_Page"
        let response = 
            let request = WebRequest.Create(url) :?> HttpWebRequest
            request.GetResponse() :?> HttpWebResponse
        if response.StatusCode = HttpStatusCode.OK then 
            use reader =
                let stream = response.GetResponseStream()
                if response.CharacterSet = null then
                    new StreamReader(stream)
                else
                    new StreamReader(stream, Encoding.GetEncoding(response.CharacterSet))
            (true, reader.ReadToEnd())
        else
            (false, "")
    ok ==> doTest(text)

[<Property>]
let ``compressor logics and decompressor is compatible`` (input: string) =
    (input <> null && input <> "") ==> doTest(input)