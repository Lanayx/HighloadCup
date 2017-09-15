module Counter

open System
let outstandingRequestCount = ref 0
let mutable lastRequestCount = 0


let syncTimer = new System.Timers.Timer(800.0)
syncTimer.Elapsed.Add(fun arg ->
    if (lastRequestCount > 0 && lastRequestCount = outstandingRequestCount.Value)
    then
        Console.WriteLine("Running GC")
        GC.Collect(2)
    else
        lastRequestCount <- outstandingRequestCount.Value
)
syncTimer.AutoReset <- true
syncTimer.Enabled <- true
syncTimer.Start()