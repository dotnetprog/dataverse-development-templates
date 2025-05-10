using Microsoft.PowerPlatform.Dataverse.Client;

namespace Dataverse.Earlybound.Console.Services.Dataverse;

public interface IDataverseClientFactory
{

    IOrganizationServiceAsync2 Create();

}
