module App.ClassifiedAds.Service

open App.Common.Types
open App.Common.Aggregate
open ClassifiedAd

let private aggregate: Aggregate<ClassifiedAd.State, ClassifiedAdCommand, ClassifiedAdEvent,ClassifiedAdError> = {    
    Zero  = ClassifiedAd.State.Zero;
    apply = ClassifiedAd.apply
    execute  = ClassifiedAd.execute
    getEventStreamId= ClassifiedAd.getId>>AdId.value>>EventStreamId 
}

let mapDomainError (err:ClassifiedAdError) = 
    match err with
    | CannotPublishAdWithoutApproval -> "Cannot publish ad without approval"
    | CannotPublishAlreadyPublishedAd -> "Cannot publish already published ad."

let mapError err =
    match err with
    | (PersistanceFailed OptimisticConcurrencyError) -> ConcurencyError
    | (DomainValidationFailed err) ->  err |> mapDomainError |> DomainError

let execute 
    (eventRepository: EventRepository<ClassifiedAdEvent>)
    (command:ClassifiedAdCommand) :CommandResult = async {
        let! result = updateAggregate eventRepository aggregate command
        return match result with
                | Ok () -> Ok ()
                | Error err -> Error <| mapError err 
    }


