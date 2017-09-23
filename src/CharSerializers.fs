module HCup.CharSerializers

open System.Text
open HCup.Models
open HCup

let rec private appendInt (value: int) fn (sb: StringBuilder) =
    match value with
    | 0 -> fn(); sb
    | number -> appendInt (value / 10) (fun () -> sb.Append(value % 10) |> ignore; fn()) sb

let rec private appendUint8 (value: uint8) fn (sb: StringBuilder) =
    match value with
    | 0uy -> fn(); sb
    | number -> appendUint8 (value / 10uy) (fun () -> sb.Append(value % 10uy) |> ignore; fn()) sb

let rec private appendInt64 (value: int64) fn (sb: StringBuilder) =
    match value with
    | 0L -> fn(); sb
    | number -> appendInt64 (value / 10L) (fun () -> sb.Append(value % 10L) |> ignore; fn()) sb

let rec private appendUint32 (value: uint32) fn (sb: StringBuilder) =
    match value with
    | 0u -> fn(); sb
    | number -> appendUint32 (value / 10u) (fun () -> sb.Append(value % 10u) |> ignore; fn()) sb


let private appendFloat5 (value: float) (sb: StringBuilder) =
     if value = 0.0
     then sb.Append("0.0")
     else
         let intValue = (int)value
         appendInt intValue ignore sb |> ignore
         sb.Append('.') |> ignore
         let decimalValue = (int)(value*100000.0) - intValue*100000
         let mutable temp = decimalValue
         while temp > 0 && temp < 10000 do
             temp <- temp*10
             sb.Append('0') |> ignore         
         sb |> appendInt decimalValue ignore


type StringBuilder with
    member this.CustomAppendInt(intVal: int) =
        if intVal < 0 
        then 
            this.Append('-') |> ignore
            appendInt (-intVal) ignore this
        else
            appendInt intVal ignore this

    member this.CustomAppendUint8(uint8Val: uint8) =
        appendUint8 uint8Val ignore this
    
    member this.CustomAppendInt64(int64Val: int64) =
        if int64Val < 0L
        then 
            this.Append('-') |> ignore
            appendInt64 (-int64Val) ignore this
        else
            appendInt64 int64Val ignore this
    
    member this.CustomAppendUint32(uint32Val: uint32) =
        appendUint32 uint32Val ignore this

    member this.CustomAppendFloat(floatVal: float) =
        appendFloat5 floatVal this

let serializeLocation (loc: Location)  =
        let sb = StringBuilderCache.Acquire()
                        .Append("{\"id\":").CustomAppendInt(loc.id)
                        .Append(",\"distance\":").CustomAppendUint8(loc.distance)
                        .Append(",\"place\":\"").Append(loc.place)
                        .Append("\",\"city\":\"").Append(loc.city)
                        .Append("\",\"country\":\"").Append(loc.country)
                        .Append("\"}")
        StringBuilderCache.GetStringAndRelease sb

let serializeUser (user: User) =
        let sb = StringBuilderCache.Acquire()
                        .Append("{\"id\":").CustomAppendInt(user.id)
                        .Append(",\"birth_date\":").CustomAppendInt64(user.birth_date)
                        .Append(",\"first_name\":\"").Append(user.first_name)
                        .Append("\",\"last_name\":\"").Append(user.last_name)
                        .Append("\",\"gender\":\"").Append(user.gender)
                        .Append("\",\"email\":\"").Append(user.email)
                        .Append("\"}")
        StringBuilderCache.GetStringAndRelease sb

let serializeVisit (visit: Visit) =
        let sb = StringBuilderCache.Acquire()
                        .Append("{\"id\":").CustomAppendInt(visit.id)
                        .Append(",\"location\":").CustomAppendInt(visit.location)
                        .Append(",\"mark\":").CustomAppendUint8(visit.mark)
                        .Append(",\"user\":").CustomAppendInt(visit.user)
                        .Append(",\"visited_at\":").CustomAppendUint32(visit.visited_at)
                        .Append("}")
        StringBuilderCache.GetStringAndRelease sb

let serializeVisits (visits: seq<UserVisit>) =
        let sb = StringBuilderCache.Acquire().Append("{\"visits\":[")
        let mutable start = true
        for visit in visits do
            (if start
            then
                start <- false 
                sb.Append("{\"mark\":")
            else 
                sb.Append(",{\"mark\":"))
              .CustomAppendUint8(visit.mark)
              .Append(",\"visited_at\":").CustomAppendUint32(visit.visited_at)
              .Append(",\"place\":\"").Append(visit.place)
              .Append("\"}") |> ignore
        StringBuilderCache.GetStringAndRelease (sb.Append("]}"))

let serializeAvg (avg: float) =
        let sb = StringBuilderCache.Acquire()
                    .Append("{\"avg\":").CustomAppendFloat(avg)
                    .Append("}")
        StringBuilderCache.GetStringAndRelease sb 