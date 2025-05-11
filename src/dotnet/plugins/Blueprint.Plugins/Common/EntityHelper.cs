using Microsoft.Xrm.Sdk;
using System.Collections.Generic;

namespace Blueprint.Plugins.Common
{
    internal static class EntityHelper
    {
        public static T ReadAttribute<T>(string attributeName, Entity target, Entity preImage = null)
        {
            var attrNameLowerCase = attributeName.ToLower();
            if (preImage == null)
            {
                return target.GetAttributeValue<T>(attrNameLowerCase);
            }
            return target.Contains(attrNameLowerCase) ? target.GetAttributeValue<T>(attrNameLowerCase) : preImage.GetAttributeValue<T>(attrNameLowerCase);
        }
        public static bool FieldHasChanged<T>(string attributeName, Entity target, Entity preImage)
        {
            attributeName = attributeName?.ToLower();
            if (!target.Contains(attributeName))
                return false;

            var tType = typeof(T);
            if (tType == typeof(Money))
            {
                var mv1 = target.GetAttributeValue<Money>(attributeName);
                var mv2 = preImage?.GetAttributeValue<Money>(attributeName);
                return mv1?.Value != mv2?.Value;
            }
            if (tType == typeof(OptionSetValue))
            {
                var ov1 = target.GetAttributeValue<OptionSetValue>(attributeName);
                var ov2 = preImage?.GetAttributeValue<OptionSetValue>(attributeName);
                return ov1?.Value != ov2?.Value;
            }
            if (tType == typeof(EntityReference))
            {
                var ev1 = target.GetAttributeValue<EntityReference>(attributeName);
                var ev2 = preImage?.GetAttributeValue<EntityReference>(attributeName);
                return ev1?.Id != ev2?.Id;
            }
            var v1 = target.GetAttributeValue<T>(attributeName);
            if (preImage == null)
            {
                return v1 != null;
            }
            var v2 = preImage.GetAttributeValue<T>(attributeName);
            return !EqualityComparer<T>.Default.Equals(v1, v2);
        }
    }
}
