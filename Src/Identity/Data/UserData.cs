using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Identity.Models;

namespace Identity.Data
{
    public class UserData : IdentityDbContext<UserModel, RoleModel, string, UserClaimModel, UserRoleModel, UserLoginModel, RoleClaimModel, UserTokenModel>
    {
        public UserData(DbContextOptions<UserData> options)
            : base(options)
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

            builder.Entity<UserModel>(entity =>
            {
                entity.ToTable(name: "Users");
            });

            builder.Entity<RoleModel>(entity =>
            {
                entity.ToTable(name: "Roles");
            });

            builder.Entity<UserClaimModel>(entity =>
            {
                entity.ToTable(name: "UserClaims");
            });

            builder.Entity<UserRoleModel>(entity =>
            {
                entity.ToTable(name: "UserRoles");
            });

            builder.Entity<UserLoginModel>(entity =>
            {
                entity.ToTable(name: "UserLogins");
            });

            builder.Entity<RoleClaimModel>(entity =>
            {
                entity.ToTable(name: "RoleClaims");
            });

            builder.Entity<UserTokenModel>(entity =>
            {
                entity.ToTable(name: "UserTokens");
            });
        }
    }
}
