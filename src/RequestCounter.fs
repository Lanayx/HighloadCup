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
                                then GC.Collect(2, GCCollectionMode.Forced, true, true)
                                if (reqCount % 7000 = 0)
                                then
                                    let curProcess = Process.GetCurrentProcess()
                                    Console.WriteLine("Gen0={0} Gen1={1} Gen2={2} Alloc={3} Time={4} Treads={5} Mem={6} ReqCount={7}",
                                        GC.CollectionCount(0),
                                        GC.CollectionCount(1),
                                        GC.CollectionCount(2),
                                        GC.GetTotalMemory(false),                                        
                                        DateTime.Now.ToString("HH:mm:ss.ffff"),
                                        curProcess.Threads.Count,
                                        curProcess.PrivateMemorySize64,
                                        reqCount))
        } :> Task


type IApplicationBuilder with
    member this.UseRequestCounter (handler : HttpHandler) =
        this.UseMiddleware<RequestCounterMiddleware> handler
        |> ignore