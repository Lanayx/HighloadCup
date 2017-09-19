module HCup.Serializers

open System.Text
open HCup.Models

let inline serializeLocation (loc: Location) =
    let str = StringBuilder()
    str.Append("{\"id\":").Append(loc.id)
       .Append(",\"distance\":").Append(loc.distance)
       .Append(",\"place\":\"").Append(loc.place)
       .Append("\",\"city\":\"").Append(loc.city)
       .Append("\",\"country\":\"").Append(loc.country)
       .Append("\"}").ToString()

let inline serializeUser (user: User) =
    let str = StringBuilder()
    str.Append("{\"id\":").Append(user.id)
       .Append(",\"birth_date\":").Append(user.birth_date)
       .Append(",\"first_name\":\"").Append(user.first_name)
       .Append("\",\"last_name\":\"").Append(user.last_name)
       .Append("\",\"gender\":\"").Append(user.gender)
       .Append("\",\"email\":\"").Append(user.email)
       .Append("\"}").ToString()

let inline serializeVisit (visit: Visit) =
    let str = StringBuilder()
    str.Append("{\"id\":").Append(visit.id)
       .Append(",\"location\":").Append(visit.location)
       .Append(",\"mark\":").Append(visit.mark)
       .Append(",\"user\":").Append(visit.user)
       .Append(",\"visited_at\":").Append(visit.visited_at)
       .Append("}").ToString()

