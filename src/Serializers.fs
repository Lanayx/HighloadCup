module HCup.Serializers

open System
open System.IO
open System.Globalization
open Newtonsoft.Json
open HCup.Models
open HCup.Binder

let serializeVisit (visit: Visit) =
    use sw = new StringWriter()
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
    sw.ToString()

let deserializeVisit (json: string) =
    let visit = Visit()  
    let mutable success = true
    use sr = new StringReader(json)
    use reader = new JsonTextReader(sr)
    while(reader.Read() && success) do            
        Console.WriteLine("Reader: {0} {1}", reader.ValueType, reader.Value)
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

let serializeUser (user: User) =
    use sw = new StringWriter()
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
    sw.ToString()

let deserializeUser (json: string) =
    let user = User()  
    let mutable success = true
    use sr = new StringReader(json)
    use reader = new JsonTextReader(sr)
    while(reader.Read() && success) do            
        Console.WriteLine("Reader: {0} {1}", reader.ValueType, reader.Value)
        match string reader.Value with
        | "id" ->
             match toParseResult Int32.TryParse (reader.ReadAsString()) with
             | Success id -> user.id <- id
             | _ -> success <- false
        | "first_name" ->
             user.first_name <- reader.ReadAsString()
        | "last_name" -> 
             user.last_name <- reader.ReadAsString()
        | "birth_date" ->
             match toParseResult Double.TryParse (reader.ReadAsString()) with
             | Success birth_date -> user.birth_date <- birth_date
             | _ -> success <- false
        | "gender" -> 
             match toParseResult Sex.TryParse (reader.ReadAsString()) with
             | Success gender -> user.gender <- gender
             | _ -> success <- false
        | "email" -> 
             user.email <- reader.ReadAsString()
        | _ -> ()
    if success && user.id <> 0 && user.first_name |> isNull |> not && user.last_name |> isNull |> not && 
        user.birth_date<>0.0 && user.gender<> Sex.undef && user.email  |> isNull |> not
    then Some user 
    else 
        reader.Close()
        None

let serializeLocation (location: Location) =
    use sw = new StringWriter()
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
    sw.ToString()

let serializeAverage (average: Average) =
    use sw = new StringWriter()
    use writer = new JsonTextWriter(sw)
    writer.WriteStartObject()    
    writer.WritePropertyName("avg")
    writer.WriteValue(average.avg)
    writer.WriteEndObject()
    sw.ToString()

let serializeUserVisits (userVisits: UserVisits) =
    use sw = new StringWriter()
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
    sw.ToString()

