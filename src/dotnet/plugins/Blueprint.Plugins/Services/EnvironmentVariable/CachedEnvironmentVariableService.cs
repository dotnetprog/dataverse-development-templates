using System.Collections.Generic;

namespace Blueprint.Plugins.Services.EnvironmentVariable
{
    public class CachedEnvironmentVariableService : ICustomEnvironmentVariableService
    {
        private readonly ICustomEnvironmentVariableService _variableService;
        private IDictionary<string, string> cachedVariables { get; set; }
        private IDictionary<string, string> cachedAllVariables { get; set; }
        public CachedEnvironmentVariableService(ICustomEnvironmentVariableService variableService)
        {

            this._variableService = variableService;
            cachedVariables = new Dictionary<string, string>();
            cachedAllVariables = new Dictionary<string, string>();
        }

        public string Get(string variableName)
        {
            if (this.cachedVariables.ContainsKey(variableName))
            {
                return this.cachedVariables[variableName];
            }

            var value = this._variableService.Get(variableName);
            if (string.IsNullOrWhiteSpace(value))
                return null;

            this.cachedVariables[variableName] = value;
            return value;
        }

        public IDictionary<string, string> GetAll()
        {
            if (cachedAllVariables.Count > 0)
            {
                return cachedAllVariables;
            }

            var values = this._variableService.GetAll();

            cachedVariables = values;
            cachedAllVariables = values;

            return values;

        }
    }
}
