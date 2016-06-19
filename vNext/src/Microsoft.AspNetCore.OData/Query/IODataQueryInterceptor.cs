using System;
using System.Linq.Expressions;

namespace Microsoft.AspNetCore.OData.Query
{
    public interface IODataQueryInterceptor<T>
    {
        Expression<Func<T, bool>> Intercept(ODataQuerySettings querySettings, ODataQueryOptions queryOptions);
    }
}