using System.Threading.Tasks;
using IdentityServer4.Stores;
using IdentityServer4.Models;

namespace Identity
{
    public static class Extensions
    {
        public static async Task<bool> IsPkceClientAsync(this IClientStore store, string clientId)
        {
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                Client client = await store.FindEnabledClientByIdAsync(clientId);

                return client?.RequirePkce == true;
            }

            return false;
        }
    }
}
