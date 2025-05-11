using System.Collections.Generic;

namespace Blueprint.Plugins.Platform
{
    public static class CollectionExtensions
    {
        public static void Register<T>(this ICollection<T> collection, string entityName, params Step[] values) where T : Step
        {
            foreach (var step in values)
            {
                if (string.IsNullOrEmpty(step.EntityName))
                {
                    step.EntityName = entityName;
                }
                collection.Add((T)step);
            }
        }
    }
}
