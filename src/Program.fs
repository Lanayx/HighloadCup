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

let isValidLocation (location: Location) =
    location.country.Length <=50 
    && location.city.Length <=50

let isValidUser (user: User) =
    user.email.Length <= 100 
    && user.first_name.Length <=50 
    && user.last_name.Length <=50 

let isValidVisit (visit: Visit) =
    visit.mark <= 5uy 
    // && users.ContainsKey(visit.user)
    // && locations.ContainsKey(visit.location)      


let updateLocation (location:Location) (httpContext: HttpContext) = 
    async {
        let! json = httpContext.ReadBodyFromRequest()
        if (json.Contains(": null"))
            then failwith "Null field"
        let value = JsonConvert.DeserializeObject<LocationUpd>(json)
        let updatedLocation  = 
            { location with 
                distance = if value.distance.HasValue |> not then location.distance else value.distance.Value 
                city = if value.city = null then location.city else value.city 
                place = if value.place = null then location.place else value.place 
                country = if value.country = null then location.country else value.country }

        if (isValidLocation updatedLocation |> not)
        then failwith "Invalid data"
        
        return updatedLocation 
    }

let updateUser (user:User) (httpContext: HttpContext) = 
    async {
        let! json = httpContext.ReadBodyFromRequest()
        if (json.Contains(": null"))
            then failwith "Null field"
        let value = JsonConvert.DeserializeObject<UserUpd>(json)
        let updatedUser  = 
            { user with 
                first_name = if value.first_name = null then user.first_name else value.first_name
                last_name = if value.last_name = null then user.last_name else value.last_name 
                birth_date = if value.birth_date.HasValue |> not then user.birth_date else value.birth_date.Value 
                gender = if value.gender.HasValue |> not then user.gender else value.gender.Value 
                email = if value.email = null then user.email else value.email }

        if (isValidUser updatedUser |> not)
        then failwith "Invalid data"
        
        return updatedUser 
    }

let updateVisit (visit:Visit) (httpContext: HttpContext) = 
    async {
        let! json = httpContext.ReadBodyFromRequest()
        if (json.Contains(": null"))
            then failwith "Null field"
        let value = JsonConvert.DeserializeObject<VisitUpd>(json)
        let updatedVisit  = 
            { visit with 
                user = if value.user.HasValue |> not then visit.user else value.user.Value
                location = if value.location.HasValue |> not then visit.location else value.location.Value 
                visited_at = if value.visited_at.HasValue |> not then visit.visited_at else value.visited_at.Value 
                mark = if value.mark.HasValue |> not then visit.mark else value.mark.Value }

        if (isValidVisit updatedVisit |> not)
        then failwith "Invalid data"
        
        return updatedVisit 
    }

