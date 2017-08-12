namespace HCup.Models

[<CLIMutable>]
type Location =
    {
        distance : string
        city: string
        place: string
        id: int32
        country: string
    }

[<CLIMutable>]
type Locations =
    {
        locations : Location[]
    }

[<CLIMutable>]
type User =
    {
        id: int32
        first_name : string
        last_name: string
        birth_date: int64
        gender: string
        email: string
    }

[<CLIMutable>]
type Users =
    {
        users : User[]
    }

[<CLIMutable>]
type Visit =
    {
        id: int32
        user : int32
        location: int32
        visited_at: uint32
        mark: uint8
    }

[<CLIMutable>]
type Visits =
    {
        visits : Visit[]
    }
