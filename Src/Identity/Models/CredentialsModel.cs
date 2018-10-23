using System.ComponentModel.DataAnnotations;

namespace Identity.Models
{
    public class CredentialsModel
    {

        [Required]
        public string Username
        {
            get;
            set;
        }

        [Required]
        public string Password
        {
            get;
            set;
        }

        public bool RememberLogin
        {
            get;
            set;
        }

        public string ReturnUrl
        {
            get;
            set;
        }
    }
}
