using System;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Builder.Conventions.Attributes
{
    /// <summary>
    /// Includes properties with the ODataIncludeAttribute from <see cref="IEdmStructuredType"/>.
    /// </summary>
    internal class ODataIncludeAttributeConvention : AttributeEdmPropertyConvention<PropertyConfiguration>
    {
        public ODataIncludeAttributeConvention()
            : base(attribute => attribute.GetType() == typeof(ODataIncludeAttribute), allowMultiple: false)
        {
        }

        public override void Apply(PropertyConfiguration edmProperty,
            StructuralTypeConfiguration structuralTypeConfiguration,
            Attribute attribute,
            ODataConventionModelBuilder model)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            if (structuralTypeConfiguration == null)
            {
                throw Error.ArgumentNull("structuralTypeConfiguration");
            }

            //if (!edmProperty.AddedExplicitly)
            //{
                structuralTypeConfiguration.AddProperty(edmProperty.PropertyInfo);
            //}
        }
    }
}