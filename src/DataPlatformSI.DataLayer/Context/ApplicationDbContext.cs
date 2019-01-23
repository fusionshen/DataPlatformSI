using DataPlatformSI.DataLayer.Mappings;
using DataPlatformSI.Common.GuardToolkit;
using DataPlatformSI.Entities;
using DataPlatformSI.Entities.AuditableEntity;
using DataPlatformSI.Entities.Identity;
using DataPlatformSI.ViewModels.Identity.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using DataPlatformSI.Entities.Modules;
using DataPlatformSI.Entities.MDM;

namespace DataPlatformSI.DataLayer.Context
{
    public class ApplicationDbContext :
       IdentityDbContext<User, Role, int, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>,
       IUnitOfWork
    {
        // we can't use constructor injection anymore, because we are using the `AddDbContextPool<>`
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        #region BaseClass

        public virtual DbSet<AppLogItem> AppLogItems { get; set; }
        public virtual DbSet<AppSqlCache> AppSqlCache { get; set; }
        public virtual DbSet<AppDataProtectionKey> AppDataProtectionKeys { get; set; }

        public void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            Set<TEntity>().AddRange(entities);
        }

        public void ExecuteSqlCommand(string query)
        {
            Database.ExecuteSqlCommand(query);
        }

        public void ExecuteSqlCommand(string query, params object[] parameters)
        {
            Database.ExecuteSqlCommand(query, parameters);
        }

        public T GetShadowPropertyValue<T>(object entity, string propertyName) where T : IConvertible
        {
            var value = this.Entry(entity).Property(propertyName).CurrentValue;
            return value != null
                ? (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture)
                : default(T);
        }

        public object GetShadowPropertyValue(object entity, string propertyName)
        {
            return this.Entry(entity).Property(propertyName).CurrentValue;
        }

        public void MarkAsChanged<TEntity>(TEntity entity) where TEntity : class
        {
            Update(entity);
        }

        public void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            Set<TEntity>().RemoveRange(entities);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ChangeTracker.DetectChanges();

            BeforeSaveTriggers();

            ChangeTracker.AutoDetectChangesEnabled = false; // for performance reasons, to avoid calling DetectChanges() again.
            var result = base.SaveChanges(acceptAllChangesOnSuccess);
            ChangeTracker.AutoDetectChangesEnabled = true;
            return result;
        }

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();

            BeforeSaveTriggers();

            ChangeTracker.AutoDetectChangesEnabled = false; // for performance reasons, to avoid calling DetectChanges() again.
            var result = base.SaveChanges();
            ChangeTracker.AutoDetectChangesEnabled = true;
            return result;
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            ChangeTracker.DetectChanges();

            BeforeSaveTriggers();

            ChangeTracker.AutoDetectChangesEnabled = false; // for performance reasons, to avoid calling DetectChanges() again.
            var result = base.SaveChangesAsync(cancellationToken);
            ChangeTracker.AutoDetectChangesEnabled = true;
            return result;
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            ChangeTracker.DetectChanges();

            BeforeSaveTriggers();

            ChangeTracker.AutoDetectChangesEnabled = false; // for performance reasons, to avoid calling DetectChanges() again.
            var result = base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            ChangeTracker.AutoDetectChangesEnabled = true;
            return result;
        }

        private void BeforeSaveTriggers()
        {
            ValidateEntities();
            SetShadowProperties();
        }

        private void SetShadowProperties()
        {
            // we can't use constructor injection anymore, because we are using the `AddDbContextPool<>`
            var httpContextAccessor = this.GetService<IHttpContextAccessor>();
            httpContextAccessor.CheckArgumentIsNull(nameof(httpContextAccessor));
            ChangeTracker.SetAuditableEntityPropertyValues(httpContextAccessor);
        }

        private void ValidateEntities()
        {
            var errors = this.GetValidationErrors();
            if (!string.IsNullOrWhiteSpace(errors))
            {
                // we can't use constructor injection anymore, because we are using the `AddDbContextPool<>`
                var loggerFactory = this.GetService<ILoggerFactory>();
                loggerFactory.CheckArgumentIsNull(nameof(loggerFactory));
                var logger = loggerFactory.CreateLogger<ApplicationDbContext>();
                logger.LogError(errors);
                throw new InvalidOperationException(errors);
            }
        }

        #endregion

        public virtual DbSet<Category> Categories { set; get; }
        public virtual DbSet<Product> Products { set; get; }
        public virtual DbSet<Module> Modules { get; set; }
        public virtual DbSet<Server> Servers { get; set; }
        public virtual DbSet<Repository> Repositories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // it should be placed here, otherwise it will rewrite the following settings!
            base.OnModelCreating(builder);
            // we can't use constructor injection anymore, because we are using the `AddDbContextPool<>`
            var siteSettings = this.GetService<IOptionsSnapshot<SiteSettings>>();
            siteSettings.CheckArgumentIsNull(nameof(siteSettings));
            siteSettings.Value.CheckArgumentIsNull(nameof(siteSettings.Value));
            // Adds all of the ASP.NET Core Identity related mappings at once.
            builder.AddCustomIdentityMappings(siteSettings.Value);

            // Custom application mappings
            builder.Entity<Category>(build =>
            {
                build.Property(category => category.Name).HasMaxLength(450).IsRequired();
                build.Property(category => category.Title).IsRequired();
            });

            builder.Entity<Product>(build =>
            {
                build.Property(product => product.Name).HasMaxLength(450).IsRequired();
                build.HasOne(product => product.Category)
                       .WithMany(category => category.Products);
            });

            builder.Entity<Module>(build =>
            {
                build.Property(module => module.Name).HasMaxLength(450).IsRequired();
                build.Property(module => module.SpaceName).HasMaxLength(450).IsRequired();
            });

            builder.Entity<Server>(build =>
            {
                build.Property(server => server.Name).HasMaxLength(450).IsRequired();
            });

            builder.Entity<Repository>(build =>
            {
                build.Property(repo => repo.Name).HasMaxLength(450).IsRequired();
                build.Property(repo => repo.DisplayName).HasMaxLength(450).IsRequired();
                build.Property(repo => repo.Server).HasMaxLength(450).IsRequired();
                build.Property(repo => repo.UserName).HasMaxLength(450).IsRequired();
                build.Property(repo => repo.Password).HasMaxLength(450).IsRequired();
                build.Property(repo => repo.Port).HasMaxLength(450).IsRequired();
            });

            // This should be placed here, at the end.
            builder.AddAuditableShadowProperties();
        }
    }
}