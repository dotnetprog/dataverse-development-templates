using Cocona;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace DataverseTemplate.ConsoleApp.Commands
{
    public class DataverseCommands
    {
        private readonly IOrganizationServiceAsync2 dataserviceClient;
        private readonly ILogger<DataverseCommands> logger;

        public DataverseCommands(IOrganizationServiceAsync2 dataserviceClient,
            ILogger<DataverseCommands> logger)
        {
            this.dataserviceClient = dataserviceClient;
            this.logger = logger;
        }
        [Command("whoami")]
        public async Task WhoAmI()
        {
            logger.LogInformation("Sending WhoAmI Request to dataverse");
            var whoAmiRequest = new WhoAmIRequest();
            var response = (await dataserviceClient.ExecuteAsync(whoAmiRequest)) as WhoAmIResponse;
            logger.LogInformation("UserId: {UserId}", response.UserId);
        }
    }
}
