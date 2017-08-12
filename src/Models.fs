namespace HCup.Models

[<CLIMutable>]
type Location =
    {
        distance : string
        city: string
        place: string
        id: uint32
        country: string
    }

[<CLIMutable>]
type Locations =
    {
        locations : Location[]
    }


