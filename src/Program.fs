module HCup.App

open System
open System.Buffers
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
open Microsoft.Extensions.Primitives
open Microsoft.Extensions.Logging
open Juraff.Common
open Juraff.Tasks
open Juraff.HttpHandlers
open Juraff.Middleware
open Juraff.HttpContextExtensions
open HCup.Models
open HCup.RequestCounter
open HCup.Actors
open HCup.Parser
open Utf8Json
open Utf8Json.Formatters

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
type IdHandler = int*HttpFunc*HttpContext -> HttpFuncResult
 
let inline serializeObject obj =
    JsonSerializer.Serialize(obj)

let inline deserializeObjectUnsafe<'a> (str: string) =
    JsonSerializer.Deserialize<'a>(str)

let inline deserializeObject<'a> (str: string) =
    try
        Some <| JsonSerializer.Deserialize<'a>(str)       
    with
    | exn ->
        None

let inline jsonCustom str (next : HttpFunc) (httpContext: HttpContext) =
    setHttpHeader "Content-Type" "application/json" >=> setBodyAsString str <| next <| httpContext

let inline jsonBuffer (bytes: byte[]) =
    fun (next : HttpFunc) (ctx: HttpContext) -> 
    ctx.Response.Headers.["Content-Type"] <- jsonStringValues
    ctx.Response.Headers.ContentLength <- Nullable<int64>(bytes.LongLength)
    task {            
        do! ctx.Response.Body.WriteAsync(bytes, 0, bytes.Length)
        do! ctx.Response.Body.FlushAsync()
        return! next ctx
    }   

let inline checkStringFromRequest (stringValue: string) = 
    stringValue.Contains(": null") |> not

let getUser(id, next, ctx) = 
    if (id > users.Length)
    then setStatusCode 404 next ctx
        else
            let user = users.[id]
            match box user with       
            | null -> setStatusCode 404 next ctx
            | _ -> jsonBuffer (serializeObject user) next ctx

let getVisit(id, next, ctx) = 
    if (id > visits.Length)
    then setStatusCode 404 next ctx
        else
            let visit = visits.[id]
            match box visit with     
            | null -> setStatusCode 404 next ctx
            | _ -> jsonBuffer (serializeObject visit) next ctx

let getLocation(id, next, ctx) = 
    if (id > locations.Length)
    then setStatusCode 404 next ctx
        else
            let location = locations.[id]
            match box location with      
            | null -> setStatusCode 404 next ctx
            | _ -> jsonBuffer (serializeObject location) next ctx

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
        | None -> false
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
        | None -> false
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
        | None -> false
    else
        false

let updateVisit (id: int, next : HttpFunc, httpContext: HttpContext) =
    if (id > visits.Length)
    then setStatusCode 404 next httpContext
    else
        let oldEntity = visits.[id]
        match box oldEntity with    
        | null -> 
            setStatusCode 404 next httpContext
        | _ -> 
            task {
                let! json = httpContext.ReadBodyFromRequest()
                if updateVisitInternal oldEntity json
                then return! setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| next <| httpContext 
                else return! setStatusCode 400 next httpContext
            }

let updateUser (id: int, next : HttpFunc, httpContext: HttpContext) =
    if (id > users.Length)
    then setStatusCode 404 next httpContext
    else
        let oldEntity = users.[id]
        match box oldEntity with    
        | null -> 
            setStatusCode 404 next httpContext
        | _ -> 
            task {
                let! json = httpContext.ReadBodyFromRequest()
                if updateUserInternal oldEntity json
                then return! setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| next <| httpContext 
                else return! setStatusCode 400 next httpContext
            }

