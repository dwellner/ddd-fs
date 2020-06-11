module App.EventRepository
open App.Common.Aggregate
open App.Common.Types


let createRepository<'Event> ():EventRepository<'Event> = 
    let mutable events: 'Event list = []
    
    let addEvent event = event :: events |> Seq.rev |> Seq.toList
    let save = fun _ event -> async { events <- addEvent event; return Ok() }
    
    {
        loadAllEvents= (fun _-> events |> asAsync)
        save=save
    }

