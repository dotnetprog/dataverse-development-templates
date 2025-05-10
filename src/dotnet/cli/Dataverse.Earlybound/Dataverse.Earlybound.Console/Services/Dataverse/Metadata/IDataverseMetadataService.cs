using Microsoft.Xrm.Sdk.Metadata;

namespace Dataverse.Earlybound.Console.Services.Dataverse.Metadata
{
    public interface IDataverseMetadataService
    {
        Task<IReadOnlyCollection<EntityMetadata>> GetEntitiesMetadataByPrefixAsync(string prefix, CancellationToken cancellationToken);
    }
}
