using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.API.Infrastructure.DTOs
{
    public class CreateUserDTO
    {
        [Required]
        public int UserNo { get; set; }
        [Required]
        [StringLength(100)]
        public string UserLocalName { get; set; }

        [StringLength(100)]
        public string UserForeignName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; }

        [Required]
        public int UserGroupNo { get; set; }
    }
}
