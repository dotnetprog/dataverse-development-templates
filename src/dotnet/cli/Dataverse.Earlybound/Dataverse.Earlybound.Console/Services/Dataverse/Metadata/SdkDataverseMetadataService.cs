using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Dataverse.Earlybound.Console.Services.Dataverse.Metadata
{
    public class SdkDataverseMetadataService : IDataverseMetadataService
    {
        private readonly IOrganizationServiceAsync2 _dataverseClient;

        public SdkDataverseMetadataService(IOrganizationServiceAsync2 dataverseClient)
        {
            _dataverseClient = dataverseClient;
        }

        public async Task<IReadOnlyCollection<EntityMetadata>> GetEntitiesMetadataByPrefixAsync(string prefix, CancellationToken cancellationToken)
        {
            var request = new RetrieveAllEntitiesRequest()
            {
                EntityFilters = EntityFilters.Entity
            };
            var response = (await _dataverseClient.ExecuteAsync(request, cancellationToken)) as RetrieveAllEntitiesResponse;
            return response.EntityMetadata.Where(e => e.LogicalName.StartsWith(prefix)).ToArray();
        }
    }
}
