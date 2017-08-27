namespace HCup.Models

open System
open System.Collections.Concurrent

[<CLIMutable>]
type Location =
    {
        id: int32
        mutable distance : uint16
        mutable city: string
        mutable place: string
        mutable country: string
    }

[<CLIMutable>]
type LocationUpd =
    {
        distance : Nullable<uint16>
        city: string
        place: string
        country: string
    }

[<CLIMutable>]
type Locations =
    {
        locations : Location[]
    }


[<Struct>]
type Sex = 
    | m = 0
    | f = 1

[<CLIMutable>]
type UserUpd =
    {
        first_name : string
        last_name: string
        birth_date: Nullable<int64>
        gender: Nullable<Sex>
        email: string
    }

[<CLIMutable>]
type User =
    {
        id: int32
        mutable first_name : string
        mutable last_name: string
        mutable birth_date: int64
        mutable gender: Sex
        mutable email: string
    }

[<CLIMutable>]
type Users =
    {
        users : User[]
    }

[<CLIMutable>]
type VisitUpd =
    {
        user : Nullable<int32>
        location: Nullable<int32>
        visited_at: Nullable<uint32>
        mark: Nullable<uint8>
    }

[<CLIMutable>]
type Visit =
    {
        id: int32
        mutable user : int32
        mutable location: int32
        mutable visited_at: uint32
        mutable mark: uint8
    }

[<CLIMutable>]
type Visits =
    {
        visits : Visit[]
    }

[<Struct>]
type StructOption<'a> =
    | Som of 'a
    | Non
