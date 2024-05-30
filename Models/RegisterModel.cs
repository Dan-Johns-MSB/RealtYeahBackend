using System.ComponentModel.DataAnnotations;

namespace RealtYeahBackend.Models
{
    public class RegisterModel
    {
        [Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public int EmployeeId { get; set; }
    }
}