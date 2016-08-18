using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OData.Builder;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Extensions
{
	public static class ActionDescriptorExtensions
	{
        /// <summary>Adds a key/value pair to the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> by using the specified function, if the key does not already exist.</summary>
        /// <returns>The value for the key. This will be either the existing value for the key if the key is already in the dictionary, or the new value for the key as returned by valueFactory if the key was not in the dictionary.</returns>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> or <paramref name="valueFactory" /> is null.</exception>
        /// <exception cref="T:System.OverflowException">The dictionary already contains the maximum number of elements (<see cref="F:System.Int32.MaxValue" />).</exception>
        private static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, Func<TKey, TValue> valueFactory)
        {
            if ((object)key == null)
                throw new ArgumentNullException("key");
            if (valueFactory == null)
                throw new ArgumentNullException("valueFactory");
            TValue resultingValue;
            if (dic.TryGetValue(key, out resultingValue))
                return resultingValue;
            resultingValue = valueFactory(key);
            dic.Add(key, resultingValue);
            return resultingValue;
        }

        // Maintain the System.Web.OData. prefix in any new properties to avoid conflicts with user properties
        // and those of the v3 assembly.  Concern is reduced here due to addition of user type name but prefix
        // also clearly ties the property to code in this assembly.
        private const string ModelKeyPrefix = "System.Web.OData.Model+";

        internal static IEdmModel GetEdmModel(this ActionDescriptor actionDescriptor,
            Type entityClrType,
            AssembliesResolver assembliesResolver)
        {
            if (actionDescriptor == null)
            {
                throw Error.ArgumentNull("actionDescriptor");
            }

            if (entityClrType == null)
            {
                throw Error.ArgumentNull("entityClrType");
            }

            // save the EdmModel to the action descriptor
            return actionDescriptor.Properties.GetOrAdd(ModelKeyPrefix + entityClrType.FullName, _ =>
            {
                ODataConventionModelBuilder builder =
                    new ODataConventionModelBuilder(assembliesResolver, isQueryCompositionMode: true);
                EntityTypeConfiguration entityTypeConfiguration = builder.AddEntityType(entityClrType);
                builder.AddEntitySet(entityClrType.Name, entityTypeConfiguration);
                IEdmModel edmModel = builder.GetEdmModel();
                Contract.Assert(edmModel != null);
                return edmModel;
            }) as IEdmModel;
        }

        public static bool HasQueryOption(this ActionDescriptor actionDescriptor)
		{
			return actionDescriptor.PageSize().IsSet;
		}

		public static ActionPageSize PageSize(this ActionDescriptor actionDescriptor)
		{
			var controllerActionDescriptor = actionDescriptor as ControllerActionDescriptor;
			var pageSizeAttribute = controllerActionDescriptor?.MethodInfo.GetCustomAttribute<PageSizeAttribute>();
			var actionPageSize = new ActionPageSize();
			if (pageSizeAttribute != null)
			{
				actionPageSize.IsSet = true;
				actionPageSize.Size = pageSizeAttribute.Value;
			}
			return actionPageSize;
		}
	}
}