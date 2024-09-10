using System.ComponentModel.DataAnnotations;

namespace AuthMicroservice.Infrastructure.DTOs
{
    public class LoginDto
    {
        [Required]
        [StringLength(256)]
        public string Username { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }
    }
}

