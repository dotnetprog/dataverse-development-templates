using Blueprint.Plugins.Common;
using Blueprint.Plugins.Platform;
using Microsoft.Xrm.Sdk;

namespace Blueprint.Plugins.Features.CustomersManagement.BusinessRules
{
    public class EnsureContactEmailAddressIsNotEmptyBusinessRule : IBusinessRule
    {
        public void Apply(ILocalPluginContext localContext)
        {

            var target = localContext.GetTarget();
            var preImage = localContext.GetPreImage();

            //Do not need to execute the business rule if field hasnt changed
            if (!EntityHelper.FieldHasChanged<string>("emailaddress", target, preImage))
            {
                return;
            }
            var currentEmailAddress = target.GetAttributeValue<string>("emailaddress");

            if (!string.IsNullOrWhiteSpace(currentEmailAddress)) { return; }

            throw new InvalidPluginExecutionException("Email address is mandatory.");


        }
    }
}
