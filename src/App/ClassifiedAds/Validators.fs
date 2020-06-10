module App.ClassifiedAds.Validators

open App.ClassifiedAds.Types
open FsToolkit.ErrorHandling
open App.Common.Validators
open ClassifiedAd

let (>>=) left right = left |> Result.bind right

let private validateCreate (request:CreateAdRequest)  = result {
    let! id = request.Id |> nonEmptyString "Id" >>= toGuid
    let! ownerId = request.OwnerId |>  nonEmptyString "OwnerId" >>= toGuid
    return Create { Id = AdId id; OwnerId = UserId ownerId }
  }

let private validateApprove (request:ApproveAdRequest)  = result {
  let! id = request.Id |> nonEmptyString "Id" >>= toGuid
  let! approverId = request.ApproverId |>  nonEmptyString "ApproverId" >>= toGuid

  return Approve { Id = AdId id; ApproverId= UserId approverId }
}


let private validatePublish (request:PublishAdRequest)  = result {
    let! id = request.Id |> nonEmptyString "Id" >>= toGuid

    return Publish { 
        Id = AdId id 
     }
  }


let validateCommand =  function 
    | (CreateRequest request) -> validateCreate request
    | (ApproveRequest request) -> validateApprove request
    | (PublishRequest request ) -> validatePublish request
