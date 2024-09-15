using System.ComponentModel.DataAnnotations;

namespace AuthService.API.Infrastructure.DTOs
{
    public class CreateUserDto
    {
        
        [Required]
        [StringLength(256)]
        public string Username { get; set; }

        [Required]
        [StringLength(256)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string UserCode { get; set; }
    }
}

