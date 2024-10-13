using System.ComponentModel.DataAnnotations;

namespace AuthService.API.Infrastructure.DTOs
{
    public class UpdateUserDto
    {
        public string Id { get; set; }

        [Required]
        [StringLength(256)]
        public string Username { get; set; }

        [Required]
        [StringLength(256)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string UserNo { get; set; }
    }
}

