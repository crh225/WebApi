using System.Linq;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenIddict;

namespace ODataSample.Web.Models
{
	public class ApplicationDbContext : OpenIddictDbContext<ApplicationUser, ApplicationRole, OpenIddictApplication, OpenIddictAuthorization, OpenIddictScope, OpenIddictToken, string>, ISampleService
	{
		public DbSet<Client> Clients { get; set; }
		public DbSet<Product> Products { get; set; }
		public DbSet<Customer> Customers { get; set; }
		public DbSet<Order> Orders { get; set; }

		IQueryable<Client> ISampleService.Clients => Clients;
		IQueryable<ApplicationUser> ISampleService.Users => Users;
		//IQueryable<IdentityUserRole<string>> ISampleService.UserRoles => UserRoles;
		//IQueryable<IdentityRole> ISampleService.Roles => Roles;
		IQueryable<Order> ISampleService.Orders => Orders;
		IQueryable<Product> ISampleService.Products => Products;
		IQueryable<Customer> ISampleService.Customers => Customers;


		protected override void OnModelCreating(ModelBuilder builder)
		{
			//builder
			//	.Entity<Product>()
			//	.Property(e => e.DateCreated)
			//	.HasDefaultValueSql("getdate()");
			builder
				.Entity<ApplicationUser>()
				.HasOne(u => u.UsedProduct)
				.WithMany(p => p.UsedByUsers);
			builder
				.Entity<ApplicationUser>()
				.HasOne(u => u.FavouriteProduct)
				;
			builder
				.Entity<ApplicationUser>()
				.HasMany(u => u.ProductsCreated)
				.WithOne(u => u.CreatedByUser)
				;
			builder
				.Entity<Product>()
                .HasKey(p => p.ProductId)
                ;
            builder
				.Entity<Product>()
				.HasOne(u => u.CreatedByUser)
				.WithMany(u => u.ProductsCreated)
                ;
            builder
				.Entity<Product>()
				.HasOne(u => u.LastModifiedByUser)
				.WithMany(u => u.ProductsLastModified);
			//builder
			//	.Entity<Order>()
			//	.HasOne(u => u.Customer)
			//	.WithMany(u => u.Orders);
			base.OnModelCreating(builder);
		}

		protected override void OnConfiguring(DbContextOptionsBuilder builder)
		{
			builder.UseSqlServer(
				"Server=.;Database=Microsoft.AspNetCore.OData.App.Data;User ID=morselsLogin;Password=PPm|Wb(An!Cb1~{}&]UPxO@nf;Trusted_Connection=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;");
			base.OnConfiguring(builder);
		}
	}
}
