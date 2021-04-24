using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CCCount.Models;
using CCCount.Infrastructure;
using NLog.Extensions.Logging;
using NLog.Web;
using NLog;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace CCCount
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.

            services.AddOptions();
            services.AddMvc(option => option.EnableEndpointRouting = false);
            services.Configure<ConnectionStrings>(Configuration.GetSection("ConnectionStrings"));
            services.Configure<ContentTypes>(Configuration.GetSection("ContentTypes"));
            services.Configure<CCCountSettings>(Configuration.GetSection("CCCountSettings"));
            services.Configure<FormOptions>(x => x.ValueCountLimit = 10000);

            // Http
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Logging
            services.AddScoped<GlobalLogFilter>();

            // UI Class
            services.AddScoped<ICCCountUI, CCCountUI>();

            // Session/TempData
            services.AddMemoryCache();
            services.AddSession();

            // MVC
            //  - Add GlobalExceptionFilter for global exception handling
            services.AddMvc(opt => {
                opt.Filters.Add(typeof(GlobalExceptionFilter));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env/*, ILoggerFactory loggerFactory*/)
        {


            // Set exception handling early in pipeline
            if (env.Equals("Development"))
            {
                //app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            //else
            //{
            //}

            //// NLog XML configuration file
            //env.ConfigureNLog("nlog.config");

            // NLog config variables
            //LogManager.Configuration.Variables["connectionString"] = Configuration.GetSection("ConnectionStrings").Get<ConnectionStrings>().SetupData_NLog;
            //LogManager.Configuration.Variables["configDir"] = $"C:\\Temp\\Logs";

            // Added to enable access to TempData
            app.UseSession();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
