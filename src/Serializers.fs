module HCup.Serializers

open System
open System.IO
open System.Globalization
open Microsoft.AspNetCore.Http
open Newtonsoft.Json
open HCup.Models
open HCup.Binder
open Giraffe.Tasks
open Giraffe.HttpHandlers

let serializeVisit (visit: Visit) : HttpHandler =
    fun (next : HttpFunc) (httpContext : HttpContext) ->
        use sw = new StreamWriter(httpContext.Response.Body)
        use writer = new JsonTextWriter(sw)
        writer.WriteStartObject()    
        writer.WritePropertyName("id")
        writer.WriteValue(visit.id) 
        writer.WritePropertyName("user")
        writer.WriteValue(visit.user) 
        writer.WritePropertyName("location")
        writer.WriteValue(visit.location) 
        writer.WritePropertyName("visited_at")
        writer.WriteValue(visit.visited_at) 
        writer.WritePropertyName("mark")
        writer.WriteValue(visit.mark)
        writer.WriteEndObject()
        task {
            do! writer.FlushAsync()
            return! next httpContext
        }

let deserializeVisit (json: string) =
    let visit = Visit()  
    let mutable success = true
    use sr = new StringReader(json)
    use reader = new JsonTextReader(sr)
    while(reader.Read() && success) do      
        match string reader.Value with
        | "id" ->
             match toParseResult Int32.TryParse (reader.ReadAsString()) with
             | Success id -> visit.id <- id
             | _ -> success <- false
        | "user" ->
             match toParseResult Int32.TryParse (reader.ReadAsString()) with
             | Success user -> visit.user <- user
             | _ -> success <- false
        | "visited_at" -> 
             match toParseResult UInt32.TryParse (reader.ReadAsString()) with
             | Success visited_at -> visit.visited_at <- visited_at
             | _ -> success <- false
        | "location" ->
             match toParseResult Int32.TryParse (reader.ReadAsString()) with
             | Success location -> visit.location <- location
             | _ -> success <- false
        | "mark" -> 
             match toParseResult Double.TryParse (reader.ReadAsString()) with
             | Success mark -> visit.mark <- mark
             | _ -> success <- false
        | _ -> ()
    if success && visit.id<>0 && visit.location<>0 && visit.mark<>0.0 && visit.user<>0 && visit.visited_at <>0ul
    then Some visit 
    else 
        reader.Close()
        None

let deserializeVisitUpd (json: string) =
    let visit = VisitUpd()  
    let mutable success = true
    use sr = new StringReader(json)
    use reader = new JsonTextReader(sr)
    while(reader.Read() && success) do     
        match string reader.Value with
        | "user" ->
             match toParseResult Int32.TryParse (reader.ReadAsString()) with
             | Success user -> visit.user <- Nullable user
             | _ -> success <- false

        | "visited_at" -> 
             match toParseResult UInt32.TryParse (reader.ReadAsString()) with
             | Success visited_at -> visit.visited_at <- Nullable visited_at
             | _ -> success <- false
        | "location" ->
             match toParseResult Int32.TryParse (reader.ReadAsString()) with
             | Success location -> visit.location <- Nullable location
             | _ -> success <- false
        | "mark" -> 
             match toParseResult Double.TryParse (reader.ReadAsString()) with
             | Success mark -> visit.mark <- Nullable mark
             | _ -> success <- false
        | _ -> ()
    if success
    then Some visit 
    else 
        reader.Close()
        None

let serializeUser (user: User) : HttpHandler =
    fun (next : HttpFunc) (httpContext : HttpContext) ->
        use sw = new StreamWriter(httpContext.Response.Body)
        use writer = new JsonTextWriter(sw)
        writer.WriteStartObject()    
        writer.WritePropertyName("id");
        writer.WriteValue(user.id) 
        writer.WritePropertyName("first_name")
        writer.WriteValue(user.first_name) 
        writer.WritePropertyName("last_name")
        writer.WriteValue(user.last_name) 
        writer.WritePropertyName("birth_date")
        writer.WriteValue(user.birth_date) 
        writer.WritePropertyName("gender")
        writer.WriteValue(user.gender.ToString())
        writer.WritePropertyName("email")
        writer.WriteValue(user.email)
        writer.WriteEndObject()
        task {
            do! writer.FlushAsync()
            return! next httpContext
        }

let rec readUser (reader: JsonTextReader) (user: User)  = task {
        let! readerAvailable = reader.ReadAsync()
        if (readerAvailable)
        then
            match string reader.Value with
            | "id" ->
                 match toParseResult Int32.TryParse (reader.ReadAsString()) with
                 | Success id -> 
                    user.id <- id
                    return! readUser reader user
                 | _ -> return false
            | "first_name" ->
                 user.first_name <- reader.ReadAsString()
                 return! readUser reader user
            | "last_name" -> 
                 user.last_name <- reader.ReadAsString()
                 return! readUser reader user
            | "birth_date" ->
                 match toParseResult Double.TryParse (reader.ReadAsString()) with
                 | Success birth_date ->
                    user.birth_date <- birth_date
                    return! readUser reader user
                 | _ -> return false
            | "gender" -> 
                 match toParseResult Sex.TryParse (reader.ReadAsString()) with
                 | Success gender -> 
                    user.gender <- gender
                    return! readUser reader user
                 | _ -> return false
            | "email" -> 
                 user.email <- reader.ReadAsString()
                 return! readUser reader user
            | _ -> return! readUser reader user            
        else
            return true        
    }

