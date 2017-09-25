open System
open System.Buffers
open System.IO

open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running
open FSharp.NativeInterop

let private writeInt32Bufferless (output : MemoryStream) (number: int) =
    let numbersCount = (int) ((float)number |> Math.Log10 ) + 1
    let mutable num = number
    for i = numbersCount downto 2 do
        let divider = (int)(10.0 ** (float)(i-1))
        let number = num / divider
        output.WriteByte((byte) (number + 48))
        num <- num - number*divider
    output.WriteByte((byte) (num + 48))

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

    [<Benchmark>]
    member __.WriteInt32Bufferless() =
        Array.mapi(fun i n -> writeInt32Bufferless buffers.[i] n; buffers.[i]) numbers

[<EntryPoint>]
let main argv =
    ignore <| BenchmarkRunner.Run<SerializerBenchmarks>()
    0

(*                  Method |     Mean |    Error |   StdDev |
-------------------------- |---------:|---------:|---------:|
         WriteInt32LogPool | 177.8 us | 3.535 us | 4.471 us |
       WriteInt32TablePool | 165.3 us | 3.218 us | 4.404 us |
   WriteInt32LogStackalloc | 140.5 us | 2.756 us | 5.631 us |
 WriteInt32TableStackalloc | 115.9 us | 2.285 us | 4.402 us |
      WriteInt32Bufferless | 169.1 us | 3.317 us | 5.722 us | *)