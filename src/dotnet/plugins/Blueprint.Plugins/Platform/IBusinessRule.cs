namespace Blueprint.Plugins.Platform
{
    public interface IBusinessRule
    {
        void Apply(ILocalPluginContext localContext);
    }
}
