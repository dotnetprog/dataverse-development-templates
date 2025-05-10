using Cocona;
using Dataverse.Earlybound.Console.Commands;
using Dataverse.Earlybound.Console.Services.Dataverse;
using Dataverse.Earlybound.Console.Services.Dataverse.Configuration;
using Dataverse.Earlybound.Console.Services.Dataverse.Metadata;
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
    .AddDataverseClient()
    .AddTransient<IDataverseMetadataService, SdkDataverseMetadataService>();


var app = builder.Build();
app.AddCommands<DataverseCommands>();
try
{
    await app.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}

