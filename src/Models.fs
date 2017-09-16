namespace HCup.Models

open System
open System.Collections.Concurrent


type Location() =
        [<DefaultValue>]val mutable id: int32
        [<DefaultValue>]val mutable distance : uint16
        [<DefaultValue>]val mutable city: string
        [<DefaultValue>]val mutable place: string
        [<DefaultValue>]val mutable country: string

type LocationUpd() =
        [<DefaultValue>]val mutable distance : Nullable<uint16>
        [<DefaultValue>]val mutable city: string
        [<DefaultValue>]val mutable place: string
        [<DefaultValue>]val mutable country: string

[<CLIMutable>]
type Locations =
    {
        locations : Location[]
    }


[<Struct>]
type Sex = 
    | undef = 0
    | m = 1
    | f = 2


type UserUpd() =
        [<DefaultValue>]val mutable first_name : string
        [<DefaultValue>]val mutable last_name: string
        [<DefaultValue>]val mutable birth_date: Nullable<float>
        [<DefaultValue>]val mutable gender: Nullable<Sex>
        [<DefaultValue>]val mutable email: string

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


type VisitUpd() =
        [<DefaultValue>]val mutable user : Nullable<int32>
        [<DefaultValue>]val mutable location: Nullable<int32>
        [<DefaultValue>]val mutable visited_at: Nullable<uint32>
        [<DefaultValue>]val mutable mark: Nullable<float>

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

[<Struct>]
type Average = { avg: float }

[<Struct>]
type UserVisit = { mark: float; visited_at: uint32; place: string }

[<Struct>]
type UserVisits = { visits: seq<UserVisit> }

    