let updateLocation (id: int, next : HttpFunc, httpContext: HttpContext) =
    if (id > locations.Length)
    then setStatusCode 404 next httpContext
    else
        let oldEntity = locations.[id]
        match box oldEntity with    
        | null -> 
            setStatusCode 404 next httpContext
        | _ -> 
            task {
                let! json = httpContext.ReadBodyFromRequest()
                if updateLocationInternal oldEntity json 
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
                                    if location.id = 7772 then Console.WriteLine("{0} {1}",stringValue,location.country)                    
                                    locations.[location.id] <- location
                                    locationVisits.[location.id] <- VisitsCollection()
                                    setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| next <| httpContext
                            | _ -> setStatusCode 400 next httpContext
    | None -> setStatusCode 400 next httpContext

let addLocation (next : HttpFunc, httpContext: HttpContext) = 
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
              VisitActor.AddLocationVisit visit.location locationVisits.[visit.location] visit.id                         
              VisitActor.AddUserVisit visit.user userVisits.[visit.user] visit.id visit.visited_at
              setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| next <| httpContext
        | _ -> setStatusCode 400 next httpContext
    | None -> setStatusCode 400 next httpContext

let addVisit (next : HttpFunc, httpContext: HttpContext) = 
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
                                       userVisits.[user.id] <- VisitsCollectionSorted()
                                       setHttpHeader "Content-Type" "application/json" >=> setBodyAsString "{}" <| next <| httpContext
                             | _ -> setStatusCode 400 next httpContext
    | None -> setStatusCode 400 next httpContext

let addUser (next : HttpFunc, httpContext: HttpContext) = 
    task {
        let! stringValue = httpContext.ReadBodyFromRequest()
        return! addUserInternal stringValue next httpContext
    }

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
    // checkParseResult query.fromDate (fun fromDate -> visit.visited_at > fromDate)
    //     && (checkParseResult query.toDate (fun toDate -> visit.visited_at < toDate))
    //     && (checkParseResult query.toDistance (fun toDistance -> location.distance < toDistance))
    //     && (String.IsNullOrEmpty(query.country) || location.country = query.country)

let getUserVisits (userId, next : HttpFunc, ctx: HttpContext) = 
    if (userId > users.Length)
    then setStatusCode 404 next ctx
    else
        let user = users.[userId]
        match box user with      
        | null ->
            setStatusCode 404 next ctx
        | _ ->
            match getUserVisitsQuery ctx with
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
                jsonBuffer (serializeObject { visits = usersVisits }) next ctx
            | Non -> setStatusCode 400 next ctx  

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

    // checkParseResult query.fromDate (fun fromDate -> visit.visited_at > fromDate)
    //     && (checkParseResult query.toDate (fun toDate -> visit.visited_at < toDate))
    //     && (checkParseResult query.gender (fun gender -> user.gender = gender))
    //     && (checkParseResult query.toAge (fun toAge -> (diffYears (user.birth_date |> convertToDate) currentDate ) <  toAge))
    //     && (checkParseResult query.fromAge (fun fromAge -> (diffYears (user.birth_date |> convertToDate) currentDate ) >= fromAge))



let getAvgMark (locationId, next : HttpFunc, ctx: HttpContext) = 
    if (locationId > locations.Length)
    then setStatusCode 404 next ctx
    else
        let location = locations.[locationId]
        match box location with    
        | null ->
            setStatusCode 404 <| next <| ctx
        | _ ->
            match getAvgMarkQuery ctx with
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
                jsonBuffer (serializeObject { avg = avg }) next ctx
            | Non -> setStatusCode 400 next ctx    

let private usersPathString = PathString("/users")
let private visitsPathString = PathString("/visits")
let private locationsPathString = PathString("/locations")



let inline private tryParseId stringId (f: IdHandler) next ctx =
   let id = ref 0
   if Int32.TryParse(stringId, id)
   then f(id.Value, next, ctx)
   else setStatusCode 404 next ctx

let customGetRoutef : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let remaining = ref PathString.Empty
        let id = ref 0
        match ctx.Request.Path with
        | visitPath when (visitPath.StartsWithSegments(visitsPathString, remaining)) ->
             if Int32.TryParse(remaining.Value.Value.Substring(1), id)
             then getVisit(id.Value, next, ctx)
             else setStatusCode 404 next ctx
        | userPath when (userPath.StartsWithSegments(usersPathString, remaining)) -> 
            let pathString = remaining.Value.Value;
            let slashIndex = pathString.IndexOf("/visits", StringComparison.Ordinal)
            if (slashIndex > -1)
            then
                if Int32.TryParse(pathString.Substring(1,slashIndex-1), id)
                then getUserVisits(id.Value, next, ctx)
                else setStatusCode 404 next ctx
            else 
                if Int32.TryParse(pathString.Substring(1), id)
                then getUser(id.Value, next, ctx)
                else setStatusCode 404 next ctx
        | locationPath when (locationPath.StartsWithSegments(locationsPathString, remaining)) ->          
            let pathString = remaining.Value.Value;
            let slashIndex = pathString.IndexOf("/avg", StringComparison.Ordinal)
            if (slashIndex > -1)
            then
                if Int32.TryParse(pathString.Substring(1,slashIndex-1), id)
                then getAvgMark(id.Value, next, ctx)
                else setStatusCode 404 next ctx
            else
                if Int32.TryParse(pathString.Substring(1), id)
                then getLocation(id.Value, next, ctx)
                else setStatusCode 404 next ctx
        | _-> shortCircuit

let inline private getPostRoute (newRoute: IdHandler) (updateRoute: IdHandler) (remaining: PathString ref) next ctx =
    let pathString = remaining.Value.Value
    if pathString.Equals("/new",StringComparison.Ordinal)
    then newRoute (0, next, ctx)
    else tryParseId (pathString.Substring(1)) updateRoute next ctx

let customPostRoutef : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let remaining = ref PathString.Empty
        match ctx.Request.Path with
        | visitPath when (visitPath.StartsWithSegments(visitsPathString, remaining)) ->
            let pathString = remaining.Value.Value
            if pathString.Equals("/new",StringComparison.Ordinal)
            then addVisit(next, ctx)
            else 
                let id = ref 0
                if Int32.TryParse(pathString.Substring(1), id)
                then updateVisit(id.Value, next, ctx)
                else setStatusCode 404 next ctx
        | userPath when (userPath.StartsWithSegments(usersPathString, remaining)) -> 
            let pathString = remaining.Value.Value
            if pathString.Equals("/new",StringComparison.Ordinal)
            then addUser(next, ctx)
            else 
                let id = ref 0
                if Int32.TryParse(pathString.Substring(1), id)
                then updateUser(id.Value, next, ctx)
                else setStatusCode 404 next ctx
        | locationPath when (locationPath.StartsWithSegments(locationsPathString, remaining)) ->    
            let pathString = remaining.Value.Value
            if pathString.Equals("/new",StringComparison.Ordinal)
            then addLocation(next, ctx)
            else 
                let id = ref 0
                if Int32.TryParse(pathString.Substring(1), id)
                then updateLocation(id.Value, next, ctx)
                else setStatusCode 404 next ctx
        | _-> shortCircuit




let webApp = 
    choose [
        GET >=> customGetRoutef
        POST >=> customPostRoutef
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
                    users.[user.id] <-user
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
        // .UseUrls("http://0.0.0.0:80")
        .Configure(Action<IApplicationBuilder> configureApp)
        .Build()
        .Run()
    0