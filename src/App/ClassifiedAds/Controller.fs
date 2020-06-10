module App.ClassifiedAds.Controller

open Giraffe
open App.Common.Types
open App.Common.HttpHandlers
open App.ClassifiedAds.Types
open App.ClassifiedAds.Validators

open FsToolkit.ErrorHandling


let createController executeDomainCommand
     :HttpHandler =

    let validateAndExecute request = 
        request 
        |> validateCommand 
        |> asAsync
        |> AsyncResult.mapError ValidationError 
        |> AsyncResult.bind executeDomainCommand


    let controller = choose [
        route "/create" >=> 
            handleCommand<CreateAdRequest> (CreateRequest >> validateAndExecute)
        route "/approve" >=> 
            handleCommand<ApproveAdRequest> (ApproveRequest >> validateAndExecute)
        route "/publish" >=> 
            handleCommand<PublishAdRequest> (PublishRequest >> validateAndExecute)
    ]

    subRoute "/api/ad" (POST >=> controller)
