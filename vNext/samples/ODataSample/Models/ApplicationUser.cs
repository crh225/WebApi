using System.Collections.Generic;
using OpenIddict;

namespace ODataSample.Web.Models
{
	public class ApplicationUser : OpenIddictUser<string, OpenIddictAuthorization, OpenIddictToken>
	{
		public int? UsedProductId { get; set; }
		public UserType Type { get; set; }
		public Product UsedProduct { get; set; }
		public Product FavouriteProduct { get; set; }
		public int? FavouriteProductId { get; set; }
		public List<Product> ProductsCreated { get; set; }
		public List<Product> ProductsLastModified { get; set; }
	}
}