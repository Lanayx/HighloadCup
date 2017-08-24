module HCup.Parser

open System
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives

let bind m f =
    match m with
    | true, x -> 
        x |> f
    | x -> 
        System.Nullable()

let toNullable parseFun value = 
    bind (parseFun value) (fun x -> Nullable(x))

let toString parseFun (strv: StringValues) = 
    strv.Item 0 |> (toNullable parseFun)

let queryNullableParse paramName parseFun (httpContext: HttpContext) =
    bind (httpContext.Request.Query.TryGetValue(paramName)) (toString parseFun)

let queryStringParse paramName (httpContext: HttpContext) =
    match httpContext.Request.Query.TryGetValue(paramName) with
    | true, x -> 
        x.Item 0
    | x -> 
        null