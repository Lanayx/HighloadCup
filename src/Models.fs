namespace HCup.Models

open System
open System.Collections.Concurrent


type Location() =
        [<DefaultValue>]val mutable id: int32
        [<DefaultValue>]val mutable distance : uint8
        [<DefaultValue>]val mutable city: string 
        [<DefaultValue>]val mutable place: string
        [<DefaultValue>]val mutable country: string


[<CLIMutable>]
type LocationUpd =
    {
        distance : Nullable<uint8>
        city: string
        place: string
        country: string
    }

[<CLIMutable>]
type Locations =
    {
        locations : Location[]
    }

[<CLIMutable>]
type UserUpd =
    {
        first_name : string
        last_name: string
        birth_date: Nullable<int32>
        gender: Nullable<char>
        email: string
    }


type User() =
        [<DefaultValue>]val mutable id: int32
        [<DefaultValue>]val mutable first_name : string
        [<DefaultValue>]val mutable last_name: string
        [<DefaultValue>]val mutable birth_date: int32
        [<DefaultValue>]val mutable gender: char
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
        mark: Nullable<uint8>
    }

type Visit() =
        [<DefaultValue>]val mutable id: int32
        [<DefaultValue>]val mutable user : int32
        [<DefaultValue>]val mutable location: int32
        [<DefaultValue>]val mutable visited_at: uint32
        [<DefaultValue>]val mutable mark: uint8

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
type UserVisit = { mark: uint8; visited_at: uint32; place: string }