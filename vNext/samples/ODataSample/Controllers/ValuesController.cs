using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;

namespace ODataSample.Web.Controllers
{
    [EnableQuery]
    [Route("odata/Values")]
    public class ValuesController : Controller
    {
        [HttpGet]
        public virtual async Task<IEnumerable<int>> Get()
        {
            return new[] { 1, 2, 3 };
        }
    }
}