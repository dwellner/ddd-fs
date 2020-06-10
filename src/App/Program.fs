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
open App.Common.Aggregate


// ---------------------------------
// Web app
// ---------------------------------

let mutable ads: ClassifiedAd.ClassifiedAdEvent list = []


let repository: EventRepository<ClassifiedAd.ClassifiedAdEvent> = {
    loadAllEvents= fun id-> async { return ads }
    save=fun id event-> async { 
        ads <- (event :: ads |> Seq.rev |> Seq.toList) ; 
        return Ok () 
    }
}

let webApp =

    let classifiedAdController = App.ClassifiedAds.Controller.createController (App.ClassifiedAds.Service.execute repository)


    choose [
        subRoute "/api/hello2"
            (choose [
                GET >=> handleGetHello
            ])
        classifiedAdController
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
    let env = app.ApplicationServices.GetService<IWebHostEnvironment>()
    (*(match env.IsDevelopment() with
    | true  -> app.UseDeveloperExceptionPage()
    | false -> app.UseGiraffeErrorHandler errorHandler)*)
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