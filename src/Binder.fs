module HCup.Binder

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