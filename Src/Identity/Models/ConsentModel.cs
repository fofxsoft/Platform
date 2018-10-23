using System.Collections.Generic;

namespace Identity.Models
{
    public class ConsentModel : ResourceModel
    {
        public string ClientName
        {
            get;
            set;
        }

        public string ClientUrl
        {
            get;
            set;
        }

        public string ClientLogoUrl
        {
            get;
            set;
        }

        public IEnumerable<ScopeModel> IdentityScopes
        {
            get;
            set;
        }

        public IEnumerable<ScopeModel> ResourceScopes
        {
            get;
            set;
        }
    }
}
