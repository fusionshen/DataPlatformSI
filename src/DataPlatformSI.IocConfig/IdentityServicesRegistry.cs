using DataPlatformSI.Common.GuardToolkit;
using DataPlatformSI.Services.Contracts.Identity;
using DataPlatformSI.ViewModels.Identity.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataPlatformSI.IocConfig
{
    public static class IdentityServicesRegistry
    {
        /// <summary>
        /// Adds all of the ASP.NET Core Identity related services and configurations at once.
        /// </summary>
        public static void AddCustomIdentityServices(this IServiceCollection services, string xmlPath)
        {
            var siteSettings = GetSiteSettings(services);
            services.AddIdentityOptions(siteSettings);
            services.AddCustomServices();
            services.AddCustomTicketStore(siteSettings);
            services.AddDynamicPermissions();
            services.AddCustomDataProtection(siteSettings);
            services.AddJWTAuthentication(siteSettings);

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
            services.AddSwaggerGen(xmlPath);


        }

        public static void UseSwaggerServices(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(
                options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "DataPlatform API V1");
                });
        }

        /// <summary>
        /// Adds all of the ASP.NET Core Identity related initializations at once.
        /// </summary>
        public static void UseCustomIdentityServices(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.CallDbInitializer();
        }

        private static void CallDbInitializer(this IApplicationBuilder app)
        {
            var scopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var identityDbInitialize = scope.ServiceProvider.GetService<IIdentityDbInitializer>();
                identityDbInitialize.Initialize();
                identityDbInitialize.SeedData();
            }
        }

        public static SiteSettings GetSiteSettings(this IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            var siteSettingsOptions = provider.GetService<IOptionsSnapshot<SiteSettings>>();
            siteSettingsOptions.CheckArgumentIsNull(nameof(siteSettingsOptions));
            var siteSettings = siteSettingsOptions.Value;
            siteSettings.CheckArgumentIsNull(nameof(siteSettings));
            return siteSettings;
        }
    }
}