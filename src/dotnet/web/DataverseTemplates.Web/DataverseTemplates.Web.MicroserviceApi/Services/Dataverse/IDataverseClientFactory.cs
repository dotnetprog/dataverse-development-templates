using Microsoft.PowerPlatform.Dataverse.Client;

namespace DataverseTemplates.Web.MicroserviceApi.Services.Dataverse;

public interface IDataverseClientFactory
{

    IOrganizationServiceAsync2 Create();

}
