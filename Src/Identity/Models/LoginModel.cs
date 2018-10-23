using System;
using System.Collections.Generic;
using System.Linq;

namespace Identity.Models
{
    public class LoginModel : CredentialsModel
    {
        public bool EnableLocalLogin
        {
            get;
            set;
        } = true;

        public IEnumerable<ProviderModel> ExternalProviders
        {
            get;
            set;
        }

        public IEnumerable<ProviderModel> VisibleExternalProviders => ExternalProviders.Where(x => !String.IsNullOrWhiteSpace(x.DisplayName));

        public bool IsExternalLoginOnly => EnableLocalLogin == false && ExternalProviders?.Count() == 1;

        public string ExternalLoginScheme => IsExternalLoginOnly ? ExternalProviders?.SingleOrDefault()?.AuthenticationScheme : null;
    }
}
