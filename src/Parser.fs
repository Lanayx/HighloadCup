module HCup.Parser

open System
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives
open HCup.Models
open HCup.Binder


let toString parseFun (strv: StringValues) = 
    toParseResult parseFun <| strv.Item 0

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