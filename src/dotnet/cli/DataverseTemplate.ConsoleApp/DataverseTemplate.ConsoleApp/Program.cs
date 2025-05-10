// See https://aka.ms/new-console-template for more information

using Cocona;
using DataverseTemplate.ConsoleApp.Commands;
using DataverseTemplate.ConsoleApp.Services.Dataverse;
using DataverseTemplate.ConsoleApp.Services.Dataverse.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

var builder = CoconaApp.CreateBuilder();
builder.Configuration
        .AddEnvironmentVariables()
        .AddJsonFile("appsettings.json", false, false)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", false, false);

if (!builder.Environment.IsProduction())
{
    //Secrets should never be in clear text in source controlled file such appsettings.json.
    //For Developement, we therefore store them locally into UserSecrets Store, part of dotnet foundation.
    //For Production, secrets can be either written into appsettings.Production.json file by pipeline
    //or you can configure another Configuration Provider to provide the secrets like AzureKeyvault or Hashicorp Vault.
    builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly());
}

builder.Services
    .AddLogging()
    .Configure<SdkDataverseServiceFactoryOptions>(builder.Configuration.GetSection("Dataverse"))
    .AddDataverseClient();


var app = builder.Build();
app.AddCommands<DataverseCommands>();
await app.RunAsync();


