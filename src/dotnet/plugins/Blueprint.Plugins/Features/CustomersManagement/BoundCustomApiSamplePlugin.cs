using Blueprint.Plugins.Features.CustomersManagement.BusinessRules;
using Blueprint.Plugins.Platform;

namespace Blueprint.Plugins.Features.CustomersManagement
{
    public class BoundCustomApiSamplePlugin : PluginBase
    {
        //if custom api is bounded to an entity type, empty string should be provided to ctor base class.
        public BoundCustomApiSamplePlugin() : base("contact")
        {
            //This is necessary so that the base class knows about the ExecuteCustomApi Method.
            //It basically tells to trigger the function on core operation of your custom api
            //Don't be afrais to edit the Message Enum to add your custom api names. It's case sensitive.
            this.AddStep("contact", Stage.CoreOperation, Message.sample_customapiname, ExecutionMode.Sync, ExecuteCustomApi);
        }


        private void ExecuteCustomApi(ILocalPluginContext context)
        {
            ApplyBusinessRules(context, new SampleBusinessRule());
        }


    }
}
