using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace UserService.API.Infrastructure.Entities
{
    public class UserGroup
    {
        [Key]
        public int UserGroupNo { get; set; }

        [Required]
        [StringLength(100)]
        public string UserGroupLocalName { get; set; }

        [StringLength(100)]
        public string UserGroupForeignName { get; set; }

        [Column(TypeName = "int2")]
        public bool AllowAddUser { get; set; }

        [Column(TypeName = "int2")]
        public bool AllowAddUserGroup { get; set; }

        [Column(TypeName = "int2")]
        public bool AllowAddCustomer { get; set; }

        [Column(TypeName = "int2")]
        public bool AllowAddProducts { get; set; }

        [Column(TypeName = "int2")]
        public bool AllowAddOrder { get; set; }

        [Column(TypeName = "int2")]
        public bool InactiveFlag { get; set; }

        [Column(TypeName = "int2")]
        public bool IsAdmin { get; set; }
    }
}
