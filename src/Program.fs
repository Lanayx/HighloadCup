module HCup.App

open System
open System.Buffers
open System.IO
open System.IO.Compression
open System.Collections.Generic
open System.Collections.Concurrent
open System.Threading.Tasks
open System.Text
open System.Threading
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Server.Kestrel.Core
open Microsoft.AspNetCore.Server.Kestrel.Transport
open Microsoft.Extensions.Primitives
open Microsoft.Extensions.Logging
// open Juraff.Common
// open Juraff.Tasks
// open Juraff.HttpHandlers
// open Juraff.Middleware
// open Juraff.HttpContextExtensions
open Zebra
open Zebra.State
open Zebra.Router
open Zebra.Middleware
open Utf8Json
open HCup.Models
open HCup.RequestCounter
open HCup.Actors
open HCup.Parser
open HCup.MethodCounter

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

let jsonStringValues = StringValues "application/json"

type UpdateEntity<'a> = 'a -> string -> bool

let inline isNotNull x = isNull x |> not

let inline deserializeObjectUnsafe<'a> (str: string) =
    JsonSerializer.Deserialize<'a>(str)

let inline deserializeObject<'a> (str: string) : 'a option =
    try
        Some <| JsonSerializer.Deserialize<'a>(str)
    with
    | exn ->
        Option.None

let inline checkStringFromRequest (stringValue: string) =
    stringValue.Contains(": null") |> not

