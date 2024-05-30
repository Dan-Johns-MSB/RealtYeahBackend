using System.ComponentModel.DataAnnotations;

namespace RealtYeahBackend.Models
{
    public class UserUpdateModel
    {
        [Required]
        public int? UserId { get; set; }

        [Required]
        public string Login { get; set; }

        [Required]
        public byte[] Password { get; set; }

        [Required]
        public string Role { get; set; }

        [Required]
        public int EmployeeId { get; set; }
    }
}
