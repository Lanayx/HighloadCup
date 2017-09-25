open System
open System.Buffers
open System.IO

open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running
open FSharp.NativeInterop

let private writeInt32LogPool (output : MemoryStream) (number: int) =
    let numbersCount = (int) ((float)number |> Math.Log10 ) + 1
    let buffer = ArrayPool.Shared.Rent numbersCount
    let mutable num = number
    for i = 1 to numbersCount do
        buffer.[numbersCount - i] <- (byte) (num % 10 + 48)
        num <- num /10
    output.Write(buffer, 0, numbersCount)
    ArrayPool.Shared.Return buffer

let private writeInt32TablePool (output : MemoryStream) (number: int) =
    let numbersCount =
        if number < 10 then 1
        else if number < 100 then 2
        else if number < 1_000 then 3
        else if number < 10_000 then 4
        else if number < 100_000 then 5
        else if number < 1_000_000 then 6
        else if number < 10_000_000 then 7
        else if number < 100_000_000 then 8
        else if number < 1_000_000_000 then 9
        else 10
    let buffer = ArrayPool.Shared.Rent numbersCount
    let mutable num = number
    for i = 1 to numbersCount do
        buffer.[numbersCount - i] <- (byte) (num % 10 + 48)
        num <- num /10
    output.Write(buffer, 0, numbersCount)
    ArrayPool.Shared.Return buffer

#nowarn "9"

let private writeInt32LogStackalloc (output : MemoryStream) (number: int) =
    let numbersCount = (int) ((float)number |> Math.Log10 ) + 1
    let buffer = NativePtr.stackalloc<byte> numbersCount
    let mutable num = number
    for i = 1 to numbersCount do
        NativePtr.set buffer (numbersCount - i) (byte (num % 10 + 48))
        num <- num / 10
    for i = 1 to numbersCount do
        output.WriteByte (NativePtr.get buffer (i - 1))

let private writeInt32TableStackalloc (output : MemoryStream) (number: int) =
    let numbersCount =
        if number < 10 then 1
        else if number < 100 then 2
        else if number < 1_000 then 3
        else if number < 10_000 then 4
        else if number < 100_000 then 5
        else if number < 1_000_000 then 6
        else if number < 10_000_000 then 7
        else if number < 100_000_000 then 8
        else if number < 1_000_000_000 then 9
        else 10
    let buffer = NativePtr.stackalloc<byte> numbersCount
    let mutable num = number
    for i = 1 to numbersCount do
        NativePtr.set buffer (numbersCount - i) (byte (num % 10 + 48))
        num <- num / 10
    for i = 1 to numbersCount do
        output.WriteByte (NativePtr.get buffer (i - 1))

type SerializerBenchmarks() =
    let count = 1000
    let numbers = [| 1..count |]
    let buffers = numbers |> Array.map(fun _ -> new MemoryStream(100))

    [<Benchmark>]
    member __.WriteInt32LogPool() =
        Array.mapi(fun i n -> writeInt32LogPool buffers.[i] n; buffers.[i]) numbers

    [<Benchmark>]
    member __.WriteInt32TablePool() =
        Array.mapi(fun i n -> writeInt32TablePool buffers.[i] n; buffers.[i]) numbers

    [<Benchmark>]
    member __.WriteInt32LogStackalloc() =
        Array.mapi(fun i n -> writeInt32LogStackalloc buffers.[i] n; buffers.[i]) numbers

    [<Benchmark>]
    member __.WriteInt32TableStackalloc() =
        Array.mapi(fun i n -> writeInt32TableStackalloc buffers.[i] n; buffers.[i]) numbers

[<EntryPoint>]
let main argv =
    ignore <| BenchmarkRunner.Run<SerializerBenchmarks>()
    0

(*                  Method |     Mean |    Error |   StdDev |
-------------------------- |---------:|---------:|---------:|
         WriteInt32LogPool | 171.7 us | 3.168 us | 2.645 us |
       WriteInt32TablePool | 159.4 us | 2.714 us | 2.406 us |
   WriteInt32LogStackalloc | 140.9 us | 2.933 us | 5.437 us |
 WriteInt32TableStackalloc | 117.6 us | 2.367 us | 5.246 us | *)