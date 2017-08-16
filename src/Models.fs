namespace HCup.Models

open System
open System.Collections.Concurrent
open Newtonsoft.Json

[<CLIMutable>]
[<Struct>]
type Location =
    {
        distance : uint16
        city: string
        place: string
        id: int32
        country: string
    }

[<CLIMutable>]
[<Struct>]
type LocationUpd =
    {
        distance : Nullable<uint16>
        city: string
        place: string
        country: string
    }

[<CLIMutable>]
[<Struct>]
type Locations =
    {
        locations : Location[]
    }


[<Struct>]
type Sex = 
    | m = 'm'
    | f = 'f'

[<CLIMutable>]
[<Struct>]
type UserUpd =
    {
        first_name : string
        last_name: string
        birth_date: Nullable<int64>
        gender: Nullable<Sex>
        email: string
    }

[<CLIMutable>]
[<Struct>]
type User =
    {
        id: int32
        first_name : string
        last_name: string
        birth_date: int64
        gender: Sex
        email: string
    }

[<CLIMutable>]
[<Struct>]
type Users =
    {
        users : User[]
    }

[<CLIMutable>]
[<Struct>]
type VisitUpd =
    {
        user : Nullable<int32>
        location: Nullable<int32>
        visited_at: Nullable<uint32>
        mark: Nullable<uint8>
    }

[<CLIMutable>]
[<Struct>]
type Visit =
    {
        id: int32
        user : int32
        location: int32
        visited_at: uint32
        mark: uint8
    }

[<CLIMutable>]
[<Struct>]
type Visits =
    {
        visits : Visit[]
    }
