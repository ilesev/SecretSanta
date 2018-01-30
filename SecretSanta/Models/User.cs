using System.ComponentModel.DataAnnotations;

namespace SecretSanta.Repository
{
    public class User
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

        [Required]
        public string DisplayName
        {
            get;
            set;
        }
    }
}