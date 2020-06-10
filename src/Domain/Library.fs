module Domain
open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols


// Hello world
let hello name =
    printfn "Hello %s" name

let name = "world!"
hello name

// values and variables
let value = 3       // C#: readonly
let mutable var = 4 // C#: var. Red flag


// functions
let myFunctionVerbose (x: int) (y:int) : int = 
    x + y

let sum x y = x + y

let three = sum 1 2
// partial application
let addThree = sum 3

let seven = addThree 4

// pipeline operator (experimental in JS)
let multiply x y = x * y

let twentyOne = multiply 3 6 |> addThree

let twentyFour = 9 |> multiply 3 |> addThree

//let events = placeOrder |> validate |> checkCredit |> placeOrder |> sendConfirmation

let oneHour = 3600<s>
let oneKg = 1<kg>
// let oneHour + oneKg <-- will not compile


[<Measure>] type visitors
[<Measure>] type subscribers
[<Measure>] type conversionRate = visitors / subscribers

let rate: decimal<conversionRate> = 500m<visitors> / 200m<subscribers>

// TYPES - domain modelling


// modelling domain errors as first class citizens rather than implicit exceptions
// a bit like checked exceptions in Java

// explicit value types are easy and cheap
type ItemNumber = ItemNumber of string
type CustomerId = CustomerId of string

// let myItemNr: ItemNumber = "123" wont compile. Explicit type is needed
let myItemNr: ItemNumber = ItemNumber "123"

// normal enum
type BookingSource =
    | Online
    | Walk_In

// the command. This is a record type. (will be in C# 9)
type BookOrderCommand = { 
    CustomerId: CustomerId
    ItemNumbers: ItemNumber list
    Source: BookingSource
}

let myBookingCommand = { 
    CustomerId=CustomerId "123"
    ItemNumbers=[]
    Source=Online
}

// records are immutable. Mutation can only be done by creating a new one.
let anotherCmd = { myBookingCommand with Source=Walk_In }

// the resulting event when the command is successfull
type OrderBookedEvent = {
    Timestamp: string
    CustomerId: string
    ItemNumbers: string list
    Source: string
}

// all the business reasons that a command could fail
type BookOrderError =
     | ItemsOutOfStock of ItemNumber list
     | CustomerHasBadCredit
     | TooManyOpenOrders of int


// declaration of our command handler (C#: interface)
type BookOrder = BookOrderCommand -> Result<OrderBookedEvent, BookOrderError>


// implementation of our command handler
let bookOrder:BookOrder = fun command -> 

    // business logic goes here, either an 'Ok' Result is returned with the event
    // or an error is returned. 

    Ok({
        Timestamp=System.DateTime.UtcNow.ToString()
        CustomerId= command.CustomerId |> fun (CustomerId id) -> id
        ItemNumbers=command.ItemNumbers|> List.map (fun (ItemNumber nr) -> nr)
        Source=match command.Source with Online -> "www" | Walk_In -> "shop"
    })


type CardNumber = CardNumber of string
type CheckNumber = CheckNumber of string
type CardType =  Visa | Mastercard

type CreditCardInfo = {
    CardType: CardType
    CardNumber: CardNumber
}

type PaymentMethod = 
    | Cash
    | Card of CreditCardInfo
    | Check of CheckNumber


type Payment = {
    Amount: decimal
    Method: PaymentMethod
}

// pattern matching instead of if/else (introduced also in C#8)
let describePayment {Amount=amount; Method=method} = 
    let description = 
        match method with
        | Cash -> "cash"
        | Card {CardType=Visa} -> "Visa card"
        | Card {CardType=Mastercard} -> "Mastercard"
        | Check _ -> "Check"

    sprintf "%f EUR paid by %s" amount description 


// Some/None instead of null (Similar to Optional in C# but much easier to use)
let getFirstName (fullName:string) = 
    let nameParts = fullName.Split ' '
    if nameParts.Length > 1 then Some nameParts.[0] else None


let createGreeting name =
    match getFirstName name with
    | Some firstName -> sprintf "Hi %s" name
    | None -> sprintf "Dear customer"


let getCustomerSaldo customerId = async { return 0 } //fake API call


// async
type CustomerCredit = Good | Bad

let getCustomerCredit customerId limit = 
    async {
        let! saldo = getCustomerSaldo customerId  // let! inside async == "await"
        return if saldo < limit then Good else Bad
    }


