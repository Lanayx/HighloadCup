module HCup.Parser

open System
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives
open HCup.Models

[<Struct>]
type ParseResult<'a> =
    | Success of 'a
    | Empty
    | Error

let inline int32Parse str = 
    let result = ref 0
    if Int32.TryParse(str, result)
    then ParseResult.Success result.Value
    else ParseResult.Error

let inline uint32Parse str = 
    let result = ref 0u
    if UInt32.TryParse(str, result)
    then ParseResult.Success result.Value
    else ParseResult.Error

let inline byteParse str = 
    let result = ref 0uy
    if Byte.TryParse(str, result)
    then ParseResult.Success result.Value
    else ParseResult.Error

let inline genderParse str = 
    let result = ref '0'
    if Char.TryParse(str, result)
    then ParseResult.Success result.Value
    else ParseResult.Error

let queryNullableParse (prevResult: ParseResult<'b>) paramName (parseFun: string -> 'a ParseResult) (httpContext: HttpContext) =
    match prevResult with
    | Error -> Error
    | _ ->
        let outParam = ref StringValues.Empty
        let result = httpContext.Request.Query.TryGetValue(paramName, outParam)
        if result
        then
            parseFun (outParam.Value.Item 0)
        else ParseResult.Empty

let queryStringParse paramName (httpContext: HttpContext) =
        let outParam = ref StringValues.Empty
        let result = httpContext.Request.Query.TryGetValue(paramName, outParam)
        if result
        then
            outParam.Value.Item 0
        else null

let checkParseResult result f =
    match result with
    | Success a -> a |> f
    | _ -> true