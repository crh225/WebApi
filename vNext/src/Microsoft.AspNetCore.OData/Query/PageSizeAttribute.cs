using System;

namespace Microsoft.AspNetCore.OData.Query
{
	public class PageSizeAttribute : Attribute
	{
		public virtual int? Value { get; }

		public PageSizeAttribute(int value)
		{
			Value = value;
		}
	}
}