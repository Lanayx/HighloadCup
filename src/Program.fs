module HCup.App

open System
open System.IO
open System.IO.Compression
open System.Collections.Generic
open System.Collections.Concurrent
open System.Threading.Tasks
open System.Text
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Server.Kestrel.Core
open Microsoft.AspNetCore.Server.Kestrel.Transport
open Microsoft.Extensions.Logging
open Juraff.Common
open Juraff.Tasks
open Juraff.HttpHandlers
open Juraff.Middleware
open Juraff.HttpContextExtensions
open Jil
open HCup.Models
open HCup.RequestCounter
open HCup.Actors
open HCup.Router
open HCup.Parser
open HCup.Serializers

// ---------------------------------
// Web app
// ---------------------------------

[<Literal>]
let LocationsSize = 800000
[<Literal>]
let UsersSize = 1050000
[<Literal>]
let VisitsSize = 10050000

let mutable currentDate = DateTime.Now
let timestampBase = DateTime(1970, 1, 1, 0, 0, 0, 0)

let locations = Array.zeroCreate<Location> LocationsSize
let users = Array.zeroCreate<User> UsersSize
let visits = Array.zeroCreate<Visit> VisitsSize

let locationVisits = Array.zeroCreate<VisitsCollection> LocationsSize
let userVisits = Array.zeroCreate<VisitsCollectionSorted> UsersSize

type UpdateEntity<'a> = 'a -> string -> bool
type UpdateEntityStr<'a> = 'a -> string -> StructOption<'a> 
 
let inline serializeObject obj =
    JSON.Serialize(obj)

let inline deserializeObjectUnsafe<'a> (str: string) =
    JSON.Deserialize<'a>(str)

let inline deserializeObject<'a> (str: string) =
    try
        Some <| JSON.Deserialize<'a>(str)        
    with
    | exn ->
        None

let jsonCustom str (next : HttpFunc) (httpContext: HttpContext) =
    setHttpHeader "Content-Type" "application/json" >=> setBodyAsString str <| next <| httpContext

let inline checkStringFromRequest (stringValue: string) = 
    stringValue.Contains(": null") |> not

let getUser id next = 
    if (id > users.Length)
    then setStatusCode 404 next
        else
            match users.[id] with
            | entity when entity.id > 0 -> jsonCustom (serializeUser entity) next        
            | _ -> setStatusCode 404 next

let getVisit id next = 
    if (id > visits.Length)
    then setStatusCode 404 next
        else
            match visits.[id] with
            | entity when entity.id > 0 -> jsonCustom (serializeVisit entity) next        
            | _ -> setStatusCode 404 next

let getLocation id next = 
    if (id > locations.Length)
    then setStatusCode 404 next
        else
            match locations.[id] with
            | entity when entity.id > 0 -> jsonCustom (serializeLocation entity) next        
            | _ -> setStatusCode 404 next

let updateLocation (oldLocation:Location) json = 
    if checkStringFromRequest json
    then
        match deserializeObject<LocationUpd>(json) with
        | Some newLocation ->
            Location (
                       oldLocation.id,
                       (if newLocation.distance.HasValue then newLocation.distance.Value else oldLocation.distance),
                       (if newLocation.place |> isNotNull then newLocation.place else oldLocation.place),
                       (if newLocation.city |> isNotNull then newLocation.city else oldLocation.city),
                       (if newLocation.country |> isNotNull then newLocation.country else oldLocation.country)
                     ) |> Som
        | None -> Non
    else    
        Non

let updateUser (oldUser:User) json = 
    if checkStringFromRequest json
    then
        match deserializeObject<UserUpd>(json) with
        | Some newUser ->
            User (
                oldUser.id,
                (if newUser.first_name |> isNotNull then newUser.first_name else oldUser.first_name),
                (if newUser.last_name |> isNotNull then newUser.last_name else oldUser.last_name),
                (if newUser.birth_date.HasValue then newUser.birth_date.Value else oldUser.birth_date),
                (if newUser.gender.HasValue then newUser.gender.Value else oldUser.gender),
                (if newUser.email |> isNotNull then newUser.email else oldUser.email)
            ) |> Som
        | None -> Non
    else
        Non

