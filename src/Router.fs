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

let customRoutef (dictIdHandler: IdHandlers) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let remaining = ref PathString.Empty
        match ctx.Request.Path with
        | IsVisit path ->  dictIdHandler.["/visits/%i"] (Int32.Parse(path.Value.Value.Substring(1))) next ctx
        | IsUser path -> 
            let pathString = path.Value.Value.Substring(1)
            if (pathString.EndsWith("/visits"))
            then
                let slashIndex = pathString.IndexOf("/") 
                dictIdHandler.["/users/%i/visits"] (Int32.Parse(pathString.Substring(0,slashIndex))) next ctx
            else dictIdHandler.["/users/%i"] (Int32.Parse(pathString)) next ctx
        | IsLocation path ->         
            let pathString = path.Value.Value.Substring(1)
            if (pathString.EndsWith("/avg"))
            then
                let slashIndex = pathString.IndexOf("/") 
                dictIdHandler.["/locations/%i/avg"] (Int32.Parse(pathString.Substring(0,slashIndex))) next ctx
            else dictIdHandler.["/locations/%i"] (Int32.Parse(pathString)) next ctx
        | _-> shortCircuit