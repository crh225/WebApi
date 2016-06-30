using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.OData.Routing
{
    public class ODataActionSelector : IActionSelector
    {
        private readonly IActionSelector _selector;
        private readonly IODataRoutingConvention _convention;

        public ODataActionSelector(IODataRoutingConvention convention,
            IActionDescriptorCollectionProvider actionDescriptorsCollectionProvider,
            IActionSelectorDecisionTreeProvider decisionTreeProvider,
            IEnumerable<IActionConstraintProvider> actionConstraintProviders,
            ILoggerFactory loggerFactory)
        {

            _selector = new ActionSelector(decisionTreeProvider,
                new ActionConstraintCache(actionDescriptorsCollectionProvider, actionConstraintProviders),
                loggerFactory);
            _convention = convention;
        }

        public bool HasValidAction(VirtualPathContext context)
        {
            return true;
        }

        public async Task<ActionDescriptor> SelectAsync(RouteContext context)
        {
            return await Task.FromResult(Select(context));
        }

        public ActionDescriptor Select(RouteContext context)
        {
            return SelectCandidates(context).First();
        }

        public IReadOnlyList<ActionDescriptor> SelectCandidates(RouteContext context)
        {
            if (context.HttpContext.ODataProperties().IsValidODataRequest)
            {
                return AsCollection(_convention.SelectAction(context));
            }
            return _selector.SelectCandidates(context);
        }

        private static IReadOnlyList<ActionDescriptor> AsCollection(ActionDescriptor actionDescriptor)
        {
            return new ReadOnlyCollection<ActionDescriptor>(new[] { actionDescriptor });
        }

        public ActionDescriptor SelectBestCandidate(RouteContext context, IReadOnlyList<ActionDescriptor> candidates)
        {
            //return _selector.SelectBestCandidate(context, candidates);
            return _convention.SelectAction(context);
        }
    }
}
