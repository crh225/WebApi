using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.WebApiCompatShim;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData
{
    // TODO: Replace with full version in the future.
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class EnableQueryAttribute : ActionFilterAttribute
    {
        private const char CommaSeparator = ',';

        // validation settings
        private ODataValidationSettings _validationSettings;
        private string _allowedOrderByProperties;

        // query settings
        private ODataQuerySettings _querySettings;

        public EnableQueryAttribute()
        {
            _validationSettings = new ODataValidationSettings();
            _querySettings = 
                new ODataQuerySettings
                {
                    SearchDerivedTypeWhenAutoExpand = true
                };
        }

        /// <summary>
        /// Gets or sets a value indicating whether query composition should
        /// alter the original query when necessary to ensure a stable sort order.
        /// </summary>
        /// <value>A <c>true</c> value indicates the original query should
        /// be modified when necessary to guarantee a stable sort order.
        /// A <c>false</c> value indicates the sort order can be considered
        /// stable without modifying the query.  Query providers that ensure
        /// a stable sort order should set this value to <c>false</c>.
        /// The default value is <c>true</c>.</value>
        public bool EnsureStableOrdering
        {
            get
            {
                return _querySettings.EnsureStableOrdering;
            }
            set
            {
                _querySettings.EnsureStableOrdering = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how null propagation should
        /// be handled during query composition.
        /// </summary>
        /// <value>
        /// The default is <see cref="HandleNullPropagationOption.Default"/>.
        /// </value>
        public HandleNullPropagationOption HandleNullPropagation
        {
            get
            {
                return _querySettings.HandleNullPropagation;
            }
            set
            {
                _querySettings.HandleNullPropagation = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether constants should be parameterized. Parameterizing constants
        /// would result in better performance with Entity framework.
        /// </summary>
        /// <value>The default value is <c>true</c>.</value>
        public bool EnableConstantParameterization
        {
            get
            {
                return _querySettings.EnableConstantParameterization;
            }
            set
            {
                _querySettings.EnableConstantParameterization = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum depth of the Any or All elements nested inside the query. This limit helps prevent
        /// Denial of Service attacks.
        /// </summary>
        /// <value>
        /// The maxiumum depth of the Any or All elements nested inside the query. The default value is 1.
        /// </value>
        public int MaxAnyAllExpressionDepth
        {
            get
            {
                return _validationSettings.MaxAnyAllExpressionDepth;
            }
            set
            {
                _validationSettings.MaxAnyAllExpressionDepth = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of nodes inside the $filter syntax tree.
        /// </summary>
        /// <value>The default value is 100.</value>
        public int MaxNodeCount
        {
            get
            {
                return _validationSettings.MaxNodeCount;
            }
            set
            {
                _validationSettings.MaxNodeCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of query results to send back to clients.
        /// </summary>
        /// <value>
        /// The maximum number of query results to send back to clients.
        /// </value>
        public int PageSize
        {
            get
            {
                return _querySettings.PageSize ?? default(int);
            }
            set
            {
                _querySettings.PageSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the query parameters that are allowed in queries.
        /// </summary>
        /// <value>The default includes all query options: $filter, $skip, $top, $orderby, $expand, $select, $count,
        /// $format, $skiptoken and $deltatoken.</value>
        public AllowedQueryOptions AllowedQueryOptions
        {
            get
            {
                return _validationSettings.AllowedQueryOptions;
            }
            set
            {
                _validationSettings.AllowedQueryOptions = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that represents a list of allowed functions used in the $filter query. Supported
        /// functions include the following:
        /// <list type="definition">
        /// <item>
        /// <term>String related:</term>
        /// <description>substringof, endswith, startswith, length, indexof, substring, tolower, toupper, trim,
        /// concat e.g. ~/Customers?$filter=length(CompanyName) eq 19</description>
        /// </item>
        /// <item>
        /// <term>DateTime related:</term>
        /// <description>year, month, day, hour, minute, second, fractionalseconds, date, time
        /// e.g. ~/Employees?$filter=year(BirthDate) eq 1971</description>
        /// </item>
        /// <item>
        /// <term>Math related:</term>
        /// <description>round, floor, ceiling</description>
        /// </item>
        /// <item>
        /// <term>Type related:</term>
        /// <description>isof, cast</description>
        /// </item>
        /// <item>
        /// <term>Collection related:</term>
        /// <description>any, all</description>
        /// </item>
        /// </list>
        /// </summary>
        public AllowedFunctions AllowedFunctions
        {
            get
            {
                return _validationSettings.AllowedFunctions;
            }
            set
            {
                _validationSettings.AllowedFunctions = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that represents a list of allowed arithmetic operators including 'add', 'sub', 'mul',
        /// 'div', 'mod'.
        /// </summary>
        public AllowedArithmeticOperators AllowedArithmeticOperators
        {
            get
            {
                return _validationSettings.AllowedArithmeticOperators;
            }
            set
            {
                _validationSettings.AllowedArithmeticOperators = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that represents a list of allowed logical Operators such as 'eq', 'ne', 'gt', 'ge',
        /// 'lt', 'le', 'and', 'or', 'not'.
        /// </summary>
        public AllowedLogicalOperators AllowedLogicalOperators
        {
            get
            {
                return _validationSettings.AllowedLogicalOperators;
            }
            set
            {
                _validationSettings.AllowedLogicalOperators = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets a string with comma seperated list of property names. The queryable result can only be
        /// ordered by those properties defined in this list.</para>
        ///
        /// <para>Note, by default this string is null, which means it can be ordered by any property.</para>
        ///
        /// <para>For example, setting this value to null or empty string means that we allow ordering the queryable
        /// result by any properties. Setting this value to "Name" means we only allow queryable result to be ordered
        /// by Name property.</para>
        /// </summary>
        public string AllowedOrderByProperties
        {
            get
            {
                return _allowedOrderByProperties;
            }
            set
            {
                _allowedOrderByProperties = value;

                if (String.IsNullOrEmpty(value))
                {
                    _validationSettings.AllowedOrderByProperties.Clear();
                }
                else
                {
                    // now parse the value and set it to validationSettings
                    string[] properties = _allowedOrderByProperties.Split(CommaSeparator);
                    for (int i = 0; i < properties.Length; i++)
                    {
                        _validationSettings.AllowedOrderByProperties.Add(properties[i].Trim());
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the max value of $skip that a client can request.
        /// </summary>
        public int MaxSkip
        {
            get
            {
                return _validationSettings.MaxSkip ?? default(int);
            }
            set
            {
                _validationSettings.MaxSkip = value;
            }
        }

        /// <summary>
        /// Gets or sets the max value of $top that a client can request.
        /// </summary>
        public int MaxTop
        {
            get
            {
                return _validationSettings.MaxTop ?? default(int);
            }
            set
            {
                _validationSettings.MaxTop = value;
            }
        }

        /// <summary>
        /// Gets or sets the max expansion depth for the $expand query option. To disable the maximum expansion depth
        /// check, set this property to 0.
        /// </summary>
        public int MaxExpansionDepth
        {
            get { return _validationSettings.MaxExpansionDepth; }
            set
            {
                _validationSettings.MaxExpansionDepth = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of expressions that can be present in the $orderby.
        /// </summary>
        public int MaxOrderByNodeCount
        {
            get { return _validationSettings.MaxOrderByNodeCount; }
            set
            {
                _validationSettings.MaxOrderByNodeCount = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to search derived type when finding AutoExpand properties.
        /// </summary>
        public bool SearchDerivedTypeWhenAutoExpand
        {
            get
            {
                return _querySettings.SearchDerivedTypeWhenAutoExpand;
            }
            set
            {
                _querySettings.SearchDerivedTypeWhenAutoExpand = value;
            }
        }

        /// <summary>
        /// Applies the query to the given entity based on incoming query from uri and query settings.
        /// </summary>
        /// <param name="entity">The original entity from the response message.</param>
        /// <param name="queryOptions">
        /// The <see cref="ODataQueryOptions"/> instance constructed based on the incoming request.
        /// </param>
        /// <returns>The new entity after the $select and $expand query has been applied to.</returns>
        public virtual IQueryable ApplyQuery(IQueryable entity, ODataQueryOptions queryOptions)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            if (queryOptions == null)
            {
                throw Error.ArgumentNull("queryOptions");
            }
            // TODO: If we are using SQL, set this to false
            // otherwise if it is entities in code then
            // set it to true
            _querySettings.HandleNullPropagation =
                //HandleNullPropagationOption.True
                HandleNullPropagationOptionHelper.GetDefaultHandleNullPropagationOption(entity);
                    //PageSize = actionDescriptor.PageSize(),

            return queryOptions.ApplyTo(entity, _querySettings);
        }

        /// <summary>
        /// Applies the query to the given entity based on incoming query from uri and query settings.
        /// </summary>
        /// <param name="entity">The original entity from the response message.</param>
        /// <param name="queryOptions">
        /// The <see cref="ODataQueryOptions"/> instance constructed based on the incoming request.
        /// </param>
        /// <returns>The new entity after the $select and $expand query has been applied to.</returns>
        public virtual object ApplyQuery(object entity, ODataQueryOptions queryOptions)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            if (queryOptions == null)
            {
                throw Error.ArgumentNull("queryOptions");
            }

            return queryOptions.ApplyTo(entity, _querySettings);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            var response = context.HttpContext.Response;
            if (!response.IsSuccessStatusCode())
            {
                return;
            }

            var request = context.HttpContext.Request;

            var result = context.Result as ObjectResult;

            if (result == null)
            {
                throw Error.Argument("context", SRResources.QueryingRequiresObjectContent,
                    context.Result.GetType().FullName);
            }

            bool shouldApplyQuery =
                request.GetDisplayUrl() != null &&
                (!String.IsNullOrWhiteSpace(new Uri(request.GetDisplayUrl()).Query) ||
                _querySettings.PageSize.HasValue ||
                result.Value is SingleResult ||
                ODataCountMediaTypeMapping.IsCountRequest(request) ||
                ContainsAutoExpandProperty(result.Value, request, context.ActionDescriptor));

            if (shouldApplyQuery)
            {
                if (result.Value != null)
                {
                    result.Value = ApplyQueryOptions(result.Value, request, context.ActionDescriptor);
                }
            }
            //if (ShouldBeSingleEntity(request.ODataProperties().Path.PathTemplate))
            //{
            //    var queryable = result.Value as IQueryable;
            //    if (queryable != null)
            //    {
            //        result.Value = SingleOrDefault(queryable, context.ActionDescriptor);
            //    }
            //}
        }

        public virtual object ApplyQueryOptions(object value, HttpRequest request, ActionDescriptor actionDescriptor)
        {
            var elementClrType = value is IEnumerable
                ? TypeHelper.GetImplementedIEnumerableType(value.GetType())
                : value.GetType();

            var model = request.ODataProperties().Model;
            if (model == null)
            {
                throw Error.InvalidOperation(SRResources.QueryGetModelMustNotReturnNull);
            }

            var assembliesResolver = request.HttpContext.RequestServices.GetService<AssembliesResolver>();
            var queryContext = new ODataQueryContext(
                model,
                elementClrType,
                assembliesResolver,
                request.ODataProperties().Path
                );

            var queryOptions = new ODataQueryOptions(queryContext, request, assembliesResolver);
            _querySettings.PageSize = _querySettings.PageSize ?? actionDescriptor.PageSize();

            ValidateQuery(request, queryOptions);

            var enumerable = value as IEnumerable;
            if (enumerable == null)
            {
                // response is not a collection; we only support $select and $expand on single entities.
                ValidateSelectExpandOnly(queryOptions);

                SingleResult singleResult = value as SingleResult;
                if (singleResult == null)
                {
                    // response is a single entity.
                    return ApplyQuery(entity: value, queryOptions: queryOptions);
                }
                else
                {
                    // response is a composable SingleResult. ApplyQuery and call SingleOrDefault.
                    IQueryable queryable = singleResult.Queryable;
                    queryable = ApplyQuery(queryable, queryOptions);
                    return SingleOrDefault(queryable, actionDescriptor);
                }
            }
            else
            {
                // response is a collection.
                var entries = enumerable as object[] ?? enumerable.Cast<object>().ToArray();
                IQueryable queryable = (enumerable as IQueryable) ?? entries.AsQueryable();
                return ApplyQuery(queryable, queryOptions);
            }

            // response is a collection.
            //var query = value as IQueryable ?? enumerable.AsQueryable();

            //query = queryOptions.ApplyTo(query,
            //    new ODataQuerySettings
            //    {
            //        // TODO: If we are using SQL, set this to false
            //        // otherwise if it is entities in code then
            //        // set it to true
            //        HandleNullPropagation =
            //            //HandleNullPropagationOption.True
            //            HandleNullPropagationOptionHelper.GetDefaultHandleNullPropagationOption(query),
            //        PageSize = actionDescriptor.PageSize(),
            //        SearchDerivedTypeWhenAutoExpand = true
            //    },
            //    AllowedQueryOptions.None);
            //// Determine if this result should be a single entity

            //if (ODataCountMediaTypeMapping.IsCountRequest(request))
            //{
            //    var count = request.ODataProperties().TotalCount;

            //    if (count.HasValue)
            //    {
            //        // Return the count value if it is a $count request.
            //        return count.Value;
            //    }
            //}
            //return query;
        }

        /// <summary>
        ///     Validates the OData query in the incoming request. By default, the implementation throws an exception if
        ///     the query contains unsupported query parameters. Override this method to perform additional validation of
        ///     the query.
        /// </summary>
        /// <param name="request">The incoming request.</param>
        /// <param name="queryOptions">
        ///     The <see cref="ODataQueryOptions" /> instance constructed based on the incoming request.
        /// </param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "Response disposed after being sent.")]
        public virtual void ValidateQuery(HttpRequest request, ODataQueryOptions queryOptions)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (queryOptions == null)
            {
                throw Error.ArgumentNull("queryOptions");
            }

            var httpRequestMessage = request.HttpContext.GetHttpRequestMessage();
            foreach (var kvp in request.Query)
            {
                if (!queryOptions.IsSupportedQueryOption(kvp.Key) &&
                    kvp.Key.StartsWith("$", StringComparison.Ordinal))
                {
                    // we don't support any custom query options that start with $
                    throw new HttpResponseException(httpRequestMessage.CreateErrorResponse(HttpStatusCode.BadRequest,
                        Error.Format(SRResources.QueryParameterNotSupported, kvp.Key)));
                }
            }

            queryOptions.Validate(_validationSettings);
        }

        private bool ShouldBeSingleEntity(ODataQueryOptions queryOptions)
        {
            return ShouldBeSingleEntity(queryOptions.Context.Path.PathTemplate);
        }

        private bool ShouldBeSingleEntity(string pathTemplate)
        {
            return pathTemplate == "~/entityset/key";
        }

        internal static object SingleOrDefault(IQueryable queryable, ActionDescriptor actionDescriptor)
        {
            var enumerator = queryable.GetEnumerator();
            try
            {
                var result = enumerator.MoveNext() ? enumerator.Current : null;

                if (enumerator.MoveNext())
                {
                    throw new InvalidOperationException(Error.Format(
                        SRResources.SingleResultHasMoreThanOneEntity));
                }

                return result;
            }
            finally
            {
                // Fix for Issue #2097
                // Ensure any active/open database objects that were created
                // iterating over the IQueryable object are properly closed.
                var disposable = enumerator as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        internal static Type GetElementType(object response, ActionDescriptor actionDescriptor)
        {
            Contract.Assert(response != null);

            IEnumerable enumerable = response as IEnumerable;
            if (enumerable == null)
            {
                SingleResult singleResult = response as SingleResult;
                if (singleResult == null)
                {
                    return response.GetType();
                }

                enumerable = singleResult.Queryable;
            }

            Type elementClrType = TypeHelper.GetImplementedIEnumerableType(enumerable.GetType());
            if (elementClrType == null)
            {
                // The element type cannot be determined because the type of the content
                // is not IEnumerable<T> or IQueryable<T>.
                throw Error.InvalidOperation(
                    SRResources.FailedToRetrieveTypeToBuildEdmModel,
                    typeof(EnableQueryAttribute).Name,
                    actionDescriptor.Name,
                    "Unknown controller",
                    response.GetType().FullName);
            }

            return elementClrType;
        }

        /// <summary>
        /// Gets the EDM model for the given type and request. Override this method to customize the EDM model used for
        /// querying.
        /// </summary>
        /// <param name="elementClrType">The CLR type to retrieve a model for.</param>
        /// <param name="request">The request message to retrieve a model for.</param>
        /// <param name="actionDescriptor">The action descriptor for the action being queried on.</param>
        /// <returns>The EDM model for the given type and request.</returns>
        public virtual IEdmModel GetModel(Type elementClrType, HttpRequest request,
            ActionDescriptor actionDescriptor)
        {
            // Get model for the request
            IEdmModel model = request.ODataProperties().Model;

            if (model?.GetEdmType(elementClrType) == null)
            {
                // user has not configured anything or has registered a model without the element type
                // let's create one just for this type and cache it in the action descriptor
                model = actionDescriptor.GetEdmModel(elementClrType, request.HttpContext.RequestServices.GetRequiredService<AssembliesResolver>());
            }

            Contract.Assert(model != null);
            return model;
        }

        private bool ContainsAutoExpandProperty(object response, HttpRequest request, ActionDescriptor actionDescriptor)
        {
            Type elementClrType = GetElementType(response, actionDescriptor);

            IEdmModel model = GetModel(elementClrType, request, actionDescriptor);
            if (model == null)
            {
                throw Error.InvalidOperation(SRResources.QueryGetModelMustNotReturnNull);
            }
            IEdmEntityType baseEntityType = model.GetEdmType(elementClrType) as IEdmEntityType;
            List<IEdmEntityType> entityTypes = new List<IEdmEntityType>();
            if (baseEntityType != null)
            {
                entityTypes.Add(baseEntityType);
                if (SearchDerivedTypeWhenAutoExpand)
                {
                    entityTypes.AddRange(EdmLibHelpers.GetAllDerivedEntityTypes(baseEntityType, model));
                }

                foreach (var entityType in entityTypes)
                {
                    var navigationProperties = entityType == baseEntityType
                        ? entityType.NavigationProperties()
                        : entityType.DeclaredNavigationProperties();
                    if (navigationProperties != null)
                    {
                        foreach (var navigationProperty in navigationProperties)
                        {
                            if (EdmLibHelpers.IsAutoExpand(navigationProperty, model))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        internal static void ValidateSelectExpandOnly(ODataQueryOptions queryOptions)
        {
            if (queryOptions.Filter != null || queryOptions.Count != null || queryOptions.OrderBy != null
                || queryOptions.Skip != null || queryOptions.Top != null)
            {
                throw new ODataException(Error.Format(SRResources.NonSelectExpandOnSingleEntity));
            }
        }
    }
}