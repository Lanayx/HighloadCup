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
open Giraffe.HttpContextExtensions
open HCup.Models

// ---------------------------------
// Web app
// ---------------------------------

let locations = new ConcurrentDictionary<int, Location>()
let users = new ConcurrentDictionary<int, User>()
let visits = new ConcurrentDictionary<int, Visit>()
 
let getEntity (collection: ConcurrentDictionary<int, 'a>) id httpContext = 
    match collection.TryGetValue id with
    | true, entity -> json entity httpContext
    | _ -> setStatusCode 404 httpContext

let allowedUpdate (collection: ConcurrentDictionary<int, 'a>) id (httpContext: HttpContext) = 
    async {
        let! value = httpContext.BindJson<'a>()
        collection.[id] <- value
        return! setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| httpContext        
    }

let updateEntity (collection: ConcurrentDictionary<int, 'a>) id (httpContext: HttpContext) = 
    match collection.TryGetValue id with
    | true, entity -> allowedUpdate collection id httpContext
    | _ -> setStatusCode 404 >=> setBodyAsString "Value doesn't exist" <| httpContext

let addLocation (httpContext: HttpContext) = 
    async {
        let! value = httpContext.BindJson<Location>()
        let result = match locations.TryAdd(value.id, value) with
                     | true -> setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| httpContext
                     | _ -> setStatusCode 400 >=> setBodyAsString "Value already exists" <| httpContext 
        return! result        
    }

let addVisit (httpContext: HttpContext) = 
    async {
        let! value = httpContext.BindJson<Visit>()
        let result = match visits.TryAdd(value.id, value) with
                     | true -> setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| httpContext
                     | _ -> setStatusCode 400 >=> setBodyAsString "Value already exists" <| httpContext 
        return! result        
    }

let addUser (httpContext: HttpContext) = 
    async {
        let! value = httpContext.BindJson<User>()
        let result = match users.TryAdd(value.id, value) with
                     | true -> setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| httpContext
                     | _ -> setStatusCode 400 >=> setBodyAsString "Value already exists" <| httpContext 
        return! result        
    }

type UserVisit = { mark: uint8; visited_at: uint32; place: string }
type UserVisits = { visits: UserVisit[] }

let getUserVisits userId (httpContext: HttpContext) = 
    if (users.Keys.Contains(userId))
    then
        async {
            let usersVisits = visits 
                              |> Seq.toArray 
                              |> Array.map (fun keyValue -> keyValue.Value )      
                              |> Array.filter (fun visit -> visit.user = userId)
                              |> Array.map (fun visit -> {
                                                             mark = visit.mark
                                                             visited_at = visit.visited_at
                                                             place = locations.[visit.location].place
                                                         })
            return! json usersVisits httpContext
        }
    else
        setStatusCode 404 >=> setBodyAsString "Value doesn't exist" <| httpContext

type Average = { avg: float }

let getAvgMark locationId (httpContext: HttpContext) = 
    if (locations.Keys.Contains(locationId))
    then
        async {
            let avg = visits 
                              |> Seq.toArray 
                              |> Array.map (fun keyValue -> keyValue.Value )      
                              |> Array.filter (fun visit -> visit.location = locationId)
                              |> Array.averageBy (fun visit -> (float)visit.mark)
            return! json { avg = Math.Round(avg,5) } httpContext
        }
    else
        setStatusCode 404 >=> setBodyAsString "Value doesn't exist" <| httpContext

let webApp = 
    choose [
        GET >=>
            choose [
                routef "/locations/%i" <| getEntity locations
                routef "/users/%i" <| getEntity users
                routef "/visits/%i" <| getEntity visits
                
                routef "/users/%i/visits" getUserVisits
                routef "/locations/%i/avg" getAvgMark
            ]
        POST >=>
            choose [
                routef "/locations/%i" <| updateEntity locations
                routef "/users/%i" <| updateEntity users
                routef "/visits/%i" <| updateEntity visits

                route "/locations/new" >=> addLocation
                route "/users/new" >=> addUser
                route "/visits/new" >=> addVisit
            ]
        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) (ctx : HttpContext) =
    logger.LogError(EventId(0), ex, "An unhandled exception has occurred while executing the request.")
    ctx |> (clearResponse >=> setStatusCode 400 >=> text ex.Message)

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

    Directory.EnumerateFiles(folder, "visits_*.json")
        |> Seq.map (File.ReadAllText >> JsonConvert.DeserializeObject<Visits>)
        |> Seq.collect (fun visitObj -> visitObj.visits)
        |> Seq.map (fun visit -> visits.TryAdd(visit.id, visit)) 
        |> Seq.toList
        |> ignore

[<EntryPoint>]
let main argv =
    if Directory.Exists("./data")
    then Directory.Delete("./data",true)
    Directory.CreateDirectory("./data") |> ignore
    if File.Exists("/tmp/data/data.zip")
    then ZipFile.ExtractToDirectory("/tmp/data/data.zip","./data")
    else ZipFile.ExtractToDirectory("data.zip","./data")
    loadData "./data"

    WebHostBuilder()
        .UseKestrel()
        .Configure(Action<IApplicationBuilder> configureApp)
        //.ConfigureServices(Action<IServiceCollection> configureServices)
        .ConfigureLogging(Action<ILoggerFactory> configureLogging)
        .Build()
        .Run()
    0