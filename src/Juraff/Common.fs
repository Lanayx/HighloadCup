module Juraff.Common

open System
open System.IO
open System.Text
open System.Xml
open System.Xml.Serialization
open Microsoft.Extensions.Primitives
open Newtonsoft.Json

/// ---------------------------
/// Helper functions
/// ---------------------------

let inline isNotNull x = isNull x |> not

let inline strOption (str : string) =
    if String.IsNullOrEmpty str then None else Some str

let readFileAsString (filePath : string) =
    use stream = new FileStream(filePath, FileMode.Open)
    use reader = new StreamReader(stream)
    reader.ReadToEndAsync()

let strSegment (str : string) =
    StringSegment(str)

/// ---------------------------
/// Serializers
/// ---------------------------

let inline serializeJson x = JsonConvert.SerializeObject x

let inline deserializeJson<'T> str = JsonConvert.DeserializeObject<'T> str
