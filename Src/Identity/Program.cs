using System.Linq;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

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

            var host = CreateWebHostBuilder(args).Build();

            if (seed)
            {
                SeedData.EnsureSeedData(host.Services);

                return;
            }

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .UseKestrel(options =>
                   {
                       options.Listen(IPAddress.Loopback, 5000, listenOptions =>
                       {
                           listenOptions.UseHttps("certificate.pfx", "Marvin");
                       });
                   })
                   .UseWebRoot("Public")
                   .UseStartup<Startup>();

    }
}
