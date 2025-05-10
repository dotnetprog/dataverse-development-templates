using Microsoft.PowerPlatform.Dataverse.Client;

namespace DataverseTemplate.ConsoleApp.Services.Dataverse;

public interface IDataverseClientFactory
{

    IOrganizationServiceAsync2 Create();

}
