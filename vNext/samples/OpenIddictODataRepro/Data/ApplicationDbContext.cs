using System.Linq;
using Microsoft.EntityFrameworkCore;
using OpenIddict;
using OpenIddictODataRepro.Data.Models;

namespace OpenIddictODataRepro.Data
{
    public class ApplicationDbContext : OpenIddictDbContext<ApplicationUser, ApplicationRole, OpenIddictApplication, OpenIddictAuthorization, OpenIddictScope, OpenIddictToken, string>, ISampleService
    {
        public DbSet<Product> Products { get; set; }
        IQueryable<Product> ISampleService.Products => Products;
        IQueryable<ApplicationUser> ISampleService.Users => Users;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.UseInMemoryDatabase();
            base.OnConfiguring(builder);
        }
    }
}