using System;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// Represents an <see cref="Attribute"/> that can be placed on a property to specify that 
    /// the property be included in results.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ODataIncludeAttribute : Attribute
    {
    }
}