using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace PlatformOne.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration {
            get;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore()
                    .AddAuthorization()
                    .AddJsonFormatters();

            services.AddAuthentication("Bearer")
                    .AddIdentityServerAuthentication(options =>
                    {
                        options.Authority = Configuration.GetSection("Authentication").GetValue<string>("Authority");
                        options.RequireHttpsMetadata = false;
                        options.ApiName = "api";
                        options.EnableCaching = true;
                        options.CacheDuration = TimeSpan.FromMinutes(10);
                    });

            services.AddAuthorization(options =>
                    {
                        options.AddPolicy("Users", builder =>
                        {
                            builder.RequireScope("profile");
                        });
                    });

            services.AddCors(options =>
            {
                options.AddPolicy("default", policy =>
                {
                    policy.WithOrigins("*")
                          .AllowAnyHeader()
                          .AllowAnyMethod();

                });
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors("default")
               .UseHttpsRedirection()
               .UseAuthentication()
               .UseMvc();

        }
    }
}
