using System.Linq;

namespace OpenIddictODataRepro.Data.Models
{
    public interface ISampleService
    {
        IQueryable<ApplicationUser> Users { get; }
        IQueryable<Product> Products { get; }
    }
}