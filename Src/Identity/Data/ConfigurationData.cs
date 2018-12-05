using System.Linq;
using Microsoft.EntityFrameworkCore;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Options;

namespace Identity.Data
{
    public class ConfigurationData : ConfigurationDbContext
    {
        public ConfigurationData(DbContextOptions<ConfigurationDbContext> options, ConfigurationStoreOptions storeOptions)
            : base(options, storeOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            foreach (var property in builder.Model.GetEntityTypes().SelectMany(t => t.GetProperties()).Where(p => p.ClrType == typeof(string)))
            {
                if (property.GetMaxLength() == null)
                {
                    property.SetMaxLength(256);
                }
            }

            foreach (var property in builder.Model.GetEntityTypes().SelectMany(t => t.GetProperties()).Where(p => p.ClrType == typeof(bool)))
            {
                property.SetProviderClrType(typeof(int));
            }
        }
    }
}
