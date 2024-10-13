using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.API.Infrastructure.DTOs
{
    public class UpdateUserGroupDTO
    {
        [Required]
        public int UserGroupNo { get; set; }
        [StringLength(100)]
        public string UserGroupLocalName { get; set; }

        [StringLength(100)]
        public string UserGroupForeignName { get; set; }

        public bool? AllowAddUser { get; set; }
        public bool? AllowAddUserGroup { get; set; }
        public bool? AllowAddCustomer { get; set; }
        public bool? AllowAddProducts { get; set; }
        public bool? AllowAddOrder { get; set; }
        public bool? InactiveFlag { get; set; }
        public bool? IsAdmin { get; set; }
    }

}
