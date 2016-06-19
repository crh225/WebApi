using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.OData.Query;

namespace ODataSample.Web.Models.Interceptors
{
    public class CustomerInterceptor : IODataQueryInterceptor<Customer>
    {
        public Expression<Func<Customer, bool>> Intercept(ODataQuerySettings querySettings, ODataQueryOptions queryOptions)
        {
            return q => q.LastName.Contains("Lawd");
        }
    }
}