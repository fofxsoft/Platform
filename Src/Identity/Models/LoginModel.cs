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

        public IEnumerable<LoginProviderModel> LoginProviders
        {
            get;
            set;
        }

        public IEnumerable<LoginProviderModel> VisibleLoginProviders => LoginProviders.Where(x => !String.IsNullOrWhiteSpace(x.DisplayName));

        public bool IsExternalLoginOnly => EnableLocalLogin == false && LoginProviders?.Count() == 1;

        public string ExternalLoginScheme => IsExternalLoginOnly ? LoginProviders?.SingleOrDefault()?.AuthenticationScheme : null;
    }
}