let getNewUserValue (oldValue: Visit) (newValue: VisitUpd) = 
    if newValue.user.HasValue
    then 
        VisitActor.RemoveUserVisit oldValue.user userVisits.[oldValue.user] oldValue.id oldValue.visited_at
        let visitedAt = if newValue.visited_at.HasValue then newValue.visited_at.Value else oldValue.visited_at
        VisitActor.AddUserVisit newValue.user.Value userVisits.[newValue.user.Value] oldValue.id visitedAt
        newValue.user.Value
    else if (newValue.visited_at.HasValue)
    then
        VisitActor.RemoveUserVisit oldValue.user userVisits.[oldValue.user] oldValue.id oldValue.visited_at
        VisitActor.AddUserVisit oldValue.user userVisits.[oldValue.user] oldValue.id newValue.visited_at.Value
        oldValue.user
    else
        oldValue.user

let getNewLocationValue (oldValue: Visit) (newValue: VisitUpd) = 
    if (newValue.location.HasValue)
    then         
        VisitActor.RemoveLocationVisit oldValue.location locationVisits.[oldValue.location] oldValue.id
        VisitActor.AddLocationVisit newValue.location.Value locationVisits.[newValue.location.Value] oldValue.id
        newValue.location.Value
    else 
        oldValue.location


let updateVisit (oldVisit:Visit) json = 
    if checkStringFromRequest json
    then
        match deserializeObject<VisitUpd>(json) with
        | Some newVisit ->      
            Visit (
                oldVisit.id,
                getNewUserValue oldVisit newVisit,
                getNewLocationValue oldVisit newVisit,
                (if newVisit.visited_at.HasValue then newVisit.visited_at.Value else oldVisit.visited_at),
                (if newVisit.mark.HasValue then newVisit.mark.Value else oldVisit.mark)
            ) |> Som
        | None -> Non
    else
        Non

let updateVisitStr (id: int) (next : HttpFunc) (httpContext: HttpContext) =
    if (id > visits.Length)
    then setStatusCode 404 next httpContext
    else
        let oldEntity = visits.[id]
        match oldEntity with    
        | exEntity when exEntity.id = 0 -> 
            setStatusCode 404 next httpContext
        | _ -> 
            task {
                let! json = httpContext.ReadBodyFromRequest()
                match updateVisit oldEntity json with
                | Som newEntity ->
                    visits.[id] <- newEntity                 
                    return! setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| next <| httpContext 
                | Non -> return! setStatusCode 400 next httpContext
            }

let updateUserStr (id: int) (next : HttpFunc) (httpContext: HttpContext) =
    if (id > users.Length)
    then setStatusCode 404 next httpContext
    else
        let oldEntity = users.[id]
        match oldEntity with    
        | exEntity when exEntity.id = 0 -> 
            setStatusCode 404 next httpContext
        | _ -> 
            task {
                let! json = httpContext.ReadBodyFromRequest()
                match updateUser oldEntity json with
                | Som newEntity ->
                    users.[id] <- newEntity                 
                    return! setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| next <| httpContext 
                | Non -> return! setStatusCode 400 next httpContext
            }

let updateLocationStr (id: int) (next : HttpFunc) (httpContext: HttpContext) =
    if (id > locations.Length)
    then setStatusCode 404 next httpContext
    else
        let oldEntity = locations.[id]
        match oldEntity with    
        | exEntity when exEntity.id = 0 -> 
            setStatusCode 404 next httpContext
        | _ -> 
            task {
                let! json = httpContext.ReadBodyFromRequest()
                match updateLocation oldEntity json with
                | Som newEntity ->
                    locations.[id] <- newEntity                 
                    return! setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| next <| httpContext 
                | Non -> return! setStatusCode 400 next httpContext
            }

