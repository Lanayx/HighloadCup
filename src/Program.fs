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
open ServiceStack.Text
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

let currentDate = DateTime.Now
let timestampBase = DateTime(1970, 1, 1, 0, 0, 0, 0)

let locations = Array.zeroCreate<Location> LocationsSize
let users = Array.zeroCreate<User> UsersSize
let visits = Array.zeroCreate<Visit> VisitsSize

let visitLocations = Array.zeroCreate<VisitsCollection> LocationsSize
let visitUsers = Array.zeroCreate<VisitsCollection> UsersSize

type UpdateEntity<'a> = 'a -> HttpContext -> Task<'a>
 
let serializeObject obj =
    JsonSerializer.SerializeToString(obj)

let deserializeObject<'a> str =
    JsonSerializer.DeserializeFromString<'a>(str)

let jsonCustom obj (next : HttpFunc) (httpContext: HttpContext) =
    setHttpHeader "Content-Type" "application/json" >=> setBodyAsString (serializeObject obj) <| next <| httpContext

let getStringFromRequest (httpContext: HttpContext) = 
    task {
        let! stringValue = httpContext.ReadBodyFromRequest()
        if (stringValue.Contains(": null"))
            then failwith "Null field"
        return stringValue
    }

let getEntity (collection: 'a[]) id next = 
    if (id > collection.Length)
    then setStatusCode 404 next
    else
        match box collection.[id] with
        | null -> setStatusCode 404 next
        | entity -> jsonCustom entity next

let isValidLocation (location: Location) =
    location.id > 0
    && location.country.Length <=50 
    && location.city.Length <=50

let isValidUser (user: User) =
    user.id > 0
    && user.email.Length <= 100 
    && user.first_name.Length <=50 
    && user.last_name.Length <=50 

let isValidVisit (visit: Visit) =
    visit.id > 0
    && visit.mark <= 5uy 

let updateLocation (oldLocation:Location) (httpContext: HttpContext) = 
    task {
        let! json = getStringFromRequest httpContext
        let newLocation = deserializeObject<LocationUpd>(json)
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
        let! json = getStringFromRequest httpContext
        let newUser = deserializeObject<UserUpd>(json)
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


let updateVisit (oldVisit:Visit) (httpContext: HttpContext) = 
    task {
        let! json = getStringFromRequest httpContext
        let newVisit = deserializeObject<VisitUpd>(json)
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
                let! updatedEntity = updateFunc oldEntity httpContext
                collection.[id] <- updatedEntity
                return! setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| next <| httpContext 
            }

let addLocation (next : HttpFunc) (httpContext: HttpContext) = 
    task {
        let! stringValue = getStringFromRequest httpContext       
        let location = deserializeObject<Location>(stringValue)   
        if (isValidLocation location)
        then
            let result = match box locations.[location.id] with
                         | null -> 
                                   locations.[location.id] <- location
                                   visitLocations.[location.id] <- VisitsCollection()
                                   setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| next <| httpContext
                         | _ -> setStatusCode 400 >=> setBodyAsString "Value already exists" <| next <| httpContext 
            return! result
        else
            return! setStatusCode 400 >=> setBodyAsString "Invalidvalue" <| next <| httpContext    
    }

let addVisit (next : HttpFunc) (httpContext: HttpContext) = 
    task {
        let! stringValue = getStringFromRequest httpContext
        let visit = deserializeObject<Visit>(stringValue)  
        if (isValidVisit visit)
        then
            let result = match box visits.[visit.id] with
                         | null -> 
                                   visits.[visit.id] <- visit                                
                                   VisitActor.AddLocationVisit visit.location visitLocations.[visit.location] visit.id                         
                                   VisitActor.AddUserVisit visit.user visitUsers.[visit.user] visit.id
                                   setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| next <| httpContext
                         | _ -> setStatusCode 400 >=> setBodyAsString "Value already exists" <| next <| httpContext
            return! result      
        else
            return! setStatusCode 400 >=> setBodyAsString "Invalidvalue" <| next <| httpContext 
    }

let addUser (next : HttpFunc) (httpContext: HttpContext) = 
    task {
        let! stringValue = getStringFromRequest httpContext
        let user = deserializeObject<User>(stringValue)    
        if (isValidUser user)
        then
            let result = match box users.[user.id] with
                         | null ->
                                   users.[user.id] <- user
                                   visitUsers.[user.id] <- VisitsCollection()
                                   setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| next <| httpContext
                         | _ -> setStatusCode 400 >=> setBodyAsString "Value already exists" <| next <| httpContext 
            return! result         
        else
            return! setStatusCode 400 >=> setBodyAsString "Invalidvalue" <| next <| httpContext 
    }

type UserVisit = { mark: uint8; visited_at: uint32; place: string }
type UserVisits = { visits: seq<UserVisit> }
type QueryVisit = { fromDate: ParseResult<uint32>; toDate: ParseResult<uint32>; country: string; toDistance: ParseResult<uint16>}

let getUserVisitsQuery (httpContext: HttpContext) =
    let fromDate = queryNullableParse ParseResult.Empty "fromDate" UInt32.TryParse httpContext    
    let toDate = queryNullableParse fromDate "toDate" UInt32.TryParse httpContext
    let toDistance = queryNullableParse toDate "toDistance" UInt16.TryParse httpContext
    match toDistance with
    | Error -> None
    | _ ->
            let country = queryStringParse "country" httpContext
            Some {
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
            | Some query ->
                task { 
                    let usersVisits = visitUsers.[userId] 
                                      |> Seq.map (fun key -> visits.[key])   
                                      |> Seq.filter (filterByQueryVisit query)
                                      |> Seq.map (fun visit -> {
                                                                     mark = visit.mark
                                                                     visited_at = visit.visited_at
                                                                     place = locations.[visit.location].place
                                                                 })
                                      |> Seq.sortBy (fun v -> v.visited_at)
                    return! jsonCustom { visits = usersVisits } next httpContext
                }
            | None -> setStatusCode 400 next httpContext

type Average = { avg: float }
type QueryAvg = { fromDate: ParseResult<uint32>; toDate: ParseResult<uint32>; fromAge: ParseResult<int>; toAge: ParseResult<int>; gender: ParseResult<Sex>}

let getAvgMarkQuery (httpContext: HttpContext) =
    let fromDate = queryNullableParse ParseResult.Empty "fromDate" UInt32.TryParse httpContext    
    let toDate = queryNullableParse fromDate "toDate" UInt32.TryParse httpContext   
    let fromAge = queryNullableParse toDate "fromAge" Int32.TryParse httpContext
    let toAge = queryNullableParse fromAge "toAge" Int32.TryParse httpContext
    let gender = queryNullableParse toAge "gender" Sex.TryParse httpContext
    match gender with
    | Error -> None
    | _ ->
            Some {
                fromDate = fromDate
                toDate = toDate
                fromAge = fromAge
                toAge = toAge
                gender = gender
            }  

let convertToDate timestamp =
    timestampBase.AddSeconds(timestamp)

let diffYears (startDate: DateTime) (endDate: DateTime) =
    (endDate.Year - startDate.Year - 1) + (if ((endDate.Month > startDate.Month) || ((endDate.Month = startDate.Month) && (endDate.Day >= startDate.Day))) then 1 else 0)


let filterByQueryAvg (query: QueryAvg) (visit: Visit) =
    let user = 
        if (query.gender <> ParseResult.Empty || query.fromAge <> ParseResult.Empty || query.toAge <> ParseResult.Empty)
        then Some users.[visit.user]
        else None

    checkParseResult query.fromDate (fun fromDate -> visit.visited_at > fromDate)
        && (checkParseResult query.toDate (fun toDate -> visit.visited_at < toDate))
        && (checkParseResult query.gender (fun gender -> user.Value.gender = gender))
        && (checkParseResult query.toAge (fun toAge -> (diffYears ((float) user.Value.birth_date |> convertToDate) currentDate ) <  toAge))
        && (checkParseResult query.fromAge (fun fromAge -> (diffYears ((float) user.Value.birth_date |> convertToDate) currentDate ) >= fromAge))

let getAvgMark locationId (next : HttpFunc) (httpContext: HttpContext) = 
    if (locationId > locations.Length)
    then setStatusCode 404 next httpContext
    else
        match box locations.[locationId] with
        | null ->
            setStatusCode 404 <| next <| httpContext
        | location ->
            match getAvgMarkQuery httpContext with
            | Some query ->
                task {
                    let marks = visitLocations.[locationId] 
                                      |> Seq.map (fun key -> visits.[key])   
                                      |> Seq.filter (filterByQueryAvg query)
                    let avg = match marks with
                              | seq when Seq.isEmpty seq -> 0.0
                              | seq -> Math.Round(seq |> Seq.averageBy (fun visit -> (float)visit.mark), 5)
                    return! jsonCustom { avg = avg } next httpContext
                }
            | None -> setStatusCode 400 next httpContext

let getActionsDictionary = Dictionary<string, IdHandler>()
getActionsDictionary.Add("/locations/%i", getEntity locations)
getActionsDictionary.Add("/users/%i", getEntity users)
getActionsDictionary.Add("/visits/%i", getEntity visits)
getActionsDictionary.Add("/users/%i/visits", getUserVisits)
getActionsDictionary.Add("/locations/%i/avg", getAvgMark)

let postActionsDictionary = Dictionary<string, IdHandler>()
postActionsDictionary.Add("/locations/%i", updateEntity locations updateLocation)
postActionsDictionary.Add("/users/%i", updateEntity users updateUser)
postActionsDictionary.Add("/visits/%i", updateEntity visits updateVisit)


let webApp = 
    choose [
        GET >=>
            choose [
                customRoutef getActionsDictionary
            ]
        POST >=>
            choose [
                route "/locations/new" >=> addLocation
                route "/users/new" >=> addUser
                route "/visits/new" >=> addVisit

                customRoutef postActionsDictionary
            ]
        setStatusCode 404 >=> text "Not Found" ]

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
    app.UseGiraffeErrorHandler errorHandler
    app.UseGiraffe webApp

let configureKestrel (options : KestrelServerOptions) =
    // options.ListenUnixSocket "/tmp/tkestrel.sock"
    options.ApplicationSchedulingMode <- Abstractions.Internal.SchedulingMode.Inline

let loadData folder =

    let locations = Directory.EnumerateFiles(folder, "locations_*.json")
                    |> Seq.map (File.ReadAllText >> deserializeObject<Locations>)
                    |> Seq.collect (fun locationsObj -> locationsObj.locations)
                    |> Seq.map (fun location -> 
                        locations.[location.id] <- location
                        visitLocations.[location.id] <- VisitsCollection()) 
                    |> Seq.toList
    Console.Write("Locations {0} ", locations.Length)
    
    let users = Directory.EnumerateFiles(folder, "users_*.json")
                |> Seq.map (File.ReadAllText >> deserializeObject<Users>)
                |> Seq.collect (fun usersObj -> usersObj.users)
                |> Seq.map (fun user -> 
                    users.[user.id] <- user
                    visitUsers.[user.id] <- VisitsCollection())
                |> Seq.toList
    Console.Write("Users {0} ", users.Length)

    let visits = Directory.EnumerateFiles(folder, "visits_*.json")
                |> Seq.map (File.ReadAllText >> deserializeObject<Visits>)
                |> Seq.collect (fun visitObj -> visitObj.visits)
                |> Seq.map (fun visit -> 
                    visits.[visit.id] <- visit
                    visitLocations.[visit.location].Add(visit.id) |> ignore
                    visitUsers.[visit.user].Add(visit.id) |> ignore) 
                |> Seq.toList

    Console.WriteLine("Visits: {0}", visits.Length)

[<EntryPoint>]
let main argv =
    if Directory.Exists("./data")
    then Directory.Delete("./data",true)
    Directory.CreateDirectory("./data") |> ignore
    if File.Exists("/tmp/data/data.zip")
    then ZipFile.ExtractToDirectory("/tmp/data/data.zip","./data")
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