using System;
using System.Diagnostics;
using System.IO;
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
using OpenIddict;

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

            services
                .AddOpenIddict<ApplicationUser, ApplicationRole, OpenIddictApplication, OpenIddictAuthorization, OpenIddictScope, OpenIddictToken, ApplicationDbContext, string>()
                .AddCors(c =>
                {
                    c.AllowAnyHeader();
                    c.AllowAnyMethod();
                    c.AllowAnyOrigin();
                    c.AllowCredentials();
                })
                .Configure(options =>
                {
                    options.AuthorizationEndpointPath = "/connect/authorize";
                    options.AccessTokenLifetime = new TimeSpan(30, 0, 0, 0);
                    options.AllowInsecureHttp = true;
                    options.SigningCredentials.AddCertificate(
                        new MemoryStream(
                            Convert.FromBase64String(
                                @"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")
                            ),
                        "thisB3TT3Rwork!!");
                    //app.ApplicationServices.GetService<App>().OpenIddictOptions = options;
                })
                .AddMvc()
                .DisableHttpsRequirement();

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

            app.UseOpenIddict();

            app.UseOAuthValidation();

            app.UseOData("odata");

            app.UseStaticFiles();

            var defaultFilesOptions = new DefaultFilesOptions();
            defaultFilesOptions.DefaultFileNames.Add("index.html");
            app.UseDefaultFiles(defaultFilesOptions);

            app.UseMvc(
                routes =>
                {
                    routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
                    var mvcRouter = routes.Build();
                });

            //app.UseMvcWithDefaultRoute();
        }
    }
}