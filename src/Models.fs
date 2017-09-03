namespace HCup.Models

open System
open System.Collections.Concurrent


type Location() =
        [<DefaultValue>]val mutable id: int32
        [<DefaultValue>]val mutable distance : uint16
        [<DefaultValue>]val mutable city: string
        [<DefaultValue>]val mutable place: string
        [<DefaultValue>]val mutable country: string

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
        birth_date: Nullable<float>
        gender: Nullable<Sex>
        email: string
    }

type User() =
        [<DefaultValue>]val mutable id: int32
        [<DefaultValue>]val mutable first_name : string
        [<DefaultValue>]val mutable last_name: string
        [<DefaultValue>]val mutable birth_date: float
        [<DefaultValue>]val mutable gender: Sex
        [<DefaultValue>]val mutable email: string

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
        mark: Nullable<float>
    }

type Visit() =
        [<DefaultValue>]val mutable id: int32
        [<DefaultValue>]val mutable user : int32
        [<DefaultValue>]val mutable location: int32
        [<DefaultValue>]val mutable visited_at: uint32
        [<DefaultValue>]val mutable mark: float

[<CLIMutable>]
type Visits =
    {
        visits : Visit[]
    }

[<Struct>]
type StructOption<'a> =
    | Som of 'a
    | Non
