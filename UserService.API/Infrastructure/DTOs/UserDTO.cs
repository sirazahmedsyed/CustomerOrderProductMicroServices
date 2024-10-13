using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.API.Infrastructure.DTOs
{
    public class UserDTO
    {
        public int UserNo { get; set; }
        public string UserLocalName { get; set; }
        public string UserForeignName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public int UserGroupNo { get; set; }
    }

}
