using DataPlatformSI.DataLayer.Context;
using DataPlatformSI.Entities.Identity;
using DataPlatformSI.Services.Contracts.Identity;
using DataPlatformSI.ViewModels.Identity.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System;
using Microsoft.AspNetCore.Hosting.Internal;
using DataPlatformSI.IocConfig;
using DNTCommon.Web.Core;

namespace DataPlatformSI.MSTests
{
    [TestClass]
    public class CoreTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptionsSnapshot<SiteSettings> _siteSettings;

        public CoreTests()
        {
            var services = new ServiceCollection();
            services.AddOptions();
            services.AddScoped<IHostingEnvironment, HostingEnvironment>();

            var configuration = new ConfigurationBuilder()
                                .AddJsonFile("ASPNETCoreIdentitySample/appsettings.json", reloadOnChange: true, optional: false)
                                .Build();
            services.AddSingleton<IConfigurationRoot>(provider => configuration);
            services.Configure<SiteSettings>(options => configuration.Bind(options));
            services.AddEntityFrameworkInMemoryDatabase().AddDbContext<ApplicationDbContext>(ServiceLifetime.Scoped);

            // Adds all of the ASP.NET Core Identity related services and configurations at once.
            services.AddCustomIdentityServices();

            services.AddDNTCommonWeb();
            services.AddCloudscribePagination();

            _serviceProvider = services.BuildServiceProvider();

            _siteSettings = _serviceProvider.GetRequiredService<IOptionsSnapshot<SiteSettings>>();
            _siteSettings.Value.ActiveDatabase = ActiveDatabase.InMemoryDatabase;

            var identityDbInitialize = _serviceProvider.GetRequiredService<IIdentityDbInitializer>();
            identityDbInitialize.SeedData();
        }

        [TestMethod]
        public void Test_UserAdmin_Exists()
        {
            _serviceProvider.RunScopedService<IUnitOfWork>(context =>
            {
                var users = context.Set<User>();
                Assert.IsTrue(users.Any(x => x.UserName == "Admin"));
            });
        }
    }
}