module HCup.Router


open Microsoft.AspNetCore.Http
open System.Collections.Generic
open System
open Juraff.Tasks
open Juraff.HttpHandlers
open Juraff.FormatExpressions
open Juraff.Common


type IdHandler = int -> HttpFunc -> HttpContext -> HttpFuncResult
type IdHandlers = Dictionary<string, IdHandler>

let (|IsVisit|_|) (path: PathString) =
   let remaining = ref PathString.Empty
   if (path.StartsWithSegments(PathString("/visits"), remaining))
   then Some(remaining)
   else None

let (|IsUser|_|) (path: PathString) =
   let remaining = ref PathString.Empty
   if (path.StartsWithSegments(PathString("/users"), remaining))
   then Some(remaining)
   else None

let (|IsLocation|_|) (path: PathString) =
   let remaining = ref PathString.Empty
   if (path.StartsWithSegments(PathString("/locations"), remaining))
   then Some(remaining)
   else None


let tryParseId stringId path (dictIdHandler: IdHandlers) next ctx =
   match Int32.TryParse(stringId) with
   | true, value -> dictIdHandler.[path] value next ctx
   | false, value -> setStatusCode 404 next ctx

let customRoutef (dictIdHandler: IdHandlers) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let remaining = ref PathString.Empty
        match ctx.Request.Path with
        | IsVisit path -> tryParseId (path.Value.Value.Substring(1)) "/visits/%i" dictIdHandler next ctx
        | IsUser path -> 
            let pathString = path.Value.Value.Substring(1)
            if (pathString.EndsWith("/visits"))
            then
                let slashIndex = pathString.IndexOf("/") 
                tryParseId (pathString.Substring(0,slashIndex)) "/users/%i/visits" dictIdHandler next ctx
            else 
                tryParseId pathString "/users/%i" dictIdHandler next ctx
        | IsLocation path ->         
            let pathString = path.Value.Value.Substring(1)
            if (pathString.EndsWith("/avg"))
            then
                let slashIndex = pathString.IndexOf("/") 
                tryParseId (pathString.Substring(0,slashIndex)) "/locations/%i/avg" dictIdHandler next ctx
            else 
                tryParseId pathString "/locations/%i" dictIdHandler next ctx
        | _-> shortCircuit