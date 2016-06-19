using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.OData.Query;

namespace ODataSample.Web.Models.Interceptors
{
    public class ProductInterceptor : IODataQueryInterceptor<Product>
    {
        public Expression<Func<Product, bool>> Intercept(ODataQuerySettings querySettings, ODataQueryOptions queryOptions)
        {
            return q => q.Name.Contains("1");
        }
    }
}