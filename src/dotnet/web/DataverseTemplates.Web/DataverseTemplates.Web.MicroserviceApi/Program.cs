using DataverseTemplates.Web.MicroserviceApi.Services.Dataverse;
using DataverseTemplates.Web.MicroserviceApi.Services.Dataverse.Configuration;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization()
    .Configure<SdkDataverseServiceFactoryOptions>(builder.Configuration.GetSection("Dataverse"))
    .AddDataverseClient(ServiceLifetime.Scoped);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly());
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGet("/whoami", async (IOrganizationServiceAsync2 crmService, CancellationToken cancellationtoken) =>
{
    var request = new WhoAmIRequest();
    var response = await crmService.ExecuteAsync(request, cancellationtoken) as WhoAmIResponse;
    return response;
});

app.Run();
