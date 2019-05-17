using DataPlatformSI.DataLayer.Context;
using DataPlatformSI.Services.Identity;
using DataPlatformSI.ViewModels.Identity.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DataPlatformSI.IocConfig
{
    public static class CustomTicketStoreExtensions
    {
        public static IServiceCollection AddCustomTicketStore(
            this IServiceCollection services, SiteSettings siteSettings)
        {
            // To manage large identity cookies
            var cookieOptions = siteSettings.CookieOptions;
            if (cookieOptions.UseDistributedCacheTicketStore)
            {
                switch (siteSettings.ActiveDatabase)
                {
                    case ActiveDatabase.SqlServer:
                        services.AddDistributedSqlServerCache(options =>
                        {
                            var cacheOptions = cookieOptions.DistributedSqlServerCacheOptions;
                            var connectionString = string.IsNullOrWhiteSpace(cacheOptions.ConnectionString) ?
                                    siteSettings.GetDbConnectionString() :
                                    cacheOptions.ConnectionString;
                            options.ConnectionString = connectionString;
                            options.SchemaName = cacheOptions.SchemaName;
                            options.TableName = cacheOptions.TableName;
                        });
                        break;
                    case ActiveDatabase.MySql:
                        services.AddDistributedMySqlCache(options =>
                        {
                            var cacheOptions = cookieOptions.DistributedSqlServerCacheOptions;
                            var connectionString = string.IsNullOrWhiteSpace(cacheOptions.ConnectionString) ?
                                    siteSettings.GetDbConnectionString() :
                                    cacheOptions.ConnectionString;
                            options.ConnectionString = connectionString;
                            options.SchemaName = cacheOptions.SchemaName;
                            options.TableName = cacheOptions.TableName;
                        });
                        break;
                    default:
                        throw new NotSupportedException("Please set the ActiveDatabase in appsettings.json file.");
                }
                services.AddScoped<ITicketStore, DistributedCacheTicketStore>();
            }
            else
            {
                services.AddMemoryCache();
                services.AddScoped<ITicketStore, MemoryCacheTicketStore>();
            }

            return services;
        }
    }
}
