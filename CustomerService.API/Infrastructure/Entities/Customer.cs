using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerService.API.Infrastructure.Entities
{
    public class Customer
    {
       // [Key]
        public Guid CustomerId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        [Phone]
        [StringLength(12)]
        public string PhoneNumber { get; set; }

        [Column(TypeName = "int2")]
        public bool InactiveFlag { get; set; }
    }
}
