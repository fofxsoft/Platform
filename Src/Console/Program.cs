using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using IdentityModel.Client;

namespace Console
{
    class Program
    {
        public static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();

        private static async Task MainAsync(string[] args)
        {
            if (args.Length >= 1)
            {
                switch (args[0].ToLower())
                {
                    case "public":
                        await AuthenticatePublic();
                        break;

                    case "application":
                        await AuthenticateApplication();
                        break;

                    case "user":
                        string user = "";
                        string pass = "";

                        if (args.Length >= 3)
                        {
                            for (int i = 0; i < args.Length; i++)
                            {
                                switch (args[i].ToLower())
                                {
                                    case "/u":
                                    case "/user":
                                    case "/username":
                                        if (string.IsNullOrWhiteSpace(user) && args.Length >= i + 2)
                                        {
                                            user = args[i + 1];
                                        }

                                        break;

                                    case "/p":
                                    case "/pass":
                                    case "/password":
                                        if (string.IsNullOrWhiteSpace(pass) && args.Length >= i + 2)
                                        {
                                            pass = args[i + 1];
                                        }

                                        break;
                                }
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass))
                        {
                            await AuthenticateUser(user, pass);
                        }
                        else
                        {
                            WriteHelp();
                        }

                        break;

                    default:
                        WriteHelp();
                        break;
                }
            }
            else
            {
                WriteHelp();
            }

            return;
        }

        private static void WriteHelp()
        {
            System.Console.WriteLine("Useage: [action] [options]\n\n");
            System.Console.WriteLine("Actions:\n    Application - Tests the identity server using client authentication.\n    User - Tests the identity server using resource owner authentication.\n    Public - Tests the identity server using no authentication.\n");
            System.Console.WriteLine("Options:\n    Username: [/u|/user|/username] - Sets the username for the User action.\n    Password: [/u|/user|/username] - Sets the password for the User action.");

            return;
        }

        private static async Task AuthenticatePublic()
        {
            await GetInput("");

            return;
        }

        private static async Task AuthenticateApplication()
        {
            DiscoveryResponse disco = await DiscoveryClient.GetAsync("https://localhost:5000");

            if (disco.IsError)
            {
                System.Console.WriteLine(disco.Error);

                return;
            }

            using (TokenClient tokenClient = new TokenClient(disco.TokenEndpoint, "client", "secret"))
            {
                TokenResponse tokenResponse = await tokenClient.RequestClientCredentialsAsync("api");

                if (tokenResponse.IsError)
                {
                    System.Console.WriteLine(tokenResponse.Error);

                    return;
                }

                System.Console.WriteLine("\n" + tokenResponse.Json + "\n\n");

                await GetInput(tokenResponse.AccessToken);
            }

            return;
        }

        private static async Task AuthenticateUser(string user, string pass)
        {
            DiscoveryResponse disco = await DiscoveryClient.GetAsync("https://localhost:5000");

            if (disco.IsError)
            {
                System.Console.WriteLine(disco.Error);

                return;
            }

            using (TokenClient tokenClient = new TokenClient(disco.TokenEndpoint, "ro.client", "secret"))
            {
                TokenResponse tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync(user, pass, "api");

                if (tokenResponse.IsError)
                {
                    System.Console.WriteLine(tokenResponse.Error);

                    return;
                }

                System.Console.WriteLine("\n" + tokenResponse.Json + "\n\n");

                await GetInput(tokenResponse.AccessToken);
            }

            return;
        }

        private static async Task GetInput(string accessToken)
        {
            while (true)
            {
                Point start = null;
                Point end = null;

                var input = "";

                System.Console.Write("Point 1 [0,0]: ");
                input = System.Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(input) && input.ToLower() != "exit")
                {
                    try
                    {
                        start = new Point(input);
                    }
                    catch
                    {
                        System.Console.WriteLine("\nPlease enter a valid end point. \"X, Y\"\n");

                        start = null;
                    }
                }
                else if (input.ToLower() == "exit")
                {
                    return;
                }
                else
                {
                    start = new Point(0, 0);
                }

                System.Console.Write("Point 2 : ");
                input = System.Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(input) && input.ToLower() != "exit")
                {
                    try
                    {
                        end = new Point(input);
                    }
                    catch
                    {
                        System.Console.WriteLine("\nPlease enter a valid end point. \"X, Y\"\n");

                        end = null;
                    }
                }
                else if (input.ToLower() == "exit")
                {
                    return;
                }
                else
                {
                    System.Console.WriteLine("\nPlease enter a valid end point. \"X, Y\"\n");
                }

                if (start != null && end != null)
                {
                    System.Console.WriteLine("\n" + await GetResults("https://localhost:5001/api/values/distance/?x1=" + start.X + "&y1=" + start.Y + "&x2=" + end.X + "&y2=" + end.Y, accessToken) + "\n");
                }
            }
        }

        private static async Task<object> GetResults(string url, string accessToken)
        {
            using (HttpClient client = new HttpClient())
            {
                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    client.SetBearerToken(accessToken);
                }

                using (HttpResponseMessage response = await client.GetAsync(url))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return response.StatusCode;
                    }
                    else
                    {
                        return JArray.Parse(await response.Content.ReadAsStringAsync());
                    }
                }
            }
        }
    }
}
