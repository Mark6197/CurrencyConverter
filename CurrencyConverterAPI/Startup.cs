using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using ScraperService;
using ScraperService.Models;
using System;
using System.IO;
using System.Reflection;

namespace CurrencyConverterAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Get the ScraperConfig section data from the config file and populate new ScraperConfig instance with this data
            var scraperConfig = Configuration.GetSection("Scraper").Get<ScraperConfig>();
            //Add the ScraperConfig instance as a singleton srevice
            services.AddSingleton(scraperConfig);

            //Add the IScraper interface and it's Scraper class implementation as a scoped srevice
            services.AddScoped<IScraper, Scraper>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CurrencyConverterAPI", Version = "v1" });
                // Read the comments from the xml file that is being auto genrated on build,
                // this file includes the comments from the contollers
                // We configure to auto genreate the file in csproj file: <GenerateDocumentationFile>true</GenerateDocumentationFile>
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            //Add cors policy to expose all the endpoints in the web api to any origin (might restrict it later to specific origin)
            services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CurrencyConverterAPI v1"));
            }

            //Use the cors policy defined in ConfigureServices
            app.UseCors("CorsPolicy");

            app.UseHttpsRedirection();
            
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
