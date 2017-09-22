module HCup.Serializers

open System.Text
open HCup.Models
open HCup

let inline serializeLocation (loc: Location) =
    let sb = StringBuilderCache.Acquire(SbSize.Small)
                    .Append("{\"id\":").Append(loc.id.ToString(System.Globalization.CultureInfo.InvariantCulture))
                    .Append(",\"distance\":").Append(loc.distance.ToString(System.Globalization.CultureInfo.InvariantCulture))
                    .Append(",\"place\":\"").Append(loc.place)
                    .Append("\",\"city\":\"").Append(loc.city)
                    .Append("\",\"country\":\"").Append(loc.country)
                    .Append("\"}")
    StringBuilderCache.GetStringAndRelease sb SbSize.Small

let inline serializeUser (user: User) =
    let sb = StringBuilderCache.Acquire(SbSize.Small)
                    .Append("{\"id\":").Append(user.id.ToString(System.Globalization.CultureInfo.InvariantCulture))
                    .Append(",\"birth_date\":").Append(user.birth_date.ToString(System.Globalization.CultureInfo.InvariantCulture))
                    .Append(",\"first_name\":\"").Append(user.first_name)
                    .Append("\",\"last_name\":\"").Append(user.last_name)
                    .Append("\",\"gender\":\"").Append(user.gender.ToString())
                    .Append("\",\"email\":\"").Append(user.email)
                    .Append("\"}")
    StringBuilderCache.GetStringAndRelease sb SbSize.Small

let inline serializeVisit (visit: Visit) =
    let sb = StringBuilderCache.Acquire(SbSize.Small)
                    .Append("{\"id\":").Append(visit.id.ToString(System.Globalization.CultureInfo.InvariantCulture))
                    .Append(",\"location\":").Append(visit.location.ToString(System.Globalization.CultureInfo.InvariantCulture))
                    .Append(",\"mark\":").Append(visit.mark.ToString(System.Globalization.CultureInfo.InvariantCulture))
                    .Append(",\"user\":").Append(visit.user.ToString(System.Globalization.CultureInfo.InvariantCulture))
                    .Append(",\"visited_at\":").Append(visit.visited_at.ToString(System.Globalization.CultureInfo.InvariantCulture))
                    .Append("}")
    StringBuilderCache.GetStringAndRelease sb SbSize.Small

let inline serializeVisits (visits: seq<UserVisit>) =
    let sb = StringBuilderCache.Acquire(SbSize.Big).Append("{\"visits\":[")
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
    StringBuilderCache.GetStringAndRelease (sb.Append("]}")) SbSize.Big

let inline serializeAvg (avg: float) =
    let sb = StringBuilderCache.Acquire(SbSize.Small)
                .Append("{\"avg\":").Append(avg.ToString(System.Globalization.CultureInfo.InvariantCulture))
                .Append("}")
    StringBuilderCache.GetStringAndRelease sb SbSize.Small  