let addLocationInternal stringValue (next : HttpFunc) (httpContext: HttpContext) =
    match deserializeObject<LocationOld>(stringValue) with
    | Some location ->  
        if (location.city |> isNull || location.country |> isNull || location.place |> isNull)
        then setStatusCode 400 next httpContext
        else
            match locations.[location.id] with
                            | exLocation when exLocation.id > 0 -> setStatusCode 400 next httpContext  
                            | _ -> 
                                    locations.[location.id] <- Location(location.id, location.distance, location.place, location.city, location.country)
                                    locationVisits.[location.id] <- VisitsCollection()
                                    setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| next <| httpContext
    | None -> setStatusCode 400 next httpContext

let addLocation (id:int) (next : HttpFunc) (httpContext: HttpContext) = 
    task {
        let! stringValue = httpContext.ReadBodyFromRequest()     
        return! addLocationInternal stringValue next httpContext
    }

let addVisitInternal stringValue (next : HttpFunc) (httpContext: HttpContext) =     
    match deserializeObject<VisitOld>(stringValue) with
    | Some visit ->
        match visits.[visit.id] with
        | exVisit when exVisit.id > 0 -> setStatusCode 400 next httpContext 
        | _ -> 
              visits.[visit.id] <- Visit(visit.id, visit.user, visit.location, visit.visited_at, visit.mark)                                
              VisitActor.AddLocationVisit visit.location locationVisits.[visit.location] visit.id                         
              VisitActor.AddUserVisit visit.user userVisits.[visit.user] visit.id visit.visited_at
              setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| next <| httpContext
    | None -> setStatusCode 400 next httpContext

let addVisit (id:int) (next : HttpFunc) (httpContext: HttpContext) = 
    task {
        let! stringValue = httpContext.ReadBodyFromRequest()
        return! addVisitInternal stringValue next httpContext
    }

let addUserInternal stringValue (next : HttpFunc) (httpContext: HttpContext) =
    match deserializeObject<UserOld>(stringValue) with
    | Some user ->
        if (user.email |> isNull || user.first_name |> isNull || user.last_name |> isNull)
        then setStatusCode 400 next httpContext
        else
            match users.[user.id] with
                             | exUser when exUser.id > 0 -> setStatusCode 400 next httpContext 
                             | _ ->
                                       users.[user.id] <- User(user.id, user.first_name, user.last_name, user.birth_date, user.gender, user.email)
                                       userVisits.[user.id] <- VisitsCollectionSorted()
                                       setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| next <| httpContext
    | None -> setStatusCode 400 next httpContext

let addUser (id:int) (next : HttpFunc) (httpContext: HttpContext) = 
    task {
        let! stringValue = httpContext.ReadBodyFromRequest()
        return! addUserInternal stringValue next httpContext
    }

[<Struct>]
type QueryVisit = { fromDate: ParseResult<uint32>; toDate: ParseResult<uint32>; country: string; toDistance: ParseResult<uint8>}

let getUserVisitsQuery (httpContext: HttpContext) =
    let fromDate = queryNullableParse ParseResult.Empty "fromDate" UInt32.TryParse httpContext    
    let toDate = queryNullableParse fromDate "toDate" UInt32.TryParse httpContext
    let toDistance = queryNullableParse toDate "toDistance" Byte.TryParse httpContext
    match toDistance with
    | Error -> Non
    | _ ->
            let country = queryStringParse "country" httpContext
            Som {
                fromDate = fromDate
                toDate = toDate
                country = country
                toDistance = toDistance
            }  

let filterByQueryVisit (query: QueryVisit) (visit: Visit) =
    let location = locations.[visit.location]
    checkParseResult query.fromDate (fun fromDate -> visit.visited_at > fromDate)
        && (checkParseResult query.toDate (fun toDate -> visit.visited_at < toDate))
        && (checkParseResult query.toDistance (fun toDistance -> location.distance < toDistance))
        && (String.IsNullOrEmpty(query.country) || location.country = query.country)

