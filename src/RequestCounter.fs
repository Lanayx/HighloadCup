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
            |> (fun reqCount -> if (reqCount % 1000 = 0)
                                then
                                    Console.WriteLine(("Result {0} {1} {2}; Threads {3}"),
                                        reqCount,
                                        (DateTime.Now - start).TotalMilliseconds,
                                        DateTime.Now.ToString("HH:mm:ss.ffff"),
                                        Process.GetCurrentProcess().Threads.Count))



            
        } :> Task


type IApplicationBuilder with
    member this.UseRequestCounter (handler : HttpHandler) =
        this.UseMiddleware<RequestCounterMiddleware> handler
        |> ignore