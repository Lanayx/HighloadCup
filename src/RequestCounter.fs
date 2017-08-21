module HCup.RequestCounter

open System
open System.Diagnostics
open System.Threading
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Juraff.HttpHandlers
open Juraff.Tasks

let outstandingRequestCount = ref 0

let private getRequestInfo (ctx : HttpContext) =
    (ctx.Request.Protocol,
     ctx.Request.Method,
     ctx.Request.Path.ToString())
|||> sprintf "%s %s %s"

type RequestCounterMiddleware (next : RequestDelegate,
                               handler : HttpHandler) =

    do if isNull next then raise (ArgumentNullException("next"))

    member __.Invoke (ctx : HttpContext) =
        task {
            let start = DateTime.Now
            let! result = next.Invoke ctx
            Interlocked.Increment(outstandingRequestCount)
            |> (fun reqCount -> 
                                if (reqCount = 18090 || reqCount = 30090)
                                then GC.Collect(1)
                                if (reqCount % 1000 = 0)
                                then
                                    let endTime = DateTime.Now
                                    Console.Write(("Result {0} {1} {2}; Threads {3}; "),
                                        reqCount,
                                        (endTime - start).TotalMilliseconds,
                                        endTime.ToString("HH:mm:ss.ffff"),
                                        Process.GetCurrentProcess().Threads.Count)
                                    Console.WriteLine("Gen0={0} Gen1={1} Gen2={2}",
                                        GC.CollectionCount(0),
                                        GC.CollectionCount(1),
                                        GC.CollectionCount(2)))



            
        } :> Task


type IApplicationBuilder with
    member this.UseRequestCounter (handler : HttpHandler) =
        this.UseMiddleware<RequestCounterMiddleware> handler
        |> ignore