let getUserVisits userId (next : HttpFunc) (httpContext: HttpContext) = 
    if (userId > users.Length)
    then setStatusCode 404 next httpContext
    else
        match users.[userId] with 
        | user when user.id > 0 ->
            match getUserVisitsQuery httpContext with
            | Som query ->
                let usersVisits = userVisits.[userId] 
                                      |> Seq.map (fun kv -> visits.[kv.Value])   
                                      |> Seq.filter (filterByQueryVisit query)
                                      |> Seq.map (fun visit ->
                                                                {
                                                                     mark = visit.mark
                                                                     visited_at = visit.visited_at
                                                                     place = locations.[visit.location].place
                                                                 })
                jsonCustom (serializeVisits usersVisits) next httpContext
            | Non -> setStatusCode 400 next httpContext       
        | _ ->
            setStatusCode 404 next httpContext

[<Struct>]
type QueryAvg = { fromDate: ParseResult<uint32>; toDate: ParseResult<uint32>; fromAge: ParseResult<int>; toAge: ParseResult<int>; gender: ParseResult<Sex>}

let getAvgMarkQuery (httpContext: HttpContext) =
    let fromDate = queryNullableParse ParseResult.Empty "fromDate" UInt32.TryParse httpContext    
    let toDate = queryNullableParse fromDate "toDate" UInt32.TryParse httpContext   
    let fromAge = queryNullableParse toDate "fromAge" Int32.TryParse httpContext
    let toAge = queryNullableParse fromAge "toAge" Int32.TryParse httpContext
    let gender = queryNullableParse toAge "gender" Sex.TryParse httpContext
    match gender with
    | Error -> Non
    | _ ->
            Som {
                fromDate = fromDate
                toDate = toDate
                fromAge = fromAge
                toAge = toAge
                gender = gender
            }  

let inline convertToDate timestamp =
    timestampBase.AddSeconds(timestamp)

let inline diffYears (startDate: DateTime) (endDate: DateTime) =
    (endDate.Year - startDate.Year - 1) + (if ((endDate.Month > startDate.Month) || ((endDate.Month = startDate.Month) && (endDate.Day >= startDate.Day))) then 1 else 0)


let filterByQueryAvg (query: QueryAvg) (visit: Visit) =
    let user = users.[visit.user]

    checkParseResult query.fromDate (fun fromDate -> visit.visited_at > fromDate)
        && (checkParseResult query.toDate (fun toDate -> visit.visited_at < toDate))
        && (checkParseResult query.gender (fun gender -> user.gender = gender))
        && (checkParseResult query.toAge (fun toAge -> (diffYears (user.birth_date |> convertToDate) currentDate ) <  toAge))
        && (checkParseResult query.fromAge (fun fromAge -> (diffYears (user.birth_date |> convertToDate) currentDate ) >= fromAge))

let getAvgMark locationId (next : HttpFunc) (httpContext: HttpContext) = 
    if (locationId > locations.Length)
    then setStatusCode 404 next httpContext
    else
        match locations.[locationId] with
        | location when location.id > 0 ->
            match getAvgMarkQuery httpContext with
            | Som query ->
                let markedVisits = locationVisits.[locationId] 
                                      |> Seq.map (fun key -> visits.[key])   
                                      |> Seq.filter (filterByQueryAvg query)
                let avg = match markedVisits with
                              | seq when Seq.isEmpty seq -> 0.0
                              | seq -> Math.Round(seq |> Seq.averageBy (fun markedVisit -> markedVisit.mark), 5, MidpointRounding.AwayFromZero)
                jsonCustom (serializeAvg avg) next httpContext
            | Non -> setStatusCode 400 next httpContext        
        | _ ->
            setStatusCode 404 <| next <| httpContext

let getActionsDictionary = Dictionary<Route, IdHandler>()
getActionsDictionary.Add(Route.Location, getLocation)
getActionsDictionary.Add(Route.User, getUser)
getActionsDictionary.Add(Route.Visit, getVisit)
getActionsDictionary.Add(Route.UserVisits, getUserVisits)
getActionsDictionary.Add(Route.LocationAvg, getAvgMark)

