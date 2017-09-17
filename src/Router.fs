module HCup.Router


open Microsoft.AspNetCore.Http
open System.Collections.Generic
open System
open Juraff.Tasks
open Juraff.HttpHandlers
open Juraff.Common

type Route =
    | User = 0
    | Location = 1
    | Visit = 2
    | UserVisits = 3
    | LocationAvg = 4
    | NewVisit = 5
    | NewUser = 6
    | NewLocation = 7



type IdHandler = int -> HttpFunc -> HttpContext -> HttpFuncResult
type IdHandlers = Dictionary<Route, IdHandler>
type NewHandler = HttpFunc -> HttpContext -> HttpFuncResult


let inline tryParseId stringId path (dictIdHandler: IdHandlers) next ctx =
   match Int32.TryParse(stringId) with
   | true, value -> dictIdHandler.[path] value next ctx
   | false, value -> setStatusCode 404 next ctx

let customRoutef (dictIdHandler: IdHandlers) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let remaining = ref PathString.Empty
        match ctx.Request.Path with
        | visitPath when (visitPath.StartsWithSegments(PathString("/visits"), remaining)) ->
            let visitId = remaining.Value.Value.Substring(1)
            if visitId.Equals("new")
            then dictIdHandler.[Route.NewVisit] 0 next ctx
            else tryParseId visitId Route.Visit dictIdHandler next ctx
        | userPath when (userPath.StartsWithSegments(PathString("/users"), remaining)) -> 
            let pathString = remaining.Value.Value;
            if pathString.Equals("/new")
            then dictIdHandler.[Route.NewUser] 0 next ctx
            else                 
                let slashIndex = pathString.IndexOf("/visits", StringComparison.Ordinal)
                if (slashIndex > -1)
                then
                    tryParseId (pathString.Substring(1,slashIndex-1)) Route.UserVisits dictIdHandler next ctx
                else 
                    tryParseId (pathString.Substring(1)) Route.User dictIdHandler next ctx
        | locationPath when (locationPath.StartsWithSegments(PathString("/locations"), remaining)) ->          
            let pathString = remaining.Value.Value;
            if pathString.Equals("/new")
            then dictIdHandler.[Route.NewLocation] 0 next ctx
            else  
                let slashIndex = pathString.IndexOf("/avg", StringComparison.Ordinal)
                if (slashIndex > -1)
                then
                    tryParseId (pathString.Substring(1,slashIndex-1)) Route.LocationAvg dictIdHandler next ctx
                else 
                    tryParseId (pathString.Substring(1)) Route.Location dictIdHandler next ctx
        | _-> shortCircuit