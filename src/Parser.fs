module HCup.Parser

open System
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives

[<Struct>]
type ParseResult<'a> =
    | Success of 'a
    | Empty
    | Error

let bind m f negativeValue =
    match m with
    | true, x -> 
        x |> f
    | x -> 
        negativeValue

let toParseResult parseFun value = 
    bind (parseFun value) ParseResult.Success ParseResult.Error

let toString parseFun (strv: StringValues) = 
    strv.Item 0 |> (toParseResult parseFun)

let queryNullableParse prevResult paramName parseFun (httpContext: HttpContext) =
    match prevResult with
    | Error -> Error
    | _ ->
        bind (httpContext.Request.Query.TryGetValue(paramName)) (toString parseFun) ParseResult.Empty

let queryStringParse paramName (httpContext: HttpContext) =
    match httpContext.Request.Query.TryGetValue(paramName) with
    | true, x -> 
        x.Item 0
    | x -> 
        null