using System;

namespace Microsoft.AspNetCore.OData.Query
{
	public enum PageSize
	{
		Infinite
	}

	public class ActionPageSize
	{
		public bool IsSet { get; set; }
		public int? Size { get; set; }

		public ActionPageSize() { }

		public ActionPageSize(bool isSet, int? size)
		{
			IsSet = isSet;
			Size = size;
		}
	}
	public class PageSizeAttribute : Attribute
	{
		public virtual int? Value { get; }

		public PageSizeAttribute(PageSize pageSize)
		{

		}

		public PageSizeAttribute(int value)
		{
			Value = value;
		}
	}
}