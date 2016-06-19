using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ODataSample.Web.Models;
using ODataSample.Web.Models.Interceptors;

namespace ODataSample.Web
{
    public abstract class StartupBase
    {
        public static void Init<TStartup>(Action<IWebHostBuilder> building = null, params string[] args) where TStartup : class
        {
            var host = new WebHostBuilder()
                .CaptureStartupErrors(true);

            building?.Invoke(host);

            host.UseStartup<TStartup>();
            host.Build().Run();
        }

        protected StartupBase(
            IHostingEnvironment env,
            string environment = null)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", true)
                ;


            //if (env.IsDevelopment())
            //{
            //	// For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
            //	builder.AddUserSecrets();

            //	//// This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
            //	//builder.AddApplicationInsightsSettings(developerMode: true);
            //}

            builder.AddEnvironmentVariables();
            builder.Build();
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddEntityFramework()
                .AddDbContext<ApplicationDbContext>();
            services.AddEntityFrameworkSqlServer();
            services.AddMvc();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder //.AllowAnyOrigin()
                                //.AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
            });
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Control password strength requirements here
                options.Password.RequireDigit = true;
                //options.Tokens.
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();


            services.AddScoped<ISampleService, ApplicationDbContext>();
            //services.ConfigureODataOutputFormatter<SampleOutputFormatter>();
            services.ConfigureODataSerializerProvider<SampleODataSerializerProvider>();
            //services.AddTransient<IODataQueryInterceptor<Product>, ProductInterceptor>();
            //services.AddTransient<IODataQueryInterceptor<Customer>, CustomerInterceptor>();
            services.AddOData<ISampleService>(ODataConfigurator.ConfigureODataSample);
        }

        public void Configure(IApplicationBuilder app,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            Seeder.MigrateDatabaseAsync(app.ApplicationServices);

            //app.UseMiddleware<ReflectionTypeLoadExceptionMiddleware>();
            app.UseDeveloperExceptionPage();

            app.Use(async (context, next) =>
            {
                //await context.Response.WriteAsync("HEYY2");
                try
                {
                    await next();
                }
                catch (ReflectionTypeLoadException e)
                {
                    await context.Response.WriteAsync(e.LoaderExceptions[0].Message);
                    //throw new Exception("Got it: " + e.InnerException.Message);
                }
            });

            //app.UseIISPlatformHandler();

            app.UseIdentity();

            app.UseOData("odata");

            app.UseStaticFiles();

            var defaultFilesOptions = new DefaultFilesOptions();
            defaultFilesOptions.DefaultFileNames.Add("index.html");
            app.UseDefaultFiles(defaultFilesOptions);

            app.UseMvc(
                routes =>
                {
                    routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
                    //appSettings.MvcRouter = routes.Build();
                });

            //app.UseMvcWithDefaultRoute();
        }
    }
}