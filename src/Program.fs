module HCup.App

open System
open System.IO
open System.IO.Compression
open System.Collections.Generic
open System.Collections.Concurrent
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Newtonsoft.Json
open Giraffe.Tasks
open Giraffe.HttpHandlers
open Giraffe.Middleware
open Giraffe.HttpContextExtensions
open HCup.Models
open HCup.RequestCounter

// ---------------------------------
// Web app
// ---------------------------------

let locations = new ConcurrentDictionary<int, Location>()
let users = new ConcurrentDictionary<int, User>()
let visits = new ConcurrentDictionary<int, Visit>()

type SerializedCollection = ConcurrentDictionary<int, string>
let locationsSerialized = new SerializedCollection()
let usersSerialized = new SerializedCollection()
let visitsSerialized = new SerializedCollection()

type VisitsCollection = ConcurrentDictionary<int, int>
let visitLocations = new ConcurrentDictionary<int, VisitsCollection>()
let visitUsers = new ConcurrentDictionary<int, VisitsCollection>()

type UpdateEntity<'a> = 'a -> HttpContext -> Task<'a>
 
let getEntity (serializedCollection: SerializedCollection) id next = 
    match serializedCollection.TryGetValue id with
    | true, serializedEntity -> setHttpHeader "Content-Type" "application/json" >=> setBodyAsString serializedEntity <| next
    | _ -> setStatusCode 404 next

let isValidLocation (location: Location) =
    location.country.Length <=50 
    && location.city.Length <=50

let isValidUser (user: User) =
    user.email.Length <= 100 
    && user.first_name.Length <=50 
    && user.last_name.Length <=50 

let isValidVisit (visit: Visit) =
    visit.mark <= 5uy 

let updateLocation (oldLocation:Location) (httpContext: HttpContext) = 
    task {
        let! json = httpContext.ReadBodyFromRequest()
        if (json.Contains(": null"))
            then failwith "Null field"
        let newLocation = JsonConvert.DeserializeObject<LocationUpd>(json)
        let updatedLocation  = 
            { oldLocation with 
                distance = if newLocation.distance.HasValue |> not then oldLocation.distance else newLocation.distance.Value 
                city = if newLocation.city = null then oldLocation.city else newLocation.city 
                place = if newLocation.place = null then oldLocation.place else newLocation.place 
                country = if newLocation.country = null then oldLocation.country else newLocation.country }

        if (isValidLocation updatedLocation |> not)
        then failwith "Invalid data"
        
        return updatedLocation 
    }

let updateUser (oldUser:User) (httpContext: HttpContext) = 
    task {
        let! json = httpContext.ReadBodyFromRequest()
        if (json.Contains(": null"))
            then failwith "Null field"
        let newUser = JsonConvert.DeserializeObject<UserUpd>(json)
        let updatedUser  = 
            { oldUser with 
                first_name = if newUser.first_name = null then oldUser.first_name else newUser.first_name
                last_name = if newUser.last_name = null then oldUser.last_name else newUser.last_name 
                birth_date = if newUser.birth_date.HasValue |> not then oldUser.birth_date else newUser.birth_date.Value 
                gender = if newUser.gender.HasValue |> not then oldUser.gender else newUser.gender.Value 
                email = if newUser.email = null then oldUser.email else newUser.email }

        if (isValidUser updatedUser |> not)
        then failwith "Invalid data"
        
        return updatedUser 
    }

let getNewUserValue (oldValue: Visit) (newValue: VisitUpd) = 
    if (newValue.user.HasValue)
    then 
        visitUsers.[oldValue.user].TryRemove(oldValue.id) |> ignore
        visitUsers.[newValue.user.Value].TryAdd(oldValue.id, oldValue.id) |> ignore
        newValue.user.Value
    else 
        oldValue.user

let getNewLocationValue (oldValue: Visit) (newValue: VisitUpd) = 
    if (newValue.location.HasValue)
    then 
        visitLocations.[oldValue.location].TryRemove(oldValue.id) |> ignore
        visitLocations.[newValue.location.Value].TryAdd(oldValue.id, oldValue.id) |> ignore
        newValue.location.Value
    else 
        oldValue.location


