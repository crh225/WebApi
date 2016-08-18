using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using ODataSample.Web.Models;

namespace ODataSample.Web.Controllers
{
	[EnableQuery]
	//[HazAuthorize(Policy2 = "Super")]
	[Route("odata/Clients")]
	public class ClientsController : Controller
	{
		[HttpGet]
		public IQueryable<Client> Get()
		{
			return new Client[] { }.AsQueryable();
		}
	}
}