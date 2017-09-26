module HCup.BufferSerializers

open System
open System.Buffers
open System.Globalization
open System.IO
open System.Text

open FSharp.NativeInterop

open HCup.Models
open HCup

#nowarn "9"

let private smallBuffer() = ArrayPool.Shared.Rent 400
let private bigBuffer() = ArrayPool.Shared.Rent 6000

let private utf8Encoding = Encoding.UTF8
let utf8 : string -> byte[] = utf8Encoding.GetBytes
let private writeArray (output : MemoryStream) array = output.Write(array, 0, array.Length)
let private stream buffer = new MemoryStream(buffer, 0, buffer.Length, true, true)

let private writeInt32 (output : MemoryStream) (number: int) =
    let numbersCount =
        if number >= 1_000_000_000 then 10
        elif number >= 100_000_000 then 9
        elif number >= 10_000_000 then 8
        elif number >= 1_000_000 then 7
        elif number >= 100_000 then 6
        elif number >= 10_000 then 5
        elif number >= 1_000 then 4
        elif number >= 100 then 3
        elif number >= 10 then 2
        else 1
    let buffer = NativePtr.stackalloc<byte> numbersCount
    let mutable num = number
    let loopMax = numbersCount - 1
    for i = loopMax downto 0 do
        NativePtr.set buffer i (byte (num % 10 + 48))
        num <- num / 10
    for i = 0 to loopMax do
        output.WriteByte (NativePtr.get buffer i)

let private writeUInt32 (output : MemoryStream) (number: uint32) =
    let numbersCount =
        if number >= 1_000_000_000u then 10
        elif number >= 100_000_000u then 9
        elif number >= 10_000_000u then 8
        elif number >= 1_000_000u then 7
        elif number >= 100_000u then 6
        elif number >= 10_000u then 5
        elif number >= 1_000u then 4
        elif number >= 100u then 3
        elif number >= 10u then 2
        else 1
    let buffer = NativePtr.stackalloc<byte> numbersCount
    let mutable num = number
    let loopMax = numbersCount - 1
    for i = loopMax downto 0 do
        NativePtr.set buffer i (byte (num % 10u + 48u))
        num <- num / 10u
    for i = 0 to loopMax do
        output.WriteByte (NativePtr.get buffer i)

let private writeInt32x (output : MemoryStream) (number: int32) =
    if number > 0
    then
        let numbersCount =
            if number >= 1_000_000_000 then 10
            elif number >= 100_000_000 then 9
            elif number >= 10_000_000 then 8
            elif number >= 1_000_000 then 7
            elif number >= 100_000 then 6
            elif number >= 10_000 then 5
            elif number >= 1_000 then 4
            elif number >= 100 then 3
            elif number >= 10 then 2
            else 1
        let buffer = NativePtr.stackalloc<byte> numbersCount
        let mutable num = number
        let loopMax = numbersCount - 1
        for i = loopMax downto 0 do
            NativePtr.set buffer i (byte (num % 10 + 48))
            num <- num / 10
        for i = 0 to loopMax do
            output.WriteByte (NativePtr.get buffer i)

    else
        let posNumber = -number
        let numbersCount =
            if posNumber >= 1_000_000_000 then 10
            elif posNumber >= 100_000_000 then 9
            elif posNumber >= 10_000_000 then 8
            elif posNumber >= 1_000_000 then 7
            elif posNumber >= 100_000 then 6
            elif posNumber >= 10_000 then 5
            elif posNumber >= 1_000 then 4
            elif posNumber >= 100 then 3
            elif posNumber >= 10 then 2
            else 1
        let buffer = NativePtr.stackalloc<byte> numbersCount
        let mutable num = posNumber
        let loopMax = numbersCount - 1
        for i = loopMax downto 0 do
            NativePtr.set buffer i (byte (num % 10 + 48))
            num <- num / 10        
        output.WriteByte(byte '-')
        for i = 0 to loopMax do
            output.WriteByte (NativePtr.get buffer i)

let private writeUint8 (output : MemoryStream) (number: uint8) =
    let numbersCount =
        if number < 10uy then 1
        else if number < 100uy then 2
        else 3
    let buffer = NativePtr.stackalloc<byte> numbersCount
    let mutable num = number
    for i = 1 to numbersCount do
        NativePtr.set buffer (numbersCount - i) (byte (num % 10uy + 48uy))
        num <- num /10uy
    for i = 1 to numbersCount do
        output.WriteByte (NativePtr.get buffer (i - 1))

let private writeString (output : MemoryStream) (str: string) =
    let stringLength = str.Length - 1
    let buffer = ArrayPool.Shared.Rent (str.Length*2)
    let written = utf8Encoding.GetBytes(str,0,str.Length, buffer,0)
    output.Write(buffer, 0, written)
    ArrayPool.Shared.Return buffer

let private writeChar (output : MemoryStream) (chr: char) =
    output.WriteByte((byte)chr)

let private writeFloat (output : MemoryStream) (value: float) =
     let zeroByte = (byte)'0';
     if value = 0.0
     then
        output.WriteByte(zeroByte);output.WriteByte((byte)'.');output.WriteByte(zeroByte);
     else
         let intValue = (int)value
         writeInt32 output intValue
         output.WriteByte((byte)'.')
         let mutable decimalValue = (int)(Math.Round(value*100000.0, MidpointRounding.AwayFromZero)) - intValue*100000
         if decimalValue = 0
         then output.WriteByte(zeroByte)
         else
             let mutable temp = decimalValue
             while temp < 10000 do
                 temp <- temp*10
                 output.WriteByte(zeroByte)
             while decimalValue % 10 = 0 do
                 decimalValue <- decimalValue / 10
             writeInt32 output decimalValue


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
    writeInt32 output loc.id
    write ``,"distance":``
    writeUint8 output loc.distance
    write ``,"place":"``
    write loc.place
    write ``","city":"``
    write loc.city
    write ``","country":"``
    writeString output loc.country
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
    writeInt32 output user.id
    write ``,"birth_date":``
    writeInt32x output user.birth_date
    write ``,"first_name":"``
    write user.first_name
    write ``","last_name":"``
    write user.last_name
    write ``","gender":"``
    writeChar output user.gender
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
    let writeInt = writeInt32 output
    write ``{"id":``
    writeInt visit.id
    write ``,"location":``
    writeInt visit.location
    write ``,"mark":``
    writeUint8 output visit.mark
    write ``,"user":``
    writeInt visit.user
    write ``,"visited_at":``
    writeUInt32 output visit.visited_at
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