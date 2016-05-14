using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query;

namespace ODataSample.Web.OData
{
    public class SecuredQueryAttribute : EnableQueryAttribute
    {
        //public override void ValidateQuery(HttpRequest request, ODataQueryOptions queryOptions)
        //{
        //    if (queryOptions.SelectExpand != null
        //                && queryOptions.SelectExpand.RawExpand != null
        //                && queryOptions.SelectExpand.RawExpand.Contains("Customer"))
        //    {
        //        //queryOptions.Filter.RawValue = "CustomerId eq 1";
        //        //throw new InvalidOperationException();
        //    }

        //    base.ValidateQuery(request, queryOptions);
        //}
    }
}