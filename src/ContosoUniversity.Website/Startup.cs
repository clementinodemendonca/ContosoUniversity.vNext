﻿using ContosoUniversity.DAL;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Data.Entity;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace ContosoUniversity.Website
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            // Setup configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(appEnv.ApplicationBasePath)
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add MVC services to the services container.
            services.AddMvc();

            // Uncomment the following line to add Web API services which makes it easier to port Web API 2 controllers.
            // You will also need to add the Microsoft.AspNet.Mvc.WebApiCompatShim package to the 'dependencies' section of project.json.
            // services.AddWebApiConventions();

            var configuration = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(configuration);

            var sqlConnectionString = configuration["Data:DefaultConnection:ConnectionString"];
            var useInMemoryDatabase = string.IsNullOrWhiteSpace(sqlConnectionString);

            if (useInMemoryDatabase)
            {
                services.AddEntityFramework()

                    .AddInMemoryDatabase()
                    .AddDbContext<SchoolContext>(options =>
                    {
                        options.UseInMemoryDatabase();
                    });
            }
            else
            {
                services.AddEntityFramework()
                    .AddSqlServer()
                    .AddDbContext<SchoolContext>(options =>
                    {
                        options.UseSqlServer(sqlConnectionString);
                    });
            }

            services.AddTransient<ISchoolContext>(s => s.GetService<SchoolContext>());
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.MinimumLevel = LogLevel.Information;
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            // Configure the HTTP request pipeline.

            // Add the following to the request pipeline only in development environment.
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Add Error handling middleware which catches all application specific errors and
                // send the request to the following path or controller action.
                app.UseExceptionHandler("/Home/Error");
            }

            // Add the platform handler to the request pipeline.
            app.UseIISPlatformHandler();

            // Add static files to the request pipeline.
            app.UseStaticFiles();

            // Add MVC to the request pipeline.
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                // Uncomment the following line to add a route for porting Web API 2 controllers.
                // routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");
            });

            ActivatorUtilities
                .CreateInstance<SchoolInitializer>(app.ApplicationServices, app.ApplicationServices.GetService<SchoolContext>())
                .Seed();
        }
    }
}
