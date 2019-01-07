module Algorithm_Logic_Tests

open System
open System.IO
open System.Text
open System.Collections.Generic
open FsCheck
open FsCheck.Xunit
open LZStringNet.IO
open LZStringNet.Algorithms
open System.Net

let ``encoding and decosing is compatible`` input =
    let buffer =
        let buffer = List<int>()
        let encoder =
            {
                new IEncoder with
                    member x.WriteBits(data, _) =
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
                    member x.ReadBits(_) =
                        iter.MoveNext() |> ignore
                        iter.Current
            }
        let builder = StringBuilder()
        let tmp = Decompressor(builder);
        tmp.Decompress(decoder)
        builder.ToString()
    input = decompressed

let tryGetResponse (uri:string) =
    let request = WebRequest.Create(uri) :?> HttpWebRequest
    let response = request.GetResponse() :?> HttpWebResponse
    if (response.StatusCode = HttpStatusCode.OK) then
        Some response
    else
        None

let fetchHtml (response:HttpWebResponse) =
    use reader =
        let stream = response.GetResponseStream()
        if response.CharacterSet = null then
            new StreamReader(stream)
        else
            new StreamReader(stream, Encoding.GetEncoding(response.CharacterSet))
    reader.ReadToEnd()

let [<Literal>] randomWikiUri = "http://en.wikipedia.org/wiki/Special:Random"

let tryGetRandomWikiPage() =
    match tryGetResponse randomWikiUri with
    |None -> None
    |Some response ->
        Some (response.ResponseUri.AbsolutePath, fetchHtml response)

type RandomWikiPageGenerator =
    static member HtmlUriPair() =
        {
            new Arbitrary<string * string>() with
                override x.Generator =
                    gen{
                        return
                            Seq.initInfinite (fun _ -> tryGetRandomWikiPage())
                            |>Seq.pick id
                    }
                override x.Shrinker _ = Seq.empty
        }

[<Property(Arbitrary = [|typeof<RandomWikiPageGenerator>|])>]
let ``can process random wiki page`` (uri: string, text) =
    ``encoding and decosing is compatible``(text)

[<Property>]
let ``compressor logics and decompressor is compatible`` (input: string) =
    let doTest() =
        ``encoding and decosing is compatible`` input
    (input <> null && input <> "") ==> doTest