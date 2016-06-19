using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ODataSample.Web.Models;

namespace ODataSample.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();

                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
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
        }
    }
}
