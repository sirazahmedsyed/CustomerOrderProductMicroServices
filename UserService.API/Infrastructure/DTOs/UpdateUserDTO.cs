using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.API.Infrastructure.DTOs
{
    public class UpdateUserDTO
    {
        [Required]
        public int UserNo { get; set; }

        [StringLength(100)]
        public string UserLocalName { get; set; }

        [StringLength(100)]
        public string UserForeignName { get; set; }

        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        public int? UserGroupNo { get; set; }
    }
}


