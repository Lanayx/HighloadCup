module HCup.RequestCounter

open System
open System.Threading
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Giraffe.HttpHandlers
open Giraffe.Tasks

let outstandingRequestCount = ref 0

let private getRequestInfo (ctx : HttpContext) =
    (ctx.Request.Protocol,
     ctx.Request.Method,
     ctx.Request.Path.ToString())
|||> sprintf "%s %s %s"

type RequestCounterMiddleware (next : RequestDelegate,
                               handler : HttpHandler,
                               loggerFactory : ILoggerFactory) =

    do if isNull next then raise (ArgumentNullException("next"))

    member __.Invoke (ctx : HttpContext) =
        let logger = loggerFactory.CreateLogger<RequestCounterMiddleware>()
        task {
            let start = DateTime.Now
            let! result = next.Invoke ctx
            Interlocked.Increment(outstandingRequestCount)
            |> (fun reqCount -> if (reqCount % 1000 = 0)
                                then
                                    logger.LogError(("Result {0} {1} {2}"),
                                        reqCount, (DateTime.Now - start).TotalMilliseconds, DateTime.Now.ToString("HH:mm:ss.ffff")))
            
        } :> Task


type IApplicationBuilder with
    member this.UseRequestCounter (handler : HttpHandler) =
        this.UseMiddleware<RequestCounterMiddleware> handler
        |> ignore