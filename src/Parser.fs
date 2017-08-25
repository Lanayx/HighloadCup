module HCup.Parser

open System
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives

[<Struct>]
type ParseResult<'a> =
    | Success of 'a
    | Empty
    | Error

let bind m negativeValue f  =
    match m with
    | true, x -> 
        x |> f
    | x -> 
        negativeValue

let toParseResult parseFun value = 
    bind (parseFun value) ParseResult.Error ParseResult.Success

let toString parseFun (strv: StringValues) = 
    strv.Item 0 |> (toParseResult parseFun)

let queryNullableParse prevResult paramName parseFun (httpContext: HttpContext) =
    match prevResult with
    | Error -> Error
    | _ ->
        bind (httpContext.Request.Query.TryGetValue(paramName)) ParseResult.Empty (toString parseFun)

let queryStringParse paramName (httpContext: HttpContext) =
    match httpContext.Request.Query.TryGetValue(paramName) with
    | true, x -> 
        x.Item 0
    | x -> 
        null

let checkParseResult result f =
    match result with
    | Success a -> a |> f
    | _ -> true