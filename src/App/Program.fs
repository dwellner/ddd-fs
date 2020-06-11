module App.App

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe
open App.HttpHandlers
open Giraffe.SerilogExtensions
open Serilog


let createClassifiedAdCommandController () = 
    let execute=App.ClassifiedAds.Service.execute
    let repository=
        App.EventRepository.createRepository<ClassifiedAd.ClassifiedAdEvent>()
    App.ClassifiedAds.Controller.createController (execute repository)

// ---------------------------------
// Web app
// ---------------------------------

let webApp =
    choose [
        subRoute "/api/hello2"
            (choose [
                GET >=> handleGetHello
            ])
        createClassifiedAdCommandController ()
        setStatusCode 404 >=> text "Not Found" ]
        
// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder.WithOrigins("http://localhost:8080")
           .AllowAnyMethod()
           .AllowAnyHeader()
           |> ignore

let configureApp (app : IApplicationBuilder) =
    app
        .UseHttpsRedirection()
        .UseCors(configureCors)
        .UseGiraffe(SerilogAdapter.Enable(webApp))

let configureServices (services : IServiceCollection) =
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore

Log.Logger <- 
  LoggerConfiguration()
    .Destructure.FSharpTypes()
    .WriteTo.Console() 
    .CreateLogger() 

[<EntryPoint>]
let main _ =
    WebHostBuilder()
        .UseKestrel()
        .UseIISIntegration()
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .Build()
        .Run()
    0