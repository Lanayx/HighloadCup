namespace HCup.Actors

type Action =
    | AddVisit
    | RemoveVisit

type VisitActor () = 

    static let addVisitInternal (collection: ResizeArray<int>) (visitId: int) = 
        collection.Add(visitId)

    static let removeVisitInternal (collection: ResizeArray<int>) (visitId: int) = 
        collection.Remove(visitId) |> ignore


    // create the agent
    static let agent = MailboxProcessor.Start(fun inbox -> 

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

    // public interface to hide the implementation
    static member AddVisit (collection: ResizeArray<int>) (visitId: int) = 
        agent.Post (Action.AddVisit, collection, visitId)

    static member RemoveVisit (collection: ResizeArray<int>) (visitId: int) = 
        agent.Post (Action.RemoveVisit, collection, visitId)