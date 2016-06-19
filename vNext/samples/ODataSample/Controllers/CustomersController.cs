using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using ODataSample.Web.Models;
using ODataSample.Web.OData;

namespace ODataSample.Web.Controllers
{
    [SecuredQuery]
	[Route("odata/Customers")]
	public class CustomersController : ODataCrudController<Customer, int>
	{
		private readonly ISampleService _sampleService;

		public CustomersController(IEdmModel model, ISampleService sampleService) : base(
			model,
			new CrudBase<Customer, int>(sampleService as DbContext,
				(sampleService as ApplicationDbContext).Customers,
				customer => customer.Id))
		{
			_sampleService = sampleService;
		}

		[HttpGet("{id}/FirstName")]
		public IActionResult GetFirstName(int id)
		{
			var customer = _sampleService.FindCustomer(id);
			if (customer == null)
			{
				return NotFound();
			}

			return new ObjectResult(customer.FirstName);
		}

		[HttpGet("{id}/Orders")]
		public IQueryable<Order> Orders(int id)
		{
			return _sampleService.Orders.Where(o => o.CustomerId == id);
		}

		[HttpGet("{id}/LastName")]
		public IActionResult GetLastName(int id)
		{
			var customer = _sampleService.FindCustomer(id);
			if (customer == null)
			{
				return NotFound();
			}

			return new ObjectResult(customer.LastName);
		}

		[HttpGet("{id}/CustomerId")]
		public IActionResult GetCustomerId(int id)
		{
			var customer = _sampleService.FindCustomer(id);
			if (customer == null)
			{
				return NotFound();
			}

			return new ObjectResult(customer.Id);
		}

		[HttpGet("{id}/Products")]
		public IActionResult GetProducts(int id)
		{
			var customer = _sampleService.FindCustomer(id);
			if (customer == null)
			{
				return NotFound();
			}

			return new ObjectResult(customer.Products);
		}
	}
}