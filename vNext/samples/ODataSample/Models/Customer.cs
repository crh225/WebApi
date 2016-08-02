using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.OData.Query;

namespace ODataSample.Web.Models
{
	public class Customer : DbObject
	{
		public int Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public List<Product> Products { get; set; }
		public List<Order> Orders { get; set; }
	}
}
