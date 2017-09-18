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
open Microsoft.AspNetCore.Server.Kestrel.Core
open Microsoft.AspNetCore.Server.Kestrel.Transport
open Microsoft.Extensions.Logging
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

let visitLocations = Array.zeroCreate<VisitsCollection> LocationsSize
let visitUsers = Array.zeroCreate<VisitsCollection> UsersSize

type UpdateEntity<'a> = 'a -> string -> bool
 
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


let jsonCustom obj (next : HttpFunc) (httpContext: HttpContext) =
    setHttpHeader "Content-Type" "application/json" >=> setBodyAsString (serializeObject obj) <| next <| httpContext

let inline checkStringFromRequest (stringValue: string) = 
    stringValue.Contains(": null") |> not

let getEntity (collection: 'a[]) id next = 
    if (id > collection.Length)
    then setStatusCode 404 next
    else
        match box collection.[id] with
        | null -> setStatusCode 404 next
        | entity -> jsonCustom entity next

let updateLocation (oldLocation:Location) json = 
    if checkStringFromRequest json
    then
        match deserializeObject<LocationUpd>(json) with
        | Some newLocation ->
            if newLocation.distance.HasValue then oldLocation.distance <- newLocation.distance.Value
            if newLocation.city |> isNull |> not then oldLocation.city <- newLocation.city
            if newLocation.place |> isNull |> not then oldLocation.place <- newLocation.place
            if newLocation.country |> isNull |> not then oldLocation.country <- newLocation.country
            true
        | None -> false
    else    
        false

let updateUser (oldUser:User) json = 
    if checkStringFromRequest json
    then
        match deserializeObject<UserUpd>(json) with
        | Some newUser ->
            if newUser.first_name |> isNull |> not then oldUser.first_name <- newUser.first_name
            if newUser.last_name |> isNull |> not then oldUser.last_name <- newUser.last_name
            if newUser.birth_date.HasValue then oldUser.birth_date <- newUser.birth_date.Value
            if newUser.gender.HasValue then oldUser.gender <- newUser.gender.Value
            if newUser.email |> isNull |> not then oldUser.email <- newUser.email
            true
        | None -> false
    else
        false

let getNewUserValue (oldValue: Visit) (newValue: VisitUpd) = 
    if (newValue.user.HasValue)
    then 
        VisitActor.RemoveUserVisit oldValue.user visitUsers.[oldValue.user] oldValue.id
        VisitActor.AddUserVisit newValue.user.Value visitUsers.[newValue.user.Value] oldValue.id
        newValue.user.Value
    else 
        oldValue.user

let getNewLocationValue (oldValue: Visit) (newValue: VisitUpd) = 
    if (newValue.location.HasValue)
    then         
        VisitActor.RemoveLocationVisit oldValue.location visitLocations.[oldValue.location] oldValue.id
        VisitActor.AddLocationVisit newValue.location.Value visitLocations.[newValue.location.Value] oldValue.id
        newValue.location.Value
    else 
        oldValue.location


let updateVisit (oldVisit:Visit) json = 
    if checkStringFromRequest json
    then
        match deserializeObject<VisitUpd>(json) with
        | Some newVisit ->      
            oldVisit.user <- getNewUserValue oldVisit newVisit
            oldVisit.location <- getNewLocationValue oldVisit newVisit 
            if newVisit.visited_at.HasValue then oldVisit.visited_at <- newVisit.visited_at.Value 
            if newVisit.mark.HasValue then oldVisit.mark <- newVisit.mark.Value
            true
        | None -> false
    else
        false

let updateEntity (collection: 'a[])
                 (updateFunc: UpdateEntity<'a>) (id: int) (next : HttpFunc) (httpContext: HttpContext) =
    if (id > collection.Length)
    then setStatusCode 404 next httpContext
    else
        let oldEntity = collection.[id]
        match box oldEntity with    
        | null -> 
            setStatusCode 404 next httpContext
        | _ -> 
            task {
                let! json = httpContext.ReadBodyFromRequest()
                if updateFunc oldEntity json
                then return! setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| next <| httpContext 
                else return! setStatusCode 400 next httpContext
            }

let addLocationInternal stringValue (next : HttpFunc) (httpContext: HttpContext) =
    match deserializeObject<Location>(stringValue) with
    | Some location ->  
        if (location.city |> isNull || location.country |> isNull || location.place |> isNull)
        then setStatusCode 400 next httpContext
        else
            match box locations.[location.id] with
                            | null -> 
                                    locations.[location.id] <- location
                                    visitLocations.[location.id] <- VisitsCollection()
                                    setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| next <| httpContext
                            | _ -> setStatusCode 400 next httpContext  
    | None -> setStatusCode 400 next httpContext

let addLocation (id:int) (next : HttpFunc) (httpContext: HttpContext) = 
    task {
        let! stringValue = httpContext.ReadBodyFromRequest()     
        return! addLocationInternal stringValue next httpContext
    }

let addVisitInternal stringValue (next : HttpFunc) (httpContext: HttpContext) =     
    match deserializeObject<Visit>(stringValue) with
    | Some visit ->
        match box visits.[visit.id] with
                            | null -> 
                                    visits.[visit.id] <- visit                                
                                    VisitActor.AddLocationVisit visit.location visitLocations.[visit.location] visit.id                         
                                    VisitActor.AddUserVisit visit.user visitUsers.[visit.user] visit.id
                                    setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| next <| httpContext
                            | _ -> setStatusCode 400 next httpContext 
    | None -> setStatusCode 400 next httpContext

let addVisit (id:int) (next : HttpFunc) (httpContext: HttpContext) = 
    task {
        let! stringValue = httpContext.ReadBodyFromRequest()
        return! addVisitInternal stringValue next httpContext
    }

let addUserInternal stringValue (next : HttpFunc) (httpContext: HttpContext) =
    match deserializeObject<User>(stringValue) with
    | Some user ->
        if (user.email |> isNull || user.first_name |> isNull || user.last_name |> isNull)
        then setStatusCode 400 next httpContext
        else
            match box users.[user.id] with
                             | null ->
                                       users.[user.id] <- user
                                       visitUsers.[user.id] <- VisitsCollection()
                                       setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| next <| httpContext
                             | _ -> setStatusCode 400 next httpContext 
    | None -> setStatusCode 400 next httpContext

let addUser (id:int) (next : HttpFunc) (httpContext: HttpContext) = 
    task {
        let! stringValue = httpContext.ReadBodyFromRequest()
        return! addUserInternal stringValue next httpContext
    }

type UserVisit = { mark: float; visited_at: uint32; place: string }
type UserVisits = { visits: seq<UserVisit> }
[<Struct>]
type QueryVisit = { fromDate: ParseResult<uint32>; toDate: ParseResult<uint32>; country: string; toDistance: ParseResult<uint16>}

let getUserVisitsQuery (httpContext: HttpContext) =
    let fromDate = queryNullableParse ParseResult.Empty "fromDate" UInt32.TryParse httpContext    
    let toDate = queryNullableParse fromDate "toDate" UInt32.TryParse httpContext
    let toDistance = queryNullableParse toDate "toDistance" UInt16.TryParse httpContext
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
    let location = 
        if (String.IsNullOrEmpty(query.country) |> not || query.toDistance <> ParseResult.Empty)
        then Some locations.[visit.location]
        else None
    checkParseResult query.fromDate (fun fromDate -> visit.visited_at > fromDate)
        && (checkParseResult query.toDate (fun toDate -> visit.visited_at < toDate))
        && (checkParseResult query.toDistance (fun toDistance -> location.Value.distance < toDistance))
        && (String.IsNullOrEmpty(query.country) || location.Value.country = query.country)

let getUserVisits userId (next : HttpFunc) (httpContext: HttpContext) = 
    if (userId > users.Length)
    then setStatusCode 404 next httpContext
    else
        match box users.[userId] with        
        | null ->
            setStatusCode 404 next httpContext
        | user ->
            match getUserVisitsQuery httpContext with
            | Som query ->
                let usersVisits = visitUsers.[userId] 
                                      |> Seq.map (fun key -> visits.[key])   
                                      |> Seq.filter (filterByQueryVisit query)
                                      |> Seq.map (fun visit -> {
                                                                     mark = visit.mark
                                                                     visited_at = visit.visited_at
                                                                     place = locations.[visit.location].place
                                                                 })
                                      |> Seq.sortBy (fun v -> v.visited_at)                
                jsonCustom { visits = usersVisits } next httpContext
            | Non -> setStatusCode 400 next httpContext

type Average = { avg: float }
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
        match box locations.[locationId] with
        | null ->
            setStatusCode 404 <| next <| httpContext
        | location ->
            match getAvgMarkQuery httpContext with
            | Som query ->
                let markedVisits = visitLocations.[locationId] 
                                      |> Seq.map (fun key -> visits.[key])   
                                      |> Seq.filter (filterByQueryAvg query)
                let avg = match markedVisits with
                              | seq when Seq.isEmpty seq -> 0.0
                              | seq -> Math.Round(seq |> Seq.averageBy (fun markedVisit -> markedVisit.mark), 5, MidpointRounding.AwayFromZero)
                jsonCustom { avg = avg } next httpContext
            | Non -> setStatusCode 400 next httpContext

let getActionsDictionary = Dictionary<Route, IdHandler>()
getActionsDictionary.Add(Route.Location, getEntity locations)
getActionsDictionary.Add(Route.User, getEntity users)
getActionsDictionary.Add(Route.Visit, getEntity visits)
getActionsDictionary.Add(Route.UserVisits, getUserVisits)
getActionsDictionary.Add(Route.LocationAvg, getAvgMark)

let postActionsDictionary = Dictionary<Route, IdHandler>()
postActionsDictionary.Add(Route.Location, updateEntity locations updateLocation)
postActionsDictionary.Add(Route.User, updateEntity users updateUser)
postActionsDictionary.Add(Route.Visit, updateEntity visits updateVisit)
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
                        locations.[location.id] <- location
                        visitLocations.[location.id] <- VisitsCollection()) 
                    |> Seq.toList
    Console.Write("Locations {0} ", locations.Length)
    
    let users = Directory.EnumerateFiles(folder, "users_*.json")
                |> Seq.map (File.ReadAllText >> deserializeObjectUnsafe<Users>)
                |> Seq.collect (fun usersObj -> usersObj.users)
                |> Seq.map (fun user -> 
                    users.[user.id] <- user
                    visitUsers.[user.id] <- VisitsCollection())
                |> Seq.toList
    Console.Write("Users {0} ", users.Length)

    let visits = Directory.EnumerateFiles(folder, "visits_*.json")
                |> Seq.map (File.ReadAllText >> deserializeObjectUnsafe<Visits>)
                |> Seq.collect (fun visitObj -> visitObj.visits)
                |> Seq.map (fun visit -> 
                    visits.[visit.id] <- visit
                    visitLocations.[visit.location].Add(visit.id) |> ignore
                    visitUsers.[visit.user].Add(visit.id) |> ignore) 
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