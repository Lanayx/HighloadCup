namespace HCup.Models

open System
open System.Collections.Concurrent


// type Location =
//     struct
//         val id: int32
//         val distance : uint8
//         val place: string
//         val city: string
//         val country: string
//         new(_id, _distance, _place, _city, _country) = { id = _id; distance=_distance; place=_place; city=_city; country =_country }
//     end

type Location() =
        [<DefaultValue>]val mutable id: int32
        [<DefaultValue>]val mutable distance : uint8
        [<DefaultValue>]val mutable city: byte[] 
        [<DefaultValue>]val mutable place: byte[]
        [<DefaultValue>]val mutable country: string

type LocationOld() =
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
        locations : LocationOld[]
    }

[<CLIMutable>]
type UserUpd =
    {
        first_name : string
        last_name: string
        birth_date: Nullable<int64>
        gender: Nullable<char>
        email: string
    }

// type User =
//     struct
//         val id: int32
//         val first_name : string
//         val last_name: string
//         val birth_date: float
//         val gender: Sex
//         val email: string
//         new(_id, _first_name, _last_name, _birth_date, _gender, _email) = { id = _id; first_name=_first_name; last_name=_last_name; birth_date=_birth_date; gender =_gender; email = _email }
//     end

type User() =
        [<DefaultValue>]val mutable id: int32
        [<DefaultValue>]val mutable first_name : byte[]
        [<DefaultValue>]val mutable last_name: byte[]
        [<DefaultValue>]val mutable birth_date: int64
        [<DefaultValue>]val mutable gender: char
        [<DefaultValue>]val mutable email: byte[]

type UserOld() =
        [<DefaultValue>]val mutable id: int32
        [<DefaultValue>]val mutable first_name : string
        [<DefaultValue>]val mutable last_name: string
        [<DefaultValue>]val mutable birth_date: int64
        [<DefaultValue>]val mutable gender: char
        [<DefaultValue>]val mutable email: string



[<CLIMutable>]
type Users =
    {
        users : UserOld[]
    }

[<CLIMutable>]
type VisitUpd =
    {
        user : Nullable<int32>
        location: Nullable<int32>
        visited_at: Nullable<uint32>
        mark: Nullable<uint8>
    }

// type Visit =
//     struct
//         val id: int32
//         val user : int32
//         val location: int32
//         val visited_at: uint32
//         val mark: float
//         new(_id, _user, _location, _visited_at, _mark) = { id = _id; user=_user; location=_location; visited_at=_visited_at; mark =_mark }

//     end

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
type UserVisit = { mark: uint8; visited_at: uint32; place: byte[] }