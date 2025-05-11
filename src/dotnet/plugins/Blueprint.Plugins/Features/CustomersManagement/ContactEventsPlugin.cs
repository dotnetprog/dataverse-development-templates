using Blueprint.Plugins.Features.CustomersManagement.BusinessRules;

namespace Blueprint.Plugins.Features.CustomersManagement
{
    public class ContactEventsPlugin : PluginBase
    {
        public ContactEventsPlugin() : base("contact")
        {
        }


        protected override void ExecutePreValidateCreate(ILocalPluginContext localPluginContext)
        {
            ApplyBusinessRules(localPluginContext, new EnsureContactEmailAddressIsNotEmptyBusinessRule());
        }
        protected override void ExecutePreValidateUpdate(ILocalPluginContext localPluginContext)
        {
            ApplyBusinessRules(localPluginContext, new EnsureContactEmailAddressIsNotEmptyBusinessRule());
        }



    }
}
