namespace HCup.Actors

open System.Collections.Generic

type VisitsCollection = HashSet<int>

type Action =
    | AddVisit
    | RemoveVisit

type VisitActor () = 

    static let addVisitInternal (collection: VisitsCollection) (visitId: int) = 
        collection.Add(visitId) |> ignore

    static let removeVisitInternal (collection: VisitsCollection) (visitId: int) = 
        collection.Remove(visitId) |> ignore

    static let getActor() = 
        MailboxProcessor.Start(fun inbox -> 
            // the message processing function
            let rec messageLoop() = async {

                // read a message
                let! (action, collection, id) = inbox.Receive()

                // do the core logic
                match action with
                | AddVisit -> addVisitInternal collection id
                | RemoveVisit -> removeVisitInternal collection id

                // loop to top
                return! messageLoop () 
                }

            // start the loop 
            messageLoop ()
        )

    // create the agent
    static let userAgent = getActor()
    static let locationAgent = getActor()

    // public interface to hide the implementation
    static member AddUserVisit (collection: VisitsCollection) (visitId: int) = 
        userAgent.Post (Action.AddVisit, collection, visitId)

    static member RemoveUserVisit (collection: VisitsCollection) (visitId: int) = 
        userAgent.Post (Action.RemoveVisit, collection, visitId)
    
    static member AddLocationVisit (collection: VisitsCollection) (visitId: int) = 
        locationAgent.Post (Action.AddVisit, collection, visitId)

    static member RemoveLocationVisit (collection: VisitsCollection) (visitId: int) = 
        locationAgent.Post (Action.RemoveVisit, collection, visitId)