using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using UserGroupService.API.Infrastructure.Entities;

namespace UserGroupService.API.Infrastructure.DBContext
{
    public class UserGroupDbContext : DbContext
    {
        public UserGroupDbContext(DbContextOptions<UserGroupDbContext> options) : base(options) { }

       // public DbSet<User> Users { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Add any additional configurations here
        }
    }
}