let updateEntity (collection: ConcurrentDictionary<int, 'a>) updateFunc id (httpContext: HttpContext) = 
    match collection.TryGetValue id with
    | true, entity -> 
        async {
            let! updatedEntity = updateFunc entity httpContext
            collection.[id] <- updatedEntity
            return! setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| httpContext 
        }
    | _ -> setStatusCode 404 >=> setBodyAsString "Value doesn't exist" <| httpContext

let addLocation (httpContext: HttpContext) = 
    async {
        let! value = httpContext.BindJson<Location>()
        if (isValidLocation value)
        then
            let result = match locations.TryAdd(value.id, value) with
                         | true -> setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| httpContext
                         | _ -> setStatusCode 400 >=> setBodyAsString "Value already exists" <| httpContext 
            return! result
        else
            return! setStatusCode 400 >=> setBodyAsString "Invalidvalue" <| httpContext    
    }

let addVisit (httpContext: HttpContext) = 
    async {
        let! value = httpContext.BindJson<Visit>()
        if (isValidVisit value)
        then
            let result = match visits.TryAdd(value.id, value) with
                         | true -> setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| httpContext
                         | _ -> setStatusCode 400 >=> setBodyAsString "Value already exists" <| httpContext 
            return! result      
        else
            return! setStatusCode 400 >=> setBodyAsString "Invalidvalue" <| httpContext 
    }

let addUser (httpContext: HttpContext) = 
    async {
        let! value = httpContext.BindJson<User>()
        if (isValidUser value)
        then
            let result = match users.TryAdd(value.id, value) with
                         | true -> setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| httpContext
                         | _ -> setStatusCode 400 >=> setBodyAsString "Value already exists" <| httpContext 
            return! result        
        else
            return! setStatusCode 400 >=> setBodyAsString "Invalidvalue" <| httpContext 
    }

type UserVisit = { mark: uint8; visited_at: uint32; place: string }
type UserVisits = { visits: seq<UserVisit> }
[<CLIMutable>]
type QueryVisit = { fromDate: uint32 option; toDate: uint32 option; country: string; toDistance: uint16 option}

let filterByQueryVisit (query: QueryVisit) (visit: Visit) =
    let location = 
        if (String.IsNullOrEmpty(query.country) |> not || query.toDistance.IsSome)
        then Some locations.[visit.location]
        else None
    (query.fromDate.IsNone || visit.visited_at > query.fromDate.Value)
        && (query.toDate.IsNone || visit.visited_at < query.toDate.Value)
        && (String.IsNullOrEmpty(query.country) || location.Value.country = query.country)
        && (query.toDistance.IsNone || location.Value.distance < query.toDistance.Value)


let getUserVisits userId (httpContext: HttpContext) = 
    if (users.Keys.Contains(userId))
    then
        let query = httpContext.BindQueryString<QueryVisit>()
        async {
            let usersVisits = visits  
                              |> Seq.map (fun keyValue -> keyValue.Value )      
                              |> Seq.filter (fun visit -> visit.user = userId)
                              |> Seq.filter (filterByQueryVisit query)
                              |> Seq.map (fun visit -> {
                                                             mark = visit.mark
                                                             visited_at = visit.visited_at
                                                             place = locations.[visit.location].place
                                                         })
                              |> Seq.sortBy (fun v -> v.visited_at)
            return! json { visits = usersVisits } httpContext
        }
    else
        setStatusCode 404 >=> setBodyAsString "Value doesn't exist" <| httpContext

type Average = { avg: float }
[<CLIMutable>]
type QueryAvg = { fromDate: uint32 option; toDate: uint32 option; fromAge: int option; toAge: int option; gender: Sex option}

let convertToDate timestamp =
    (DateTime(1970, 1, 1, 0, 0, 0, 0)).AddSeconds(timestamp)

let diffYears (startDate: DateTime) (endDate: DateTime) =
    (endDate.Year - startDate.Year - 1) + (if ((endDate.Month > startDate.Month) || ((endDate.Month = startDate.Month) && (endDate.Day >= startDate.Day))) then 1 else 0)


let filterByQueryAvg (query: QueryAvg) (visit: Visit) =

    let user = 
        if (query.gender.IsSome || query.fromAge.IsSome || query.toAge.IsSome)
        then Some users.[visit.user]
        else None

    (query.fromDate.IsNone || visit.visited_at > query.fromDate.Value)
        && (query.toDate.IsNone || visit.visited_at < query.toDate.Value)
        && (query.gender.IsNone || user.Value.gender = query.gender.Value)
        && (query.toAge.IsNone || (diffYears ((float) user.Value.birth_date |> convertToDate) DateTime.Now ) <  query.toAge.Value)
        && (query.fromAge.IsNone || (diffYears ((float) user.Value.birth_date |> convertToDate) DateTime.Now ) > query.fromAge.Value)

let getAvgMark locationId (httpContext: HttpContext) = 
    if (locations.Keys.Contains(locationId))
    then
        let query = httpContext.BindQueryString<QueryAvg>()
        async {
            let markArray = visits 
                              |> Seq.map (fun keyValue -> keyValue.Value )      
                              |> Seq.filter (fun visit -> visit.location = locationId)
                              |> Seq.filter (filterByQueryAvg query)
            let avg = match markArray with
                      | seq when Seq.isEmpty seq -> 0.0
                      | seq -> Math.Round(seq |> Seq.averageBy (fun visit -> (float)visit.mark), 5)
            return! json { avg = avg } httpContext
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
                routef "/locations/%i" <| updateEntity locations updateLocation
                routef "/users/%i" <| updateEntity users updateUser
                routef "/visits/%i" <| updateEntity visits updateVisit

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

let configureLogging (loggerFactory : ILoggerFactory) =
    loggerFactory.AddConsole(LogLevel.Error) |> ignore

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
        .ConfigureLogging(Action<ILoggerFactory> configureLogging)
        .Build()
        .Run()
    0