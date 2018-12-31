module Tests

open System.IO
open NUnit.Framework
open FsCheck.NUnit
open Jint.Native

type LZNew = LZStringNet.LZString

let engine = Jint.Engine()
let source =
    File.ReadAllText """..\..\..\lz-string\libs\lz-string.min.js"""
engine.Execute source |> ignore
let LZString = (engine.GetValue "LZString").AsObject()

let call name (str: string) =
    let func = (LZString.GetProperty name).Value
    let str' = if str=null then JsValue.Null else JsValue str 
    (func.Invoke str').AsString()

[<Property>]
let ``can decompress from base64`` (raw: string)=
    let compressed = call "compressToBase64" raw
    let decompressed = LZNew.DecompressFromBase64 compressed
    Assert.AreEqual(raw, decompressed)