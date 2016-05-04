using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace ODataSample.Web
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			args = DotNetRunner.AppStart(args);

			var config = new ConfigurationBuilder()
				 .AddCommandLine(args)
				 .AddJsonFile("appsettings.json")
				 .AddEnvironmentVariables(prefix: "ASPNETCORE_")
				 .Build();

			StartupBase.Init<Startup>(
				webHostBuilder =>
				{
					webHostBuilder
						.UseConfiguration(config)
						.UseEnvironment("Development")
						.UseKestrel()
						.UseIISIntegration();
					var ass = typeof (Program).GetTypeInfo().Assembly;
					// Hacky but will do for now
					if (
					ass.Location.IndexOf(@"\bin\", StringComparison.CurrentCultureIgnoreCase) != -1
					&& ass.Location.IndexOf(@"\Iis\", StringComparison.CurrentCultureIgnoreCase) == -1)
					{
						webHostBuilder.UseUrls("http://localhost:3745");
					}
				}
				);
		}

		private static void MicroApp(params string[] args)
		{
			var host = new WebHostBuilder()
				.UseKestrel()
				//.UseDefaultHostingConfiguration(args)
				.UseEnvironment("Development")
				//.UseIISPlatformHandlerUrl()
				.UseIISIntegration()
				.Configure(app =>
				{
					app.Use((context, next) =>
					{
						context.Response.Clear();
						context.Response.WriteAsync("Hey");
						return next();
					});
				})
				.Build();
			host.Run();
		}
	}
}