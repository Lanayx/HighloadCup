module HCup.App

open System
open System.IO
open System.IO.Compression
open System.Collections.Generic
open System.Collections.Concurrent
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Newtonsoft.Json
open Giraffe.HttpHandlers
open Giraffe.Middleware
open HCup.Models

// ---------------------------------
// Web app
// ---------------------------------

let locations = new ConcurrentDictionary<int, Location>()
let users = new ConcurrentDictionary<int, User>()

let getLocation id httpContext = 
    match locations.TryGetValue id with
    | true, location -> json location httpContext
    | _ -> setStatusCode 404 httpContext    
    
let getUser id httpContext = 
    match users.TryGetValue id with
    | true, user -> json user httpContext
    | _ -> setStatusCode 404 httpContext  

let webApp = 
    choose [
        GET >=>
            choose [
                routef "/locations/%i" getLocation
                routef "/users/%i" getUser
            ]
        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) (ctx : HttpContext) =
    logger.LogError(EventId(0), ex, "An unhandled exception has occurred while executing the request.")
    ctx |> (clearResponse >=> setStatusCode 500 >=> text ex.Message)

// ---------------------------------
// Config and Main
// ---------------------------------

let configureApp (app : IApplicationBuilder) = 
    app.UseGiraffeErrorHandler errorHandler
    app.UseGiraffe webApp

// let configureServices (services : IServiceCollection) =
//     let sp  = services.BuildServiceProvider()
//     let env = sp.GetService<IHostingEnvironment>()
//     let viewsFolderPath = Path.Combine(env.ContentRootPath, "Views")
//     services.AddRazorEngine viewsFolderPath |> ignore

let configureLogging (loggerFactory : ILoggerFactory) =
    loggerFactory.AddConsole(LogLevel.Information).AddDebug() |> ignore

let loadData folder =
    Directory.EnumerateFiles(folder, "locations_*.json")
        |> Seq.map (File.ReadAllText >> JsonConvert.DeserializeObject<Locations>)
        |> Seq.collect (fun locationsObj -> locationsObj.locations)
        |> Seq.map (fun loc -> locations.TryAdd(loc.id, loc)) 
        |> Seq.toList
        |> ignore
    
    Directory.EnumerateFiles(folder, "users_*.json")
        |> Seq.map (File.ReadAllText >> JsonConvert.DeserializeObject<Users>)
        |> Seq.collect (fun usersObj -> usersObj.users)
        |> Seq.map (fun user -> users.TryAdd(user.id, user)) 
        |> Seq.toList
        |> ignore

    // Directory.EnumerateFiles(folder, "locations_*.json")
    //     |> Seq.map (File.ReadAllText >> JsonConvert.DeserializeObject<Locations>)
    //     |> Seq.collect (fun locationsObj -> locationsObj.locations)
    //     |> Seq.map (fun loc -> locations.TryAdd(loc.id, loc)) 
    //     |> Seq.toList
    //     |> ignore

[<EntryPoint>]
let main argv =
    if Directory.Exists("../data/extract")
    then Directory.Delete("../data/extract",true)
    Directory.CreateDirectory("../data/extract") |> ignore

    ZipFile.ExtractToDirectory("../data/data.zip","../data/extract")
    loadData "../data/extract"

    WebHostBuilder()
        .UseKestrel()
        .Configure(Action<IApplicationBuilder> configureApp)
        //.ConfigureServices(Action<IServiceCollection> configureServices)
        .ConfigureLogging(Action<ILoggerFactory> configureLogging)
        .Build()
        .Run()
    0