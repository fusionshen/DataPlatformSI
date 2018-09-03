using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DataPlatformSI.DataLayer.Context;
using DataPlatformSI.DomainClasses;
using DataPlatformSI.Services;
using DataPlatformSI.WebAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
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
            services.Configure<BearerTokensOptions>(options => Configuration.GetSection("BearerTokens").Bind(options));
            services.Configure<ApiSettings>(options => Configuration.GetSection("ApiSettings").Bind(options));

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IAntiForgeryCookieService, AntiForgeryCookieService>();
            services.AddScoped<IUnitOfWork, ApplicationDbContext>();
            services.AddScoped<IUsersService, UsersService>();
            services.AddScoped<IRolesService, RolesService>();
            services.AddScoped<ISecurityService, SecurityService>();
            services.AddScoped<IDbInitializerService, DbInitializerService>();
            services.AddScoped<ITokenStoreService, TokenStoreService>();
            services.AddScoped<ITokenValidatorService, TokenValidatorService>();
            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")
                                 .Replace("|DataDirectory|", Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "app_data")),
                    serverDbContextOptionsBuilder =>
                    {
                        var minutes = (int)TimeSpan.FromMinutes(3).TotalSeconds;
                        serverDbContextOptionsBuilder.CommandTimeout(minutes);
                        serverDbContextOptionsBuilder.EnableRetryOnFailure();
                    });
            });

            // Only needed for custom roles.
            services.AddAuthorization(options =>
            {
                options.AddPolicy(CustomRoles.Admin, policy => policy.RequireRole(CustomRoles.Admin));
                options.AddPolicy(CustomRoles.User, policy => policy.RequireRole(CustomRoles.User));
                options.AddPolicy(CustomRoles.Editor, policy => policy.RequireRole(CustomRoles.Editor));
            });

            // Needed for jwt auth.
            services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = Configuration["BearerTokens:Issuer"], // site that makes the token
                        ValidateIssuer = false, // TODO: change this to avoid forwarding attacks
                        ValidAudience = Configuration["BearerTokens:Audience"], // site that consumes the token
                        ValidateAudience = false, // TODO: change this to avoid forwarding attacks
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["BearerTokens:Key"])),
                        ValidateIssuerSigningKey = true, // verify signature to avoid tampering
                        ValidateLifetime = true, // validate the expiration
                        ClockSkew = TimeSpan.Zero // tolerance for the expiration date
                    };
                    cfg.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
                            logger.LogError("Authentication failed.", context.Exception);
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            var tokenValidatorService = context.HttpContext.RequestServices.GetRequiredService<ITokenValidatorService>();
                            return tokenValidatorService.ValidateAsync(context);
                        },
                        OnMessageReceived = context =>
                        {
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
                            logger.LogError("OnChallenge error", context.Error, context.ErrorDescription);
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                        .WithOrigins("http://localhost:4200") //Note:  The URL must be specified without a trailing slash (/).
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            services.AddAntiforgery(x => x.HeaderName = "X-XSRF-TOKEN");

            services.AddMvc(options =>
            {
               options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            #region OData
            //services.AddOData();
            #endregion
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                   new Info()
                   {
                       Title = $"DataPlatform API V1",
                       Version = "V1",
                       Description = "A webapi application with Swagger, Swashbuckle, and API versioning.",
                       Contact = new Contact() { Name = "Fusion Shen", Email = "fusionshen@hotmail.com" },
                       TermsOfService = "Shareware",
                       License = new License() { Name = "MIT", Url = "https://opensource.org/licenses/MIT" }
                   }
                );
                c.IncludeXmlComments(XmlCommentsFilePath);
            });

            #region apiversioning
            //services.AddApiVersioning(options => options.ReportApiVersions = true);
            //services.AddOData().EnableApiVersioning();
            //services.AddODataApiExplorer(
            //    options =>
            //    {
            //        // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
            //        // note: the specified format code will format the version as "'v'major[.minor][-status]"
            //        options.GroupNameFormat = "'v'VVV";

            //        // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
            //        // can also be used to control the format of the API version in route templates
            //        options.SubstituteApiVersionInUrl = true;
            //    });
            //services.AddSwaggerGen(
            //    options =>
            //    {
            //        // resolve the IApiVersionDescriptionProvider service
            //        // note: that we have to build a temporary service provider here because one has not been created yet
            //        var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

            //        // add a swagger document for each discovered API version
            //        // note: you might choose to skip or document deprecated API versions differently
            //        foreach (var description in provider.ApiVersionDescriptions)
            //        {
            //            options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
            //        }

            //        // add a custom operation filter which sets default values
            //        options.OperationFilter<SwaggerDefaultValues>();

            //        // integrate xml comments
            //        options.IncludeXmlComments(XmlCommentsFilePath);

            //        //options.AddSecurityDefinition(
            //        //    "oauth2",
            //        //    new OAuth2Scheme
            //        //    {
            //        //        Type = "oauth2",
            //        //        Flow = "implicit",
            //        //        Description = "OAuth2.0 implicit flow",
            //        //        TokenUrl = Configuration.GetValue("jwtIss", Configuration["Jwt:Iss"]) + "/connect/token",
            //        //        AuthorizationUrl = Configuration.GetValue("jwtIss", Configuration["Jwt:Iss"]) + "/connect/authorize",
            //        //        Scopes = new Dictionary<string, string> { { "api1", "Access scope for PT"} }
            //        //    });
            //        //options.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>> { { "oauth2", new[] { "api1" } } });
            //    });
            #endregion

            #region Identity
            // ===== Add Identity ========
            //services.AddIdentity<ApplicationUser, IdentityRole>()
            //    .AddEntityFrameworkStores<ApplicationDbContext>()
            //    .AddDefaultTokenProviders();

            //services.Configure<IdentityOptions>(options =>
            //{
            //    // Password settings
            //    options.Password.RequireDigit = false;
            //    options.Password.RequiredLength = 1;
            //    options.Password.RequireNonAlphanumeric = false;
            //    options.Password.RequireUppercase = false;
            //    options.Password.RequireLowercase = false;
            //    options.Password.RequiredUniqueChars = 1;

            //    // Lockout settings
            //    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
            //    options.Lockout.MaxFailedAccessAttempts = 10;
            //    options.Lockout.AllowedForNewUsers = true;

            //    // User settings
            //    //options.User.RequireUniqueEmail = true;
            //})
            #endregion
        }

        /// <summary>
        /// Configures the application using the provided builder, hosting environment, and logging factory.
        /// </summary>
        /// <param name="app">The current application builder.</param>
        /// <param name="env">The current hosting environment.</param>
        /// <param name="loggerFactory"></param>
        public void Configure(
            IApplicationBuilder app, 
            IHostingEnvironment env, 
            ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                loggerFactory.AddConsole(Configuration.GetSection("Logging"));
                loggerFactory.AddDebug();

                app.UseBrowserLink();
                //mvc中的数据库报错页面，可初始化
                app.UseDatabaseErrorPage(); 
            }
            else
            {
                app.UseHsts();
            }
            // app.UseCors(policyName: "CorsPolicy");

            app.UseExceptionHandler(appBuilder =>
            {
                appBuilder.Use(async (context, next) =>
                {
                    var error = context.Features[typeof(IExceptionHandlerFeature)] as IExceptionHandlerFeature;
                    if (error != null && error.Error is SecurityTokenExpiredException)
                    {
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                        {
                            State = 401,
                            Msg = "token expired"
                        }));
                    }
                    else if (error != null && error.Error != null)
                    {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                        {
                            State = 500,
                            Msg = error.Error.Message
                        }));
                    }
                    else
                    {
                        await next();
                    }
                });
            });

            app.UseAuthentication();

            var scopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var dbInitializer = scope.ServiceProvider.GetService<IDbInitializerService>();
                dbInitializer.Initialize();
                dbInitializer.SeedData();
            }

            app.UseStatusCodePages();
            app.UseDefaultFiles(); // so index.html is not required
            app.UseStaticFiles();

            app.UseMvc(routeBuilder =>
            {
                //var models = modelBuilder.GetEdmModels();
                //routeBuilder.MapVersionedODataRoutes("odata", "api", models);
                //routeBuilder.MapVersionedODataRoutes("odata-bypath", "v{version:apiVersion}", models);
                //routeBuilder.MapODataServiceRoute("odata", "api", GetEdmModel());
                routeBuilder.MapRoute(
                   name: "default",
                   template: "{controller=Home}/{action=Index}/{id?}");
            });

            // catch-all handler for HTML5 client routes - serve index.html
            //app.Run(async context =>
            //{
            //    context.Response.ContentType = "text/html";
            //    await context.Response.SendFileAsync(Path.Combine(env.WebRootPath, "index.html"));
            //});

            app.UseSwagger();
            app.UseSwaggerUI(
                options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "DataPlatform API V1");
                    // build a swagger endpoint for each discovered API version
                    //foreach (var description in provider.ApiVersionDescriptions)
                    //{
                    //    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                    //}
                });
        }

        #region EDM
        // Builds the EDM model for the OData service, including the OData action definitions.
        //private static IEdmModel GetEdmModel()
        //{
        //    ODataModelBuilder builder = new ODataConventionModelBuilder();

        //    builder.EntitySet<User>("Account");

        //    var userType = builder.EntityType<User>();

        //    // Function bound to a collection
        //    // Returns the most expensive product, a single entity
        //    userType.Collection
        //        .Function("Login")
        //        .Returns<object>();

        //    // Function bound to a collection
        //    // Returns the top 10 product, a collection
        //    userType.Collection
        //        .Function("RefreshToken")
        //        .Returns<object>();

        //    // Function bound to a single entity
        //    // Returns the instance's price rank among all products
        //    userType.Collection
        //        .Function("Logout")
        //        .Returns<bool>().Parameter<string>("refreshToken");

        //    userType.Collection
        //        .Function("IsAuthenticated")
        //        .Returns<bool>();

        //    userType.Collection
        //        .Function("GetUserInfo")
        //        .Returns<object>();

        //    // Function bound to a single entity
        //    // Accept a string as parameter and return a double
        //    // This function calculate the general sales tax base on the
        //    // state
        //    //userType
        //    //    .Function("CalculateGeneralSalesTax")
        //    //    .Returns<double>()
        //    //    .Parameter<string>("state");

        //    // Unbound Function
        //    builder.Function("GetSalesTaxRate")
        //        .Returns<double>()
        //        .Parameter<string>("state");

        //    return builder.GetEdmModel();
        //}
        #endregion

        static string XmlCommentsFilePath
        {
            get
            {
                var basePath = System.AppContext.BaseDirectory;
                var fileName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name + ".xml";
                return Path.Combine(basePath, fileName);
            }
        }

        //static Info CreateInfoForApiVersion(ApiVersionDescription description)
        //{
        //    var info = new Info()
        //    {
        //        Title = $"Sample API {description.ApiVersion}",
        //        Version = description.ApiVersion.ToString(),
        //        Description = "A sample application with Swagger, Swashbuckle, and API versioning.",
        //        Contact = new Contact() { Name = "Bill Mei", Email = "bill.mei@somewhere.com" },
        //        TermsOfService = "Shareware",
        //        License = new License() { Name = "MIT", Url = "https://opensource.org/licenses/MIT" }
        //    };

        //    if (description.IsDeprecated)
        //    {
        //        info.Description += " This API version has been deprecated.";
        //    }

        //    return info;
        //}
    }
}