let updateVisit (oldVisit:Visit) (httpContext: HttpContext) = 
    task {
        let! json = httpContext.ReadBodyFromRequest()
        if (json.Contains(": null"))
            then failwith "Null field"
        let newVisit = JsonConvert.DeserializeObject<VisitUpd>(json)
        let updatedVisit  = 
            { oldVisit with 
                user = getNewUserValue oldVisit newVisit
                location = getNewLocationValue oldVisit newVisit 
                visited_at = if newVisit.visited_at.HasValue |> not then oldVisit.visited_at else newVisit.visited_at.Value 
                mark = if newVisit.mark.HasValue |> not then oldVisit.mark else newVisit.mark.Value }

        if (isValidVisit updatedVisit |> not)
        then failwith "Invalid data"
        
        return updatedVisit 
    }

let updateEntity (collection: ConcurrentDictionary<int, 'a>) (serializedCollection: SerializedCollection) 
                 (updateFunc: UpdateEntity<'a>) (id: int) (next : HttpFunc) (httpContext: HttpContext) = 
    match collection.TryGetValue id with
    | true, entity -> 
        task {
            let! updatedEntity = updateFunc entity httpContext
            collection.[id] <- updatedEntity
            serializedCollection.[id] <- JsonConvert.SerializeObject(updatedEntity)
            return! setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| next <| httpContext 
        }
    | _ -> setStatusCode 404 >=> setBodyAsString "Value doesn't exist" <| next <| httpContext

let addLocation (next : HttpFunc) (httpContext: HttpContext) = 
    task {
        let! stringValue = httpContext.ReadBodyFromRequest()
        let location = JsonConvert.DeserializeObject<Location>(stringValue)
        if (isValidLocation location)
        then
            let result = match locations.TryAdd(location.id, location) with
                         | true -> 
                                   visitLocations.TryAdd(location.id, ConcurrentDictionary<int,int>()) |> ignore
                                   locationsSerialized.TryAdd(location.id, stringValue) |> ignore
                                   setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| next <| httpContext
                         | _ -> setStatusCode 400 >=> setBodyAsString "Value already exists" <| next <| httpContext 
            return! result
        else
            return! setStatusCode 400 >=> setBodyAsString "Invalidvalue" <| next <| httpContext    
    }

let addVisit (next : HttpFunc) (httpContext: HttpContext) = 
    task {
        let! stringValue = httpContext.ReadBodyFromRequest()
        let visit = JsonConvert.DeserializeObject<Visit>(stringValue)
        if (isValidVisit visit)
        then
            let result = match visits.TryAdd(visit.id, visit) with
                         | true -> 
                                   visitsSerialized.TryAdd(visit.id, stringValue) |> ignore
                                   setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| next <| httpContext
                         | _ -> setStatusCode 400 >=> setBodyAsString "Value already exists" <| next <| httpContext
            visitLocations.[visit.location].TryAdd(visit.id, visit.id) |> ignore
            visitUsers.[visit.user].TryAdd(visit.id, visit.id) |> ignore
            return! result      
        else
            return! setStatusCode 400 >=> setBodyAsString "Invalidvalue" <| next <| httpContext 
    }

let addUser (next : HttpFunc) (httpContext: HttpContext) = 
    task {
        let! stringValue = httpContext.ReadBodyFromRequest()
        let user = JsonConvert.DeserializeObject<User>(stringValue)
        if (isValidUser user)
        then
            let result = match users.TryAdd(user.id, user) with
                         | true ->
                                   visitUsers.TryAdd(user.id, ConcurrentDictionary<int,int>()) |> ignore
                                   usersSerialized.TryAdd(user.id, stringValue) |> ignore
                                   setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| next <| httpContext
                         | _ -> setStatusCode 400 >=> setBodyAsString "Value already exists" <| next <| httpContext 
            return! result         
        else
            return! setStatusCode 400 >=> setBodyAsString "Invalidvalue" <| next <| httpContext 
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


let getUserVisits userId (next : HttpFunc) (httpContext: HttpContext) = 
    match users.TryGetValue(userId) with
    | true, user ->
        let query = httpContext.BindQueryString<QueryVisit>()
        task { 
            let usersVisits = visitUsers.[userId].Keys  
                              |> Seq.map (fun key -> visits.[key])   
                              |> Seq.filter (filterByQueryVisit query)
                              |> Seq.map (fun visit -> {
                                                             mark = visit.mark
                                                             visited_at = visit.visited_at
                                                             place = locations.[visit.location].place
                                                         })
                              |> Seq.sortBy (fun v -> v.visited_at)
            return! json { visits = usersVisits } next httpContext
        }
    | false, _ ->
        setStatusCode 404 >=> setBodyAsString "Value doesn't exist" <| next <|  httpContext

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
        && (query.fromAge.IsNone || (diffYears ((float) user.Value.birth_date |> convertToDate) DateTime.Now ) >= query.fromAge.Value)

let getAvgMark locationId (next : HttpFunc) (httpContext: HttpContext) = 
    match locations.TryGetValue(locationId) with
    | true, location ->
        let query = httpContext.BindQueryString<QueryAvg>()
        task {
            let marks = visitLocations.[locationId].Keys 
                              |> Seq.map (fun key -> visits.[key])   
                              |> Seq.filter (filterByQueryAvg query)
            let avg = match marks with
                      | seq when Seq.isEmpty seq -> 0.0
                      | seq -> Math.Round(seq |> Seq.averageBy (fun visit -> (float)visit.mark), 5)
            return! json { avg = avg } next httpContext
        }
    | false, _ ->
        setStatusCode 404 >=> setBodyAsString "Value doesn't exist" <| next <| httpContext

let webApp = 
    choose [
        GET >=>
            choose [
                routef "/locations/%i" <| getEntity locationsSerialized
                routef "/users/%i" <| getEntity usersSerialized
                routef "/visits/%i" <| getEntity visitsSerialized
                
                routef "/users/%i/visits" getUserVisits
                routef "/locations/%i/avg" getAvgMark
            ]
        POST >=>
            choose [
                routef "/locations/%i" <| updateEntity locations locationsSerialized updateLocation
                routef "/users/%i" <| updateEntity users usersSerialized updateUser
                routef "/visits/%i" <| updateEntity visits visitsSerialized updateVisit

                route "/locations/new" >=> addLocation
                route "/users/new" >=> addUser
                route "/visits/new" >=> addVisit
            ]
        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger)=
    // logger.LogError(ex.Message)
    setStatusCode 400 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureApp (app : IApplicationBuilder) = 
    app.UseRequestCounter webApp
    app.UseGiraffeErrorHandler errorHandler
    app.UseGiraffe webApp

let loadData folder =
    Directory.EnumerateFiles(folder, "locations_*.json")
        |> Seq.map (File.ReadAllText >> JsonConvert.DeserializeObject<Locations>)
        |> Seq.collect (fun locationsObj -> locationsObj.locations)
        |> Seq.map (fun location -> 
            locations.TryAdd(location.id, location) |> ignore
            visitLocations.TryAdd(location.id, ConcurrentDictionary<int,int>())|> ignore
            locationsSerialized.TryAdd(location.id, JsonConvert.SerializeObject(location)) |> ignore) 
        |> Seq.toList
        |> ignore
    
    Directory.EnumerateFiles(folder, "users_*.json")
        |> Seq.map (File.ReadAllText >> JsonConvert.DeserializeObject<Users>)
        |> Seq.collect (fun usersObj -> usersObj.users)
        |> Seq.map (fun user -> 
            users.TryAdd(user.id, user) |> ignore
            visitUsers.TryAdd(user.id, ConcurrentDictionary<int,int>())|> ignore 
            usersSerialized.TryAdd(user.id, JsonConvert.SerializeObject(user)) |> ignore)
        |> Seq.toList
        |> ignore

    Directory.EnumerateFiles(folder, "visits_*.json")
        |> Seq.map (File.ReadAllText >> JsonConvert.DeserializeObject<Visits>)
        |> Seq.collect (fun visitObj -> visitObj.visits)
        |> Seq.map (fun visit -> 
            visits.TryAdd(visit.id, visit) |> ignore
            visitLocations.[visit.location].TryAdd(visit.id, visit.id) |> ignore
            visitUsers.[visit.user].TryAdd(visit.id, visit.id) |> ignore
            visitsSerialized.TryAdd(visit.id, JsonConvert.SerializeObject(visit)) |> ignore) 
        |> Seq.toList
        |> ignore

    Console.WriteLine("Locations: {0}, Users: {1}, Visits: {2}", locations.Count, users.Count, visits.Count)

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
        .Build()
        .Run()
    0