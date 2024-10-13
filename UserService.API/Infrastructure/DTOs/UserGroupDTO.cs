using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.API.Infrastructure.DTOs
{
    public class UserGroupDTO
    {
        public int UserGroupNo { get; set; }
        public string UserGroupLocalName { get; set; }
        public string UserGroupForeignName { get; set; }
        public bool AllowAddUser { get; set; }
        public bool AllowAddUserGroup { get; set; }
        public bool AllowAddCustomer { get; set; }
        public bool AllowAddProducts { get; set; }
        public bool AllowAddOrder { get; set; }
        public bool InactiveFlag { get; set; }
        public bool IsAdmin { get; set; }
    }
}