let deserializeUser (body: Stream) = task {
    let user = User()  
    use sr = new StreamReader(body)
    use reader = new JsonTextReader(sr)
    let! success = readUser reader user
    if success && user.id <> 0 && user.first_name |> isNull |> not && user.last_name |> isNull |> not && 
        user.birth_date<>0.0 && user.gender<> Sex.undef && user.email  |> isNull |> not
    then 
        return Some user 
    else 
        reader.Close()
        return None
}

let deserializeUserUpd (json: string) =
    let user = UserUpd()  
    let mutable success = true
    use sr = new StringReader(json)
    use reader = new JsonTextReader(sr)
    while(reader.Read() && success) do          
        match string reader.Value with
        | "first_name" ->
             user.first_name <- reader.ReadAsString()
             if isNull user.first_name
             then success <- false
        | "last_name" -> 
             user.last_name <- reader.ReadAsString()
             if isNull user.last_name
             then success <- false
        | "birth_date" ->
             match toParseResult Double.TryParse (reader.ReadAsString()) with
             | Success birth_date -> user.birth_date <- Nullable birth_date
             | _ -> success <- false
        | "gender" -> 
             match toParseResult Sex.TryParse (reader.ReadAsString()) with
             | Success gender -> user.gender <- Nullable gender
             | _ -> success <- false
        | "email" -> 
             user.email <- reader.ReadAsString()
             if isNull user.email
             then success <- false
        | _ -> ()
    if success
    then Some user 
    else 
        reader.Close()
        None

let serializeLocation (location: Location) : HttpHandler =
    fun (next : HttpFunc) (httpContext : HttpContext) ->
        use sw = new StreamWriter(httpContext.Response.Body)
        use writer = new JsonTextWriter(sw)
        writer.WriteStartObject()    
        writer.WritePropertyName("id");
        writer.WriteValue(location.id) 
        writer.WritePropertyName("distance")
        writer.WriteValue(location.distance) 
        writer.WritePropertyName("city")
        writer.WriteValue(location.city) 
        writer.WritePropertyName("place")
        writer.WriteValue(location.place) 
        writer.WritePropertyName("country")
        writer.WriteValue(location.country)
        writer.WriteEndObject()
        task {
            do! writer.FlushAsync()
            return! next httpContext
        }

let deserializeLocation (json: string) =
    let location = Location()  
    let mutable success = true
    use sr = new StringReader(json)
    use reader = new JsonTextReader(sr)
    while(reader.Read() && success) do  
        match string reader.Value with
        | "id" ->
             match toParseResult Int32.TryParse (reader.ReadAsString()) with
             | Success id -> location.id <- id
             | _ -> success <- false
        | "distance" ->
             match toParseResult UInt16.TryParse (reader.ReadAsString()) with
             | Success distance -> location.distance <- distance
             | _ -> success <- false
        | "city" -> 
             location.city <- reader.ReadAsString()
        | "place" -> 
             location.place <- reader.ReadAsString()
        | "country" -> 
             location.country <- reader.ReadAsString()
        | _ -> ()
    if success && location.id <> 0 && location.distance<>0us && location.city |> isNull |> not && 
        location.place |> isNull |> not && location.country |> isNull |> not
    then Some location 
    else 
        reader.Close()
        None

let deserializeLocationUpd (json: string) =
    let location = LocationUpd()  
    let mutable success = true
    use sr = new StringReader(json)
    use reader = new JsonTextReader(sr)
    while(reader.Read() && success) do  
        match string reader.Value with
        | "distance" ->
             match toParseResult UInt16.TryParse (reader.ReadAsString()) with
             | Success distance -> location.distance <- Nullable distance
             | _ -> success <- false
        | "city" -> 
             location.city <- reader.ReadAsString()
             if isNull location.city
             then success <- false
        | "place" -> 
             location.place <- reader.ReadAsString()
             if isNull location.place
             then success <- false
        | "country" -> 
             location.country <- reader.ReadAsString()
             if isNull location.country
             then success <- false
        | _ -> ()
    if success
    then Some location 
    else 
        reader.Close()
        None

let serializeAverage (average: Average) : HttpHandler =
    fun (next : HttpFunc) (httpContext : HttpContext) ->
        use sw = new StreamWriter(httpContext.Response.Body)
        use writer = new JsonTextWriter(sw)
        writer.WriteStartObject()    
        writer.WritePropertyName("avg")
        writer.WriteValue(average.avg)
        writer.WriteEndObject()
        task {
            do! writer.FlushAsync()
            return! next httpContext
        }

let serializeUserVisits (userVisits: UserVisits) : HttpHandler =
    fun (next : HttpFunc) (httpContext : HttpContext) ->
        use sw = new StreamWriter(httpContext.Response.Body)
        use writer = new JsonTextWriter(sw)
        writer.WriteStartObject()    
        writer.WritePropertyName("visits");
        writer.WriteStartArray();
        for userVisit in userVisits.visits do
            writer.WriteStartObject()
            writer.WritePropertyName("mark")
            writer.WriteValue(userVisit.mark)
            writer.WritePropertyName("visited_at")
            writer.WriteValue(userVisit.visited_at)
            writer.WritePropertyName("place")
            writer.WriteValue(userVisit.place)
            writer.WriteEndObject()    
        writer.WriteEndArray()
        writer.WriteEndObject()
        task {
            do! writer.FlushAsync()
            return! next httpContext
        }
