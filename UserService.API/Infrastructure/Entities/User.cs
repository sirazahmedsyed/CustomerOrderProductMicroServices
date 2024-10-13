using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace UserService.API.Infrastructure.Entities
{
    public class User
    {
        [Key]
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

        [ForeignKey("UserGroup")]
        public int UserGroupNo { get; set; }

        [ForeignKey("UserGroupNo")]
        public UserGroup UserGroup { get; set; }
    }
}
