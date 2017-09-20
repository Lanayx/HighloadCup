namespace HCup.Actors

open System.Collections.Generic

type VisitsCollectionSorted = SortedList<uint32,int>
type VisitsCollection = ResizeArray<int>

type Action =
    | AddVisit
    | RemoveVisit

type VisitActor () = 

    static let addVisitSorted (collection: VisitsCollectionSorted) (visitId: int) (visitedAt: uint32) = 
        collection.Add(visitedAt, visitId) |> ignore

    static let removeVisitSorted (collection: VisitsCollectionSorted) (visitedAt: uint32) = 
        collection.Remove(visitedAt) |> ignore

    static let addVisitRegular (collection: VisitsCollection) (visitId: int) = 
        collection.Add(visitId) |> ignore

    static let removeVisitRegular (collection: VisitsCollection) (visitId: int) = 
        collection.Remove(visitId) |> ignore

    static let getLocationActor() = 
        MailboxProcessor.Start(fun inbox -> 
            // the message processing function
            let rec messageLoop() = async {

                // read a message
                let! (action, collection, id) = inbox.Receive()

                // do the core logic
                match action with
                | AddVisit -> addVisitRegular collection id
                | RemoveVisit -> addVisitRegular collection id

                // loop to top
                return! messageLoop () 
                }

            // start the loop 
            messageLoop ()
        )

    static let getUserActor() = 
        MailboxProcessor.Start(fun inbox -> 
            // the message processing function
            let rec messageLoop() = async {

                // read a message
                let! (action, collection, id, visitedAt) = inbox.Receive()

                // do the core logic
                match action with
                | AddVisit -> addVisitSorted collection id visitedAt
                | RemoveVisit -> removeVisitSorted collection visitedAt

                // loop to top
                return! messageLoop () 
                }

            // start the loop 
            messageLoop ()
        )

    // create the agent
    static let userAgent = getUserActor()
    static let locationAgent = getLocationActor()

    // public interface to hide the implementation
    static member AddUserVisit userId (collection: VisitsCollectionSorted) (visitId: int) (visitedAt: uint32) = 
        userAgent.Post (Action.AddVisit, collection, visitId, visitedAt)

    static member RemoveUserVisit userId (collection: VisitsCollectionSorted) (visitId: int) (visitedAt: uint32) = 
        userAgent.Post (Action.RemoveVisit, collection, visitId, visitedAt)
    
    static member AddLocationVisit locactionId (collection: VisitsCollection) (visitId: int) =
        locationAgent.Post (Action.AddVisit, collection, visitId)

    static member RemoveLocationVisit locactionId (collection: VisitsCollection) (visitId: int) = 
        locationAgent.Post (Action.RemoveVisit, collection, visitId)