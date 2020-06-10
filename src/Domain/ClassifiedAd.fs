module ClassifiedAd

// ----- Value types ----------

type AdId = AdId of System.Guid
module AdId =
    let value (AdId id) = id.ToString()

type UserId = UserId of System.Guid
module UserId =
    let value (UserId id) = id.ToString()


// ----- Commands ----------

type CreateAd = {
    Id: AdId
    OwnerId: UserId
}

type ApproveAd = {
    Id: AdId
    ApproverId: UserId
}


type PublishAd = {
    Id: AdId
}

type ClassifiedAdCommand = 
    | Create of CreateAd
    | Approve of ApproveAd
    | Publish of PublishAd


// ------ Events ----------

type AdCreated = {
    Id: string
    OwnerId: string
}

type AdApproved = {
    Id: string
    ApproverId: string
}

type AdPublished = {
    Id: string
}

type ClassifiedAdEvent = 
    | Created of AdCreated
    | Approved of AdApproved
    | Published of AdPublished


// ------ Errors -----------

type ClassifiedAdError = 
    | CannotPublishAdWithoutApproval
    | CannotPublishAlreadyPublishedAd

// internal state

type AdStatus = Inactive | PendingReview | Active

type State = private {    
    Status : AdStatus;
    ApprovedBy: string option
}
with static member Zero = { 
                       Status = Inactive 
                       ApprovedBy = None
                   }
// You can derive current state from events |> Seq.fold apply aggregate.Zero
let apply state = function
    | Created _ -> state
    | Approved e -> { state with ApprovedBy =Some e.ApproverId }
    | Published _ -> { state with Status = Active; }


let private ensureValidState state event =  
    match event with
    | Published _-> match state.Status, state.ApprovedBy with
                    | Active, _ -> Error CannotPublishAlreadyPublishedAd
                    | Inactive, None -> Error CannotPublishAdWithoutApproval
                    | _, _ -> Ok event 
    | _ -> Ok event

let getId cmd =
    match cmd with
    | Create cmd -> cmd.Id
    | Approve cmd -> cmd.Id
    | Publish cmd -> cmd.Id

// domain operations
let private create (cmd:CreateAd) =
    Created {
        Id=AdId.value cmd.Id
        OwnerId=UserId.value cmd.OwnerId 
    }

let private approve (cmd:ApproveAd) =
    Approved {
        Id=AdId.value cmd.Id
        ApproverId=UserId.value cmd.ApproverId 
    }

let private publish (cmd:PublishAd) =
    Published {
        Id=AdId.value cmd.Id
    }

// command executor. Execute curentState command will return new event or error
let execute (state:State) :ClassifiedAdCommand->Result<ClassifiedAdEvent, ClassifiedAdError> = 
    let apply event = 
        let newState = apply state event
        Ok event

    let ensureValidStateAndApply = (ensureValidState state) >> (Result.bind apply)

    function
    | Create cmd -> cmd |> create |> ensureValidStateAndApply                    
    | Approve cmd -> cmd |> approve |> ensureValidStateAndApply        
    | Publish cmd -> cmd |> publish |> ensureValidStateAndApply
      