module App.Common.Types

type AsyncResult<'T,'Err> = Async<Result<'T,'Err>>

let asAsync x = async{ return x }

type CommandError =
    | ValidationError of string
    | DomainError of string
    | ConcurencyError

type PersistanceError =
    | OptimisticConcurrencyError


type TransactionError<'DomainError> =
    | PersistanceFailed of PersistanceError
    | DomainValidationFailed of 'DomainError


type CommandResult = AsyncResult<Unit,CommandError>


