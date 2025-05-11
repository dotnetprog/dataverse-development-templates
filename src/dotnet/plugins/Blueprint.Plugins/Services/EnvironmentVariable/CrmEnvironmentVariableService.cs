using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Blueprint.Plugins.Services.EnvironmentVariable
{


    public class CrmEnvironmentVariableService : ICustomEnvironmentVariableService
    {
        private readonly IOrganizationService _organizationService;
        public CrmEnvironmentVariableService(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }
        public string Get(string variableName)
        {
            using (var svcContext = new OrganizationServiceContext(this._organizationService))
            {
                var variable = (from ev in svcContext.CreateQuery("environmentvariablevalue")
                                join ed in svcContext.CreateQuery("environmentvariabledefinition")
                                on ev.GetAttributeValue<EntityReference>("environmentvariabledefinitionid").Id equals ed.GetAttributeValue<Guid?>("environmentvariabledefinitionid").Value
                                where ed.GetAttributeValue<string>("schemaname") == variableName
                                select ev).FirstOrDefault();
                return variable?.GetAttributeValue<string>("value");
            }
        }

        public IDictionary<string, string> GetAll()
        {
            using (var svcContext = new OrganizationServiceContext(this._organizationService))
            {
                var variables = (from ev in svcContext.CreateQuery("environmentvariablevalue")
                                 join ed in svcContext.CreateQuery("environmentvariabledefinition")
                                 on ev.GetAttributeValue<EntityReference>("environmentvariabledefinitionid").Id equals ed.GetAttributeValue<Guid?>("environmentvariabledefinitionid").Value
                                 select new { name = ed.GetAttributeValue<string>("schemaname"), value = ev.GetAttributeValue<string>("value"), defaultvalue = ed.GetAttributeValue<string>("defaultvalue") }).ToList();

                var dict = new Dictionary<string, string>();
                foreach (var variable in variables)
                {
                    dict[variable.name] = string.IsNullOrWhiteSpace(variable.value) ? variable.defaultvalue : variable.value;
                }

                return dict;

            }
        }
    }
}
