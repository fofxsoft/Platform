using System.IO;
using System.Net;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Identity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var seed = args.Contains("/seed");

            if (seed)
            {
                args = args.Except(new[] {"/seed"}).ToArray();
            }

            IConfiguration config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                                              .AddJsonFile("appsettings.json", optional: false)
                                                              .AddCommandLine(args)
                                                              .Build();

            IWebHost host = CreateWebHostBuilder(args, config).Build();

            if (seed)
            {
                SeedData.EnsureSeedData(host.Services);

                return;
            }

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, IConfiguration config) =>
            WebHost.CreateDefaultBuilder(args)
                   .UseKestrel(options =>
                   {
                       options.Listen(IPAddress.Loopback, 5000, listenOptions =>
                       {
                           IConfigurationSection cert = config.GetSection("Certificate");

                           listenOptions.UseHttps(cert.GetValue<string>("FileName"), cert.GetValue<string>("Password"));
                       });
                   })
                   .UseWebRoot("Public")
                   .UseStartup<Startup>();

    }
}
