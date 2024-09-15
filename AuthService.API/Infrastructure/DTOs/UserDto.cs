using System.ComponentModel.DataAnnotations;

namespace AuthService.API.Infrastructure.DTOs
{
    public class UserDto
    {
        public string Id { get; set; }

        [Required]
        [StringLength(256)]
        public string Username { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string UserCode { get; set; }
    }
}
