using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blueprint.Plugins.Services.EnvironmentVariable
{
    public interface ICustomEnvironmentVariableService
    {
        IDictionary<string, string> GetAll();
        string Get(string variableName);
    }
}
