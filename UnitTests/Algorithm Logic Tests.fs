module Algorithm_Logic_Tests

open System
open System.Text
open System.Collections.Generic
open FsCheck
open FsCheck.Xunit
open LZStringNet.IO
open LZStringNet.Algorithms


[<Property>]
let ``compressor logics and decompressor is compatible`` (input: string) =
    let doTest() =
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
    (input <> null && input <> "") ==> doTest