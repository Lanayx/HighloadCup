module HCup.RequestCounter

open System
open System.Diagnostics
open System.Threading
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Juraff.HttpHandlers
open Juraff.Tasks
open Counter



let private getRequestInfo (ctx : HttpContext) =
    (ctx.Request.Protocol,
     ctx.Request.Method,
     ctx.Request.Path.ToString())
|||> sprintf "%s %s %s"

type RequestCounterMiddleware (next : RequestDelegate,
                               handler : unit -> unit) =
    do if isNull next then raise (ArgumentNullException("next"))

    member __.Invoke (ctx : HttpContext) =
        task {
            let! result = next.Invoke ctx

            Interlocked.Increment(outstandingRequestCount)
            |> (fun reqCount -> 
                                // if (reqCount = 150154 || reqCount = 190154)
                                // then GC.Collect(1)
                                if (reqCount % 7000 = 0)
                                then
                                    Console.WriteLine("Gen0={0} Gen1={1} Gen2={2} ReqCount: {3} Time {4}",
                                        GC.CollectionCount(0),
                                        GC.CollectionCount(1),
                                        GC.CollectionCount(2),
                                        reqCount,
                                        DateTime.Now.ToString("HH:mm:ss.ffff")))



            
        } :> Task


type IApplicationBuilder with
    member this.UseRequestCounter (handler : unit -> unit) =
        this.UseMiddleware<RequestCounterMiddleware> handler
        |> ignore