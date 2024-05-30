using System.ComponentModel.DataAnnotations;

namespace RealtYeahBackend.Models
{
    public class AuthenticationModel
    {
        [Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }
    }
}