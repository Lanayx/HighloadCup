module HCup.BufferSerializers

open System
open System.Buffers
open System.Globalization
open System.IO
open System.Text

open HCup.Models
open HCup

let private smallBuffer() = ArrayPool.Shared.Rent 400
let private bigBuffer() = ArrayPool.Shared.Rent 6000

let private utf8Encoding = Encoding.UTF8
let utf8 : string -> byte[] = utf8Encoding.GetBytes
let private writeArray (output : MemoryStream) array = output.Write(array, 0, array.Length)
let private stream buffer = new MemoryStream(buffer, 0, buffer.Length, true, true)

let private ``{"id":`` = utf8 "{\"id\":"
let private ``,"distance":`` = utf8 ",\"distance\":"
let private ``,"place":"`` = utf8 ",\"place\":\""
let private ``","city":"`` = utf8 "\",\"city\":\""
let private ``","country":"`` = utf8 "\",\"country\":\""
let private ``"}`` = utf8 "\"}"

let serializeLocation (loc: Location) : MemoryStream =
    let array = smallBuffer()
    let output = stream array
    let write = writeArray output
    write ``{"id":``
    write(utf8(string loc.id))
    write ``,"distance":``
    write(utf8(string loc.distance))
    write ``,"place":"``
    write loc.place
    write ``","city":"``
    write loc.city
    write ``","country":"``
    write(utf8 loc.country)
    write ``"}``
    output

let private ``,"birth_date":`` = utf8 ",\"birth_date\":"
let private ``,"first_name":"`` = utf8 ",\"first_name\":\""
let private ``","last_name":"`` = utf8 "\",\"last_name\":\""
let private ``","gender":"`` = utf8 "\",\"gender\":\""
let private ``","email":"`` = utf8 "\",\"email\":\""

let serializeUser (user: User) : MemoryStream =
    let array = smallBuffer()
    let output = stream array
    let write = writeArray output
    write ``{"id":``
    write(utf8(string user.id))
    write ``,"birth_date":``
    write(utf8(string user.birth_date))
    write ``,"first_name":"``
    write user.first_name
    write ``","last_name":"``
    write user.last_name
    write ``","gender":"``
    write(utf8 user.gender)
    write ``","email":"``
    write user.email
    write ``"}``
    output

let private ``,"location":`` = utf8 ",\"location\":"
let private ``,"mark":`` = utf8 ",\"mark\":"
let private ``,"user":`` = utf8 ",\"user\":"
let private ``,"visited_at":`` = utf8 ",\"visited_at\":"
let private ``}`` = utf8 "}"

let serializeVisit (visit: Visit) : MemoryStream =
    let array = smallBuffer()
    let output = stream array
    let write = writeArray output
    write ``{"id":``
    write(utf8(string visit.id))
    write ``,"location":``
    write(utf8(string visit.location))
    write ``,"mark":``
    write(utf8(string visit.mark))
    write ``,"user":``
    write(utf8(string visit.user))
    write ``,"visited_at":``
    write(utf8(string visit.visited_at))
    write ``}``
    output

let private ``{"visits":[`` = utf8 "{\"visits\":["
let private ``{"mark":`` = utf8 "{\"mark\":"
let private ``,{"mark":`` = utf8 ",{\"mark\":"
let private ``]}`` = utf8 "]}"

let serializeVisits (visits: UserVisit seq) : MemoryStream =
    let array = bigBuffer()
    let output = stream array
    let write = writeArray output
    write ``{"visits":[``
    let mutable start = true
    for visit in visits do
        if start
        then
            start <- false
            write ``{"mark":``
        else
            write ``,{"mark":``
        write (utf8(string visit.mark))
        write ``,"visited_at":``
        write(utf8(string visit.visited_at))
        write ``,"place":"``
        write visit.place
        write ``"}``
    write ``]}``
    output

let private ``{"avg":`` = utf8 "{\"avg\":"

let serializeAvg (avg: float) : MemoryStream =
    let array = smallBuffer()
    let output = stream array
    let write = writeArray output
    write ``{"avg":``
    write(utf8(string avg))
    write ``}``
    output