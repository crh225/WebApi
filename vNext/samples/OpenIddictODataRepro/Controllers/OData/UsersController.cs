using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using OpenIddictODataRepro.Data.Models;

namespace OpenIddictODataRepro.Controllers.OData
{
    [EnableQuery]
    [Route("odata/Users")]
    public class UsersController : Controller
    {
        [HttpGet]
        public IQueryable<ApplicationUser> Get()
        {
            return new ApplicationUser[] { }.AsQueryable();
        }
    }
}