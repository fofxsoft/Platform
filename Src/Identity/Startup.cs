using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityServer4;
using Identity.Data;
using Identity.Models;

namespace Identity
{
    public class Startup
    {
        public IConfiguration Configuration
        {
            get;
        }

        public IHostingEnvironment Environment
        {
            get;
        }

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = Configuration.GetConnectionString("DefaultConnection");
            string migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddDbContext<UserData>(options => options.UseMySql(connectionString))
                    .AddDbContext<ConfigurationData>(options => options.UseMySql(connectionString))
                    .AddDbContext<GrantData>(options => options.UseMySql(connectionString));

            services.AddIdentity<UserModel, RoleModel>()
                    .AddEntityFrameworkStores<UserData>()
                    .AddDefaultTokenProviders();

            services.AddMvc();

            IIdentityServerBuilder builder = services.AddIdentityServer(options =>
                                                     {
                                                         options.Events.RaiseErrorEvents = true;
                                                         options.Events.RaiseInformationEvents = true;
                                                         options.Events.RaiseFailureEvents = true;
                                                         options.Events.RaiseSuccessEvents = true;
                                                     })
                                                     .AddAspNetIdentity<UserModel>()
                                                     .AddConfigurationStore(options =>
                                                     {
                                                         options.ConfigureDbContext = db => db.UseMySql(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                                                     })
                                                     .AddOperationalStore(options =>
                                                     {
                                                         options.ConfigureDbContext = db => db.UseMySql(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                                                         options.EnableTokenCleanup = true;
                                                     });

            if (Environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("need to configure key material");
            }

            services.AddAuthentication()
                    .AddCookie("Cookies");

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            app.UseHttpsRedirection()
               .UseIdentityServer()
               .UseStaticFiles()
               .UseMvcWithDefaultRoute();

        }
    }
}
