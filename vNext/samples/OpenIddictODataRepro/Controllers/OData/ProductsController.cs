using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using OpenIddictODataRepro.Data.Models;

namespace OpenIddictODataRepro.Controllers.OData
{
    [EnableQuery]
    [Route("odata/Products")]
    public class ProductsController : Controller
    {
        [HttpGet]
        public IQueryable<Product> Get()
        {
            return new []
            {
                new Product(1, "OpenIddict"), 
                new Product(2, "OData vNext"), 
            }.AsQueryable();
        }
    }
}