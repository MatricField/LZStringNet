module Compatibility_Tests

open System.IO
open FsCheck.Xunit
open Jint.Native
open System

type LZNew = LZStringNet.LZString

let engine = Jint.Engine()
let source =
    File.ReadAllText """..\..\..\lz-string\libs\lz-string.min.js"""
engine.Execute source |> ignore
let LZString = (engine.GetValue "LZString").AsObject()

let call name (str: string) =
    let func = (LZString.GetProperty name).Value
    let str' = if str=null then JsValue.Null else JsValue str 
    let ret = func.Invoke str'
    if ret.IsString() then
        ret.AsString()
    elif ret.IsNull() then
        null
    else
        InvalidOperationException() |> raise

[<Property>]
let ``can decompress from base64`` (raw: string)=
    let compressed = call "compressToBase64" raw
    let decompressed = LZNew.DecompressFromBase64 compressed
    raw = decompressed

[<Property>]
let ``can compress to base64`` (raw: string)=
    let compressed = LZNew.CompressToBase64 raw
    let decompressed = call "decompressFromBase64" compressed
    raw = decompressed