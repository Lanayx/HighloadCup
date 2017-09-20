module HCup.Serializers

open System.Text
open HCup.Models

let inline serializeLocation (loc: Location) =
    StringBuilder()
        .Append("{\"id\":").Append(loc.id)
        .Append(",\"distance\":").Append(loc.distance)
        .Append(",\"place\":\"").Append(loc.place)
        .Append("\",\"city\":\"").Append(loc.city)
        .Append("\",\"country\":\"").Append(loc.country)
        .Append("\"}").ToString()

let inline serializeUser (user: User) =
    StringBuilder()
        .Append("{\"id\":").Append(user.id)
        .Append(",\"birth_date\":").Append(user.birth_date)
        .Append(",\"first_name\":\"").Append(user.first_name)
        .Append("\",\"last_name\":\"").Append(user.last_name)
        .Append("\",\"gender\":\"").Append(user.gender)
        .Append("\",\"email\":\"").Append(user.email)
        .Append("\"}").ToString()

let inline serializeVisit (visit: Visit) =
    StringBuilder()
        .Append("{\"id\":").Append(visit.id)
        .Append(",\"location\":").Append(visit.location)
        .Append(",\"mark\":").Append(visit.mark)
        .Append(",\"user\":").Append(visit.user)
        .Append(",\"visited_at\":").Append(visit.visited_at)
        .Append("}").ToString()

let inline serializeVisits (visits: seq<UserVisit>) =
    let sb = StringBuilder().Append("{\"visits\":[")
    let mutable start = true
    for visit in visits do
        (if start
        then
            start <- false 
            sb.Append("{\"mark\":")
        else 
            sb.Append(",{\"mark\":"))
          .Append(visit.mark)
          .Append(",\"visited_at\":").Append(visit.visited_at)
          .Append(",\"place\":\"").Append(visit.place)
          .Append("\"}") |> ignore
    sb.Append("]}").ToString()

let inline serializeAvg (avg: float) =
    StringBuilder()
        .Append("{\"avg\":").Append(avg.ToString(System.Globalization.CultureInfo.InvariantCulture))
        .Append("}").ToString()