let postActionsDictionary = Dictionary<Route, IdHandler>()
postActionsDictionary.Add(Route.Location, updateLocationStr)
postActionsDictionary.Add(Route.User, updateUserStr)
postActionsDictionary.Add(Route.Visit, updateVisitStr)
postActionsDictionary.Add(Route.NewVisit, addVisit)
postActionsDictionary.Add(Route.NewUser, addUser)
postActionsDictionary.Add(Route.NewLocation, addLocation)


let webApp = 
    choose [
        GET >=> customGetRoutef getActionsDictionary
        POST >=> customPostRoutef postActionsDictionary
        setStatusCode 404 ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger)=
    // logger.LogError(ex.ToString())
    setStatusCode 400

// ---------------------------------
// Config and Main
// ---------------------------------

let configureApp (app : IApplicationBuilder) = 
    app.UseRequestCounter webApp
    // app.UseGiraffeErrorHandler errorHandler
    app.UseGiraffe webApp

let configureKestrel (options : KestrelServerOptions) =
    // options.ListenUnixSocket "/tmp/tkestrel.sock"
    options.ApplicationSchedulingMode <- Abstractions.Internal.SchedulingMode.Inline

let loadData folder =

    let locations = Directory.EnumerateFiles(folder, "locations_*.json")
                    |> Seq.map (File.ReadAllText >> deserializeObjectUnsafe<Locations>)
                    |> Seq.collect (fun locationsObj -> locationsObj.locations)
                    |> Seq.map (fun location -> 
                        locations.[location.id] <- Location(location.id, location.distance, location.place, location.city, location.country)
                        locationVisits.[location.id] <- VisitsCollection()) 
                    |> Seq.toList
    Console.Write("Locations {0} ", locations.Length)
    
    
    let users = Directory.EnumerateFiles(folder, "users_*.json")
                |> Seq.map (File.ReadAllText >> deserializeObjectUnsafe<Users>)
                |> Seq.collect (fun usersObj -> usersObj.users)
                |> Seq.map (fun user -> 
                    users.[user.id] <- User(user.id, user.first_name, user.last_name, user.birth_date, user.gender, user.email)
                    userVisits.[user.id] <- VisitsCollectionSorted())
                |> Seq.toList
    Console.Write("Users {0} ", users.Length)

    Console.WriteLine("Location {0} VisitLocations {1}", locations.[1], locationVisits.[1])
    let visits = Directory.EnumerateFiles(folder, "visits_*.json")
                |> Seq.map (File.ReadAllText >> deserializeObjectUnsafe<Visits>)
                |> Seq.collect (fun visitObj -> visitObj.visits)
                |> Seq.map (fun visit -> 
                    visits.[visit.id] <- Visit(visit.id, visit.user, visit.location, visit.visited_at, visit.mark)
                    locationVisits.[visit.location].Add(visit.id) |> ignore
                    userVisits.[visit.user].Add(visit.visited_at, visit.id) |> ignore) 
                |> Seq.toList
    Console.WriteLine("Visits: {0}", visits.Length)

    let str = Path.Combine(folder,"options.txt") 
                   |> File.ReadAllLines
    currentDate <- str.[0]
                   |> Int64.Parse
                   |> float
                   |> convertToDate


[<EntryPoint>]
let main argv =
    if Directory.Exists("./data")
    then Directory.Delete("./data",true)
    Directory.CreateDirectory("./data") |> ignore
    if File.Exists("/tmp/data/data.zip")
    then
        File.Copy("/tmp/data/options.txt","./data/options.txt") 
        ZipFile.ExtractToDirectory("/tmp/data/data.zip","./data")
    else ZipFile.ExtractToDirectory("data.zip","./data")
    loadData "./data"
    GC.Collect(2)
    WebHostBuilder()
        .UseKestrel(Action<KestrelServerOptions> configureKestrel)
        // .UseUrls("http://0.0.0.0:80")
        .Configure(Action<IApplicationBuilder> configureApp)
        .Build()
        .Run()
    0