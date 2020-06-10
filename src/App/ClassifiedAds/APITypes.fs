module App.ClassifiedAds.Types

type CreateAdRequest = {
    Id: string
    OwnerId: string
}

type ApproveAdRequest = {
    Id: string
    ApproverId: string
}


type PublishAdRequest = {
    Id: string
}

type ClassifiedAdCommandRequest = 
    | CreateRequest of CreateAdRequest
    | ApproveRequest of ApproveAdRequest
    | PublishRequest of PublishAdRequest


