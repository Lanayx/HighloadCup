namespace HCup.Actors

open System.Collections.Generic

type VisitsCollection = ResizeArray<int>

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
    static let userAgent1 = getActor()
    static let userAgent2 = getActor()
    static let locationAgent1 = getActor()
    static let locationAgent2 = getActor()

    // public interface to hide the implementation
    static member AddUserVisit userId (collection: VisitsCollection) (visitId: int) = 
        let userAgent = if userId % 2 = 0 then userAgent1 else userAgent2 
        userAgent.Post (Action.AddVisit, collection, visitId)

    static member RemoveUserVisit userId (collection: VisitsCollection) (visitId: int) = 
        let userAgent = if userId % 2 = 0 then userAgent1 else userAgent2 
        userAgent.Post (Action.RemoveVisit, collection, visitId)
    
    static member AddLocationVisit locactionId (collection: VisitsCollection) (visitId: int) = 
        let locationAgent = if locactionId % 2 = 0 then locationAgent1 else locationAgent2 
        locationAgent.Post (Action.AddVisit, collection, visitId)

    static member RemoveLocationVisit locactionId (collection: VisitsCollection) (visitId: int) = 
        let locationAgent = if locactionId % 2 = 0 then locationAgent1 else locationAgent2
        locationAgent.Post (Action.RemoveVisit, collection, visitId)