let getUser id (ctx: State<'a>) =
    Interlocked.Increment(getUserCount) |> ignore
    Console.WriteLine("In User")
    if (id > UsersSize)
    then ctx { status 404 }
        else
            let user = users.[id]
            match box user with
            | null -> ctx { status 404 }
            | _ -> ctx { json user }

let getVisit id (ctx: State<'a>) =
    Interlocked.Increment(getVisitCount) |> ignore
    if (id > VisitsSize)
    then ctx { status 404 }
    else
        let visit = visits.[id]
        match box visit with
        | null -> ctx { status 404 }
        | _ -> ctx { json visit }

let getLocation id (ctx: State<'a>) =
    Interlocked.Increment(getLocationCount) |> ignore
    if (id > LocationsSize)
    then ctx { status 404 }
        else
            let location = locations.[id]
            match box location with
            | null -> ctx { status 404 }
            | _ -> ctx { json location }

let updateLocationInternal (oldLocation:Location) json =
    if checkStringFromRequest json
    then
        match deserializeObject<LocationUpd>(json) with
        | Some newLocation ->
            if newLocation.distance.HasValue then oldLocation.distance <- newLocation.distance.Value
            if newLocation.city |> isNotNull then oldLocation.city <- newLocation.city
            if newLocation.place |> isNotNull then oldLocation.place <- newLocation.place
            if newLocation.country |> isNotNull then oldLocation.country <- newLocation.country
            true
        | _ -> false
    else
        false

let updateUserInternal (oldUser:User) json =
    if checkStringFromRequest json
    then
        match deserializeObject<UserUpd>(json) with
        | Some newUser ->
            if newUser.first_name |> isNotNull then oldUser.first_name <- newUser.first_name
            if newUser.last_name |> isNotNull then oldUser.last_name <- newUser.last_name
            if newUser.birth_date.HasValue then oldUser.birth_date <- newUser.birth_date.Value
            if newUser.gender.HasValue then oldUser.gender <- newUser.gender.Value
            if newUser.email |> isNotNull then oldUser.email <- newUser.email
            true
        | _ -> false
    else
        false

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


let updateVisitInternal (oldVisit:Visit) json =
    if checkStringFromRequest json
    then
        match deserializeObject<VisitUpd>(json) with
        | Some newVisit ->
            oldVisit.user <- getNewUserValue oldVisit newVisit
            oldVisit.location <- getNewLocationValue oldVisit newVisit
            if newVisit.visited_at.HasValue then oldVisit.visited_at <- newVisit.visited_at.Value
            if newVisit.mark.HasValue then oldVisit.mark <- newVisit.mark.Value
            true
        | _ -> false
    else
        false

let updateVisit (id: int) (ctx: State<'a>) =
    if (id > VisitsSize)
    then ctx { status 404 }
    else
        let oldEntity = visits.[id]
        match box oldEntity with
        | null ->
            ctx { status 404 }
        | _ ->
            use reader = new StreamReader(ctx.HttpContext.Request.Body)
            let json = reader.ReadToEnd()
            if updateVisitInternal oldEntity json
            then
                ctx {
                    setHeader "Content-Type" "application/json"
                    text "{}"
                }
            else
                ctx {
                    status 400
                }

let updateUser (id: int) (ctx: State<'a>) =
    if (id > UsersSize)
    then ctx { status 404 }
    else
        let oldEntity = users.[id]
        match box oldEntity with
        | null ->
             ctx { status 404 }
        | _ ->
            use reader = new StreamReader(ctx.HttpContext.Request.Body)
            let json = reader.ReadToEnd()
            if updateUserInternal oldEntity json
            then
                ctx {
                    setHeader "Content-Type" "application/json"
                    text "{}"
                }
            else
                ctx {
                    status 400
                }

let updateLocation (id: int) (ctx: State<'a>) =
    if (id > LocationsSize)
    then ctx { status 404 }
    else
        let oldEntity = locations.[id]
        match box oldEntity with
        | null ->
            ctx { status 404 }
        | _ ->
            use reader = new StreamReader(ctx.HttpContext.Request.Body)
            let json = reader.ReadToEnd()
            if updateLocationInternal oldEntity json
            then
                ctx {
                    setHeader "Content-Type" "application/json"
                    text "{}"
                }
            else
                ctx {
                    status 400
                }

let addLocationInternal stringValue (ctx: State<'a>) =
    match deserializeObject<Location>(stringValue) with
    | Some location ->
        if (location.city |> isNull || location.country |> isNull || location.place |> isNull)
        then ctx { status 400 }
        else
            match box locations.[location.id] with
                            | null ->
                                    locations.[location.id] <- location
                                    locationVisits.[location.id] <- VisitsCollection()
                                    ctx {
                                        setHeader "Content-Type" "application/json"
                                        text "{}"
                                    }
                            | _ -> ctx { status 400 }
    | _ -> ctx { status 400 }

let addLocation (ctx: State<'a>) =
    use reader = new StreamReader(ctx.HttpContext.Request.Body)
    let stringValue = reader.ReadToEnd()
    addLocationInternal stringValue ctx

let addVisitInternal stringValue (ctx: State<'a>) =
    match deserializeObject<Visit>(stringValue) with
    | Some visit ->
        match box visits.[visit.id] with
        | null ->
            visits.[visit.id] <- visit
            VisitActor.AddLocationVisit visit.location locationVisits.[visit.location] visit.id
            VisitActor.AddUserVisit visit.user userVisits.[visit.user] visit.id visit.visited_at
            ctx {
                setHeader "Content-Type" "application/json"
                text "{}"
            }
        | _ -> ctx { status 400 }
    | _ -> ctx { status 400 }

let addVisit (ctx: State<'a>) =
    use reader = new StreamReader(ctx.HttpContext.Request.Body)
    let stringValue = reader.ReadToEnd()
    addVisitInternal stringValue ctx

let addUserInternal stringValue (ctx: State<'a>) =
    match deserializeObject<User>(stringValue) with
    | Some user ->
        if (user.email |> isNull || user.first_name |> isNull || user.last_name |> isNull)
        then ctx { status 400 }
        else
            match box users.[user.id] with
                             | null ->
                                       users.[user.id] <- user
                                       userVisits.[user.id] <- VisitsCollectionSorted()
                                       ctx {
                                           setHeader "Content-Type" "application/json"
                                           text "{}"
                                       }
                             | _ -> ctx { status 400 }
    | _ -> ctx { status 400 }

let addUser (ctx: State<'a>) =
    use reader = new StreamReader(ctx.HttpContext.Request.Body)
    let stringValue = reader.ReadToEnd()
    addUserInternal stringValue ctx

[<Struct>]
type QueryVisit = { fromDate: ParseResult<uint32>; toDate: ParseResult<uint32>; country: string; toDistance: ParseResult<uint8>}

let getUserVisitsQuery (httpContext: HttpContext) =
    let fromDate = queryNullableParse ParseResult.Empty "fromDate" uint32Parse httpContext
    let toDate = queryNullableParse fromDate "toDate" uint32Parse httpContext
    let toDistance = queryNullableParse toDate "toDistance" byteParse httpContext
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
    match query.fromDate with
    | Success fromDate ->
        visit.visited_at > fromDate
    | _ -> true
    &&
    match query.toDate with
    | Success toDate ->
        visit.visited_at < toDate
    | _ -> true
    &&
    match query.toDistance with
    | Success toDistance ->
        location.distance < toDistance
    | _ -> true
    &&
    (String.IsNullOrEmpty(query.country) || location.country = query.country)

[<Struct>]
type UserVisits = { visits: seq<UserVisit> }
let getUserVisits userId (ctx: State<'a>) =
    Interlocked.Increment(getVisitsCount) |> ignore

    Console.WriteLine("In getUserVisits")
    if (userId > UsersSize)
    then ctx { status 404 }
    else
        let user = users.[userId]
        match box user with
        | null ->
            ctx { status 404 }
        | _ ->
            match getUserVisitsQuery ctx.HttpContext with
            | Som query ->
                let filterQuery = filterByQueryVisit query
                let usersVisits = userVisits.[userId]
                                      |> Seq.map (fun kv -> visits.[kv.Value])
                                      |> Seq.filter filterQuery
                                      |> Seq.map (fun visit ->
                                                                {
                                                                     mark = visit.mark
                                                                     visited_at = visit.visited_at
                                                                     place = locations.[visit.location].place
                                                                 })
                ctx { json { visits = usersVisits } }
            | Non -> ctx { status 400 }

[<Struct>]
type QueryAvg = { fromDate: ParseResult<uint32>; toDate: ParseResult<uint32>; fromAge: ParseResult<int>; toAge: ParseResult<int>; gender: ParseResult<char>}


let getAvgMarkQuery (httpContext: HttpContext) =
    let fromDate = queryNullableParse ParseResult.Empty "fromDate" uint32Parse httpContext
    let toDate = queryNullableParse fromDate "toDate" uint32Parse httpContext
    let fromAge = queryNullableParse toDate "fromAge" int32Parse httpContext
    let toAge = queryNullableParse fromAge "toAge" int32Parse httpContext
    let gender = queryNullableParse toAge "gender" genderParse httpContext
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
    timestampBase.AddSeconds((float)timestamp)

let inline diffYears (startDate: DateTime) (endDate: DateTime) =
    (endDate.Year - startDate.Year - 1) + (if ((endDate.Month > startDate.Month) || ((endDate.Month = startDate.Month) && (endDate.Day >= startDate.Day))) then 1 else 0)

let inline filterByQueryAvg (query: QueryAvg) (visit: Visit) =
    let user = users.[visit.user]
    match query.fromDate with
    | Success fromDate ->
        visit.visited_at > fromDate
    | _ -> true
    &&
    match query.toDate with
    | Success toDate ->
        visit.visited_at < toDate
    | _ -> true
    &&
    match query.gender with
    | Success gender ->
                user.gender = gender
    | _ -> true
    &&
    match query.toAge with
    | Success toAge ->
        (diffYears (user.birth_date |> convertToDate) currentDate) < toAge
    | _ -> true
    &&
    match query.fromAge with
    | Success fromAge ->
        (diffYears (user.birth_date |> convertToDate) currentDate ) >= fromAge
    | _ -> true

[<Struct>]
type Avg = { avg: float }

let getAvgMark locationId (ctx: State<'a>) =
    Interlocked.Increment(getAvgCount) |> ignore
    if (locationId > LocationsSize)
    then ctx { status 404 }
    else
        let location = locations.[locationId]
        match box location with
        | null ->
            ctx { status 404 }
        | _ ->
            match getAvgMarkQuery ctx.HttpContext with
            | Som query ->
                let filterQuery = filterByQueryAvg query
                let currentVisits = locationVisits.[locationId]
                let currentVisitsLength = currentVisits.Count - 1
                let mutable sum = 0.0
                let mutable markedVisitsCount = 0.0
                for i = 0 to currentVisitsLength do
                    let visit = visits.[currentVisits.[i]]
                    if filterQuery visit
                    then
                        markedVisitsCount <- markedVisitsCount + 1.0
                        sum <- sum + (float)visit.mark
                let avg = if markedVisitsCount > 0.0
                          then Math.Round (sum/markedVisitsCount, 5, MidpointRounding.AwayFromZero)
                          else 0.0
                ctx { json { avg = avg } }
            | Non -> ctx { status 400 }

let webApp =
    router [
        subRoute "/visits" [
            get1 "/%i" => getVisit
            post1 "/%i" => updateVisit
            post "/new" => addVisit
        ]
        subRoute "/users" [
            get1 "/%i" => getUser
            get1 "/%i/visits" => getUserVisits
            post1 "/%i" => updateUser
            post "/new" => addUser
        ]
        subRoute "/locations" [
            get1 "/%i" => getLocation
            get1 "/%i/avg" => getAvgMark
            post "/new" => addLocation
        ]
    ]

// ---------------------------------
// Error handler
// ---------------------------------

let fallback : Zapp<_> = (fun ctx -> ctx {              // fall back function for the app if we return false
    text "Url Not Found"
    status 404
})

// ---------------------------------
// Config and Main
// ---------------------------------

let configureApp (app : IApplicationBuilder) =
    app.UseRequestCounter webApp
    app.UseZebraMiddleware<int>(0 , fallback, webApp) |> ignore

let configureKestrel (options : KestrelServerOptions) =
    options.ApplicationSchedulingMode <- Abstractions.Internal.SchedulingMode.Inline
    options.AllowSynchronousIO <- false

let loadData folder =

    let locations = Directory.EnumerateFiles(folder, "locations_*.json")
                    |> Seq.map (File.ReadAllText >> deserializeObjectUnsafe<Locations>)
                    |> Seq.collect (fun locationsObj -> locationsObj.locations)
                    |> Seq.map (fun location ->
                        locations.[location.id] <- location
                        locationVisits.[location.id] <- VisitsCollection())
                    |> Seq.toList
    Console.Write("Locations {0} ", locations.Length)


    let users = Directory.EnumerateFiles(folder, "users_*.json")
                |> Seq.map (File.ReadAllText >> deserializeObjectUnsafe<Users>)
                |> Seq.collect (fun usersObj -> usersObj.users)
                |> Seq.map (fun user ->
                    users.[user.id] <- user
                    userVisits.[user.id] <- VisitsCollectionSorted())
                |> Seq.toList
    Console.Write("Users {0} ", users.Length)

    let visits = Directory.EnumerateFiles(folder, "visits_*.json")
                |> Seq.map (File.ReadAllText >> deserializeObjectUnsafe<Visits>)
                |> Seq.collect (fun visitObj -> visitObj.visits)
                |> Seq.map (fun visit ->
                    visits.[visit.id] <- visit
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
        .Configure(Action<IApplicationBuilder> configureApp)
        .Build()
        .Run()
    0