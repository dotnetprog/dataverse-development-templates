using Cocona;
using Dataverse.Earlybound.Console.Services.Dataverse.Metadata;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dataverse.Earlybound.Console.Commands
{
    public class DataverseCommands
    {
        private readonly IDataverseMetadataService metadataService;
        private readonly ILogger<DataverseCommands> logger;

        public DataverseCommands(IDataverseMetadataService metadataService,
            ILogger<DataverseCommands> logger)
        {
            this.metadataService = metadataService;
            this.logger = logger;
        }
        [Command("updateBuilderSettings")]
        public async Task UpdateBuilderSettings([Option("file")] string filepath,
            [Option("prefixes")] string prefixesArg,
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Updating BuilderSettings config file");
            if (!File.Exists(filepath))
            {
                throw new ArgumentException($"file does not exist: {filepath}", "file");
            }
            var prefixes = prefixesArg.Split(',');
            var tasks = prefixes.Select(prefix => metadataService.GetEntitiesMetadataByPrefixAsync(prefix, cancellationToken));
            var allCustomEntities = await Task.WhenAll(tasks);
            var entitiesfromDataverse = allCustomEntities.SelectMany(l => l.Select(e => e.LogicalName)).ToArray();
            logger.LogInformation("Found {entitycount} entities from dataverse", entitiesfromDataverse.Length);

            var json = File.ReadAllText(filepath);
            var builderSettings = JObject.Parse(json);
            var currententitylist = JsonConvert.DeserializeObject<List<string>>(builderSettings.GetValue("entityNamesFilter").ToString());

            var entitiesNotInFile = entitiesfromDataverse.Where(e => !currententitylist.Contains(e)).ToArray();
            logger.LogInformation("Found {entitiesNotInFile} entities to add in builderSettings configuration file.", entitiesNotInFile.Length);
            if (entitiesNotInFile.Length == 0)
            {
                return;
            }

            currententitylist.AddRange(entitiesNotInFile);

            builderSettings["entityNamesFilter"] = JArray.FromObject(currententitylist.Distinct().Order().ToList());
            File.WriteAllText(filepath, builderSettings.ToString());
        }
    }
}
