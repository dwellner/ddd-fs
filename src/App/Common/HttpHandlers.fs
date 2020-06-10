module App.Common.HttpHandlers

open Giraffe
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe.SerilogExtensions
open App.Common.Types

type CommandHandler<'Command> = 'Command->CommandResult

let handleCommand<'Command> (handle:CommandHandler<'Command>) = 
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let logger = ctx.Logger() 
            let! request = ctx.BindJsonAsync<'Command>()
            logger.Information <| sprintf "processing request: %s: %s" (request.GetType().Name) (request.ToString())             

            let! result = handle request 

            let (code, message) = 
                match result with 
                | Ok _ -> 200, "OK"
                | Error (ValidationError msg) -> 400, msg
                | Error (DomainError msg) -> 403, msg  
                | Error (ConcurencyError) -> 451, "Concurrency error"

            if (code = 200) then logger.Information "Command successfully completed" 
            else logger.Error <| sprintf "command FAILED: %d %s" code message             

            ctx.SetStatusCode code
            return! if (code = 200) then next ctx else text message next ctx
        }

