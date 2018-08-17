using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DataPlatformSI.DataAccess;
using DataPlatformSI.DataAccess.Models;
using DataPlatformSI.DataAccess.Repositories;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.OData.Edm;
using Swashbuckle.AspNetCore.Swagger;

namespace DataPlatformSI.WebAPI
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
            services.AddOData();

            services.AddScoped<ProductsRepository>();
            services.AddDbContext<ProductContext>(options =>
                 options.UseSqlServer(Configuration.GetConnectionString("ProductContext")));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Workaround: https://github.com/OData/WebApi/issues/1177
            services.AddMvcCore(options =>
            {
                foreach (var outputFormatter in options.OutputFormatters.OfType<ODataOutputFormatter>().Where(_ => _.SupportedMediaTypes.Count == 0))
                {
                    outputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }
                foreach (var inputFormatter in options.InputFormatters.OfType<ODataInputFormatter>().Where(_ => _.SupportedMediaTypes.Count == 0))
                {
                    inputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "Products API",
                    Description = "A products Web API demonstrating action return types"
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetEntryAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                loggerFactory.AddConsole(Configuration.GetSection("Logging"));
                loggerFactory.AddDebug();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My ASP.NET Core 2.1 API v1");
                c.RoutePrefix = string.Empty;
            });

            //var builder = new ODataConventionModelBuilder(app.ApplicationServices);

            //builder.EntitySet<Product>("Products");

            app.UseMvc(routes =>
            {
                routes.MapODataServiceRoute("odata", "odata", GetEdmModel());

                // Workaround: https://github.com/OData/WebApi/issues/1175
                routes.EnableDependencyInjection();
            });
        }

        // Builds the EDM model for the OData service, including the OData action definitions.
        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Product>("Products");

            var moviesEntitySet = modelBuilder.EntitySet<Movie>("Movies");

            // Now add actions.

            // CheckOut
            // URI: ~/odata/Movies(1)/ODataActionSample.Models.CheckOut
            ActionConfiguration checkOutAction = modelBuilder.EntityType<Movie>().Action("CheckOut");
            checkOutAction.ReturnsFromEntitySet<Movie>("Movies");

            // ReturnMovie
            // URI: ~/odata/Movies(1)/ODataActionSample.Models.Return
            // Binds to a single entity; no parameters.
            ActionConfiguration returnAction = modelBuilder.EntityType<Movie>().Action("Return");
            returnAction.ReturnsFromEntitySet<Movie>("Movies");

            // CheckOutMany action
            // URI: ~/odata/Movies/ODataActionSample.Models.CheckOutMany
            // Binds to a collection of entities.  This action accepts a collection of parameters.
            ActionConfiguration checkOutManyAction = modelBuilder.EntityType<Movie>().Collection.Action("CheckOutMany");
            checkOutManyAction.CollectionParameter<int>("MovieIDs");
            checkOutManyAction.ReturnsCollectionFromEntitySet<Movie>("Movies");

            // CreateMovie action
            // URI: ~/odata/CreateMovie
            // Unbound action. It is invoked from the service root.
            ActionConfiguration createMovieAction = modelBuilder.Action("CreateMovie");
            createMovieAction.Parameter<string>("Title");
            createMovieAction.ReturnsFromEntitySet<Movie>("Movies");

            modelBuilder.Namespace = typeof(Movie).Namespace;

            return modelBuilder.GetEdmModel();
        }
    }
}
