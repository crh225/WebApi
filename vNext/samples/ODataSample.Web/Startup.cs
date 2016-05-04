using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ODataSample.Web
{
	public class Startup : StartupBase
	{
		public Startup(IHostingEnvironment env)
			: base(env, "development")
		{
		}

		public override void ConfigureServices(IServiceCollection services)
		{
			services.AddMvcDnx();
			base.ConfigureServices(services);
		}
	}
}