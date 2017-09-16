module HCup.Router


open Microsoft.AspNetCore.Http
open System.Collections.Generic
open System
open System.Threading.Tasks
open Giraffe.Tasks
open Giraffe.HttpHandlers
open Giraffe.FormatExpressions
open Giraffe.Common

type Route =
    | User = 0
    | Location = 1
    | Visit = 2
    | UserVisits = 3
    | LocationAvg = 4


type IdHandler = int -> HttpFunc -> HttpContext -> HttpFuncResult
type IdHandlers = Dictionary<Route, IdHandler>


let inline tryParseId stringId path (dictIdHandler: IdHandlers) next ctx =
   match Int32.TryParse(stringId) with
   | true, value -> dictIdHandler.[path] value next ctx
   | false, value -> setStatusCode 404 next ctx

let customRoutef (dictIdHandler: IdHandlers) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let remaining = ref PathString.Empty
        match ctx.Request.Path with
        | visitPath when (visitPath.StartsWithSegments(PathString("/visits"), remaining)) ->
             tryParseId (remaining.Value.Value.Substring(1)) Route.Visit dictIdHandler next ctx
        | userPath when (userPath.StartsWithSegments(PathString("/users"), remaining)) -> 
            let pathString = remaining.Value.Value;
            let slashIndex = pathString.IndexOf("/visits", StringComparison.Ordinal)
            if (slashIndex > -1)
            then
                tryParseId (pathString.Substring(1,slashIndex-1)) Route.UserVisits dictIdHandler next ctx
            else 
                tryParseId (pathString.Substring(1)) Route.User dictIdHandler next ctx
        | locationPath when (locationPath.StartsWithSegments(PathString("/locations"), remaining)) ->          
            let pathString = remaining.Value.Value
            let slashIndex = pathString.IndexOf("/avg", StringComparison.Ordinal)
            if (slashIndex > -1)
            then
                tryParseId (pathString.Substring(1,slashIndex-1)) Route.LocationAvg dictIdHandler next ctx
            else 
                tryParseId (pathString.Substring(1)) Route.Location dictIdHandler next ctx
        | _-> Task.FromResult None