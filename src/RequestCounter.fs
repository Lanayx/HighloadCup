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

type RequestCounterMiddleware (next : RequestDelegate,
                               handler : HttpHandler) =

    member __.Invoke (ctx : HttpContext) =
        task {
            let! result = next.Invoke ctx
            Interlocked.Increment(outstandingRequestCount)
            |> (fun reqCount -> 
                                if (reqCount = 150150 || reqCount = 190150)
                                then GC.Collect(1)
                                if (reqCount &&& 8191 = 0)
                                then
                                    Console.WriteLine("Gen0={0} Gen1={1} Gen2={2} Alloc={3} Time={4} ReqCount={5}",
                                        GC.CollectionCount(0),
                                        GC.CollectionCount(1),
                                        GC.CollectionCount(2),
                                        GC.GetTotalMemory(false),                                        
                                        DateTime.Now.ToString("HH:mm:ss.ffff"),
                                        reqCount))
        } :> Task


type IApplicationBuilder with
    member this.UseRequestCounter (handler : HttpHandler) =
        this.UseMiddleware<RequestCounterMiddleware> handler
        |> ignore