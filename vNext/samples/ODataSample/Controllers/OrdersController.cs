using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using ODataSample.Web.Models;

namespace ODataSample.Web.Controllers
{
	[EnableQuery]
	[Route("odata/Orders")]
	public class OrdersController : ODataCrudController<Order, int>
	{
		public OrdersController(IEdmModel model, ISampleService sampleService) : base(
			model,
			new CrudBase<Order, int>(sampleService as DbContext, (sampleService as ApplicationDbContext).Orders,
				entity => entity.Id)
			)
		{
		}

		[HttpGet("{id}/" + nameof(DuplicateMethodName))]
		public async Task<IActionResult> DuplicateMethodName(int id)
		{
			return Ok(await Crud.All().Include(o => o.Customer).ToListAsync());
		}

		[HttpGet(nameof(Expanded))]
		[EnableQuery]
		public async Task<IActionResult> Expanded()
		{
			var orders = Crud.All()
				.Include(c => c.Customer);
			return Ok(orders);
		}
	}
}