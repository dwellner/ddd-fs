module App.Common.Aggregate

open App.Common.Types
open FsToolkit.ErrorHandling

type EventStreamId = EventStreamId of string

type Aggregate<'State, 'Command, 'Event,'Error> = {    
    Zero: 'State
    apply: 'State -> 'Event -> 'State
    execute: 'State -> 'Command -> Result<'Event,'Error>
    getEventStreamId:'Command->EventStreamId
}

type EventRepository<'Event> = {
    loadAllEvents: EventStreamId->Async<'Event list>
    save:EventStreamId->'Event->AsyncResult<Unit,PersistanceError>
}

let updateAggregate 
    (repository: EventRepository<'Event>)
    (aggregate: Aggregate<'State, 'Command, 'Event,'Error>) 
    (command:'Command)
        : AsyncResult<Unit,TransactionError<'Error>> = 

            let id = aggregate.getEventStreamId command

            let save event = repository.save id event |> AsyncResult.mapError PersistanceFailed

            async {
                let! events = repository.loadAllEvents id
                let currentState = events |> Seq.fold aggregate.apply aggregate.Zero

                let result = aggregate.execute currentState command
                return! match result with 
                        | Ok newEvent -> save newEvent
                        | Error err -> Error (DomainValidationFailed err) |> asAsync
            }

    