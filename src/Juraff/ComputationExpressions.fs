module Juraff.ComputationExpressions

open System

type ResultBuilder() =
    member x.Bind(v, f) = Result.bind f v
    member x.Return v   = Ok v

let res = ResultBuilder()