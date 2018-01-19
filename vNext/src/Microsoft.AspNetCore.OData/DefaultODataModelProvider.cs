using System;
using System.Reflection;
using Microsoft.AspNetCore.OData.Builder;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Reflection;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData
{
    internal class DefaultODataModelProvider
    {
        public static IEdmModel BuildEdmModel(Type apiContextType, System.Collections.Generic.List<Type> extraTypes, AssembliesResolver assembliesResolver, Action<ODataConventionModelBuilder> after = null)
        {
            var builder = new ODataConventionModelBuilder(assembliesResolver)
            {
                Namespace = apiContextType.Namespace
            };
            
            var publicProperties = apiContextType.GetPropertiesInternal(BindingFlagsInternal.Public | BindingFlagsInternal.Instance);
            foreach (var property in publicProperties)
            {
                var entityClrType = TypeHelper.GetImplementedIEnumerableType(property.PropertyType);
                var entity = builder.AddEntityType(entityClrType);
                builder.AddEntitySet(property.Name, entity);
            }

            if (extraTypes != null)
            {
                foreach (Type extraType in extraTypes)
                {
                    publicProperties = extraType.GetPropertiesInternal(BindingFlagsInternal.Public | BindingFlagsInternal.Instance);
                    foreach (var property in publicProperties)
                    {
                        var entityClrType = TypeHelper.GetImplementedIEnumerableType(property.PropertyType);
                        var entity = builder.AddEntityType(entityClrType);
                        builder.AddEntitySet(property.Name, entity);
                    }
                }
            }

            after?.Invoke(builder);
			var edmModel = builder.GetEdmModel();            
            return edmModel;
		}
    }
}
