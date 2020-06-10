(* 


## brief intro to F#

## overview of key language features (with a DDD twist)

## quick glance at a F# ASP.NET Core example app (with a DDD twist)


F# is *NOT* better than C#. Nor worse. Nor equal.

A programming language is just a tool. None of them are good, really :)
Languages is about people more than about technology.

Functional programming is a superpower. Learing to to it better will 
help you in any language, whether that be F#, C# or even Python.


OOP
 - Biassed towards coupling between data and functionality (classes)
 - well suited for stateful operations (classes)
 - Traditionally Stateful operations are all around. 
   (A CRUD database is a huge shared mutable state container)

FP 
 - Biassed towards decoupling of data and functionality (functions and data)   
 - well suited for data heavy processing (e.g.: machine learning)
 - stateless computing (eg. serverless)

 - Domain driven design 

     "the state of a system is a left-fold of previous behaviours"
 

Advantages of F#
================
- functional first yet hybrid language. 

- Relatively easy, reader friendly syntax
     - very low on ceremony and boilerplate
     - Arguably, if a PO or BA can read a Yaml file, they can read an F# type file.
     - heaven compared to Clojure :)

- More strongly typed than C#, yet types are still easier to express and use 

- Type system and immutability greatly reduce runtime variances
    - less need for low-value-add unit testing

- Build on top of the .NET and .NET Core platform
    - Can seamlessly integrate with existing massive C# ecosystem. (e.g. Asp.NET Core)
    - No need to learn the platform, "just" need to learn the language


Considerations
==============

- F# is a lot less mainstream than it's OO causin C#. 
  Therefore the tooling around the language is currently less rich and advanced as for C#


- Depending on the team background and experience level, the functional programming 
  learning curve can prove too big of an investment for quite some teams


*)


// Hello world - obviously :) 

// No classes, namespaces or other clutter here when we don't need them.

let hello name = 
    printfn "Hello %s" name

hello "world!"




// values and variables
let value = 3       // C#: readonly / const
let mutable var = 4 // C#: var. red flag


// This is a record type. (will be in C# 9)
type Point = { x:int; y:int }

let p = { x=1; y=3 }
let x = p.x

// p.x <- 3 // inmutable by default
let p2 = { p with y =p.y+3 }




// functions
let myFunctionVerbose (x: int) (y:int) : int = 
    let sum = x + y
    sum

// same
let sum x y = x + y

let three = sum 1 2

// partial application of functions
let addThree = sum 3
let seven = addThree 4

// pipeline operator (experimental in JS)
let multiply x y = x * y

let twentyOne = multiply 3 6 |> addThree
let twentyFour = 9 |> multiply 3 |> addThree

// combining a range of methods into one
let addSix = addThree >> addThree
let nine = 3 |> addSix 


// FP in a nutshell - brutally oversimplified

// let events = command |> validate |> checkCredit |> placeOrder |> sendConfirmation

// OR 

// let placeOrder = validate >> checkCredit >> placeOrder >> sendConfirmation
// let events = command |> placeOrder




// TYPES - Let's do some domain modelling

// enum
type CardType =  Visa | Mastercard

// Union types help make the domain explicit
type CardNumber = CardNumber of string

let myCardnr: CardNumber = CardNumber "123"


// factory methods for enforcing validation rules
type CheckNumber = private CheckNumber of string
module CheckNumber =
    let create (v:string) = 
        match v with    
        | null -> failwith "value cannot be null"
        | s when s.Length <> 10 -> failwith "value must be exactly 10 chars"
        | s -> CheckNumber s

let myCheckNumber = CheckNumber.create "0123456789"


type CreditCardInfo = {
    CardType: CardType
    CardNumber: CardNumber
}

// Discriminated union. Super powerful. Maybe in C# 10?
// closed set by default, allowing exhaustive pattern matching!
type PaymentMethod = 
    | Cash
    | Card of CreditCardInfo
    | Check of CheckNumber



// An example domain - An ordering process for a sparepart retail ordering system

type ItemNumber = ItemNumber of string
type CustomerId = CustomerId of string
type BookingSource = Online | Walk_In

type BookOrderCommand = { 
    CustomerId: CustomerId
    ItemNumbers: ItemNumber list
    Source: BookingSource
}

// the resulting event when the command is successfull
type OrderBookedEvent = {
    Timestamp: string
    CustomerId: string
    ItemNumbers: string list
    Source: string
}

// all the business reasons that a command could fail. 
type BookOrderError =
     | ItemsOutOfStock of ItemNumber list
     | CustomerHasBadCredit
     | TooManyOpenOrders of int


// declaration of our command handler (C#: interface)
type BookOrder = BookOrderCommand -> Result<OrderBookedEvent, BookOrderError>


// implementation of our command handler
let bookOrder:BookOrder = fun command -> 
    // validation be here

    let event = {
        Timestamp=System.DateTime.UtcNow.ToString()
        CustomerId= command.CustomerId |> fun (CustomerId id) -> id
        ItemNumbers=command.ItemNumbers|> List.map (fun (ItemNumber nr) -> nr)
        Source=match command.Source with Online -> "www" | Walk_In -> "shop"
    }

    // business logic be here
    match event.CustomerId with 
    | "42" -> Error CustomerHasBadCredit
    | _ -> Ok event 



(* Some links if interested: 

    This materials:       github.com/dwellner/ddd-fs
    Learning F#:          fsharpforfunandprofit.com
    Domain Driven Design  Hands-On Domain-Driven Design with .NET Core: 
                          Alexey Zimarev

    Event sourcing        https://www.youtube.com/watch?v=JHGkaShoyNs
                          Greg Young

*)






   
// async
let getCustomerSaldo customerId = async { return 0 } //fake API call

type CustomerCredit = Good | Bad

let getCustomerCredit customerId limit = 
    async {
        let! saldo = getCustomerSaldo customerId  // let! inside async == "await"
        return if saldo < limit then Good else Bad
    }


open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols

// units of measure
let oneHour = 3600<s>
let oneKg = 1<kg>
// let oneHour + oneKg <-- will not compile

[<Measure>] type visitors
[<Measure>] type subscribers
[<Measure>] type conversionRate = visitors / subscribers

let rate: decimal<conversionRate> = 500m<visitors> / 200m<subscribers>
