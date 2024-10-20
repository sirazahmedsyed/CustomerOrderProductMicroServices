using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using UserGroupService.API.Infrastructure.Entities;

namespace UserGroupService.API.Infrastructure.DBContext
{
    public class UserGroupDbContext : DbContext
    {
        public UserGroupDbContext(DbContextOptions<UserGroupDbContext> options) : base(options) { }

        public DbSet<UserGroup> UserGroups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserGroup>(entity =>
            {
                entity.ToTable("user_groups");
                entity.Property(e => e.UserGroupNo).HasColumnName("user_group_no");
                entity.Property(e => e.UserGroupLocalName).HasColumnName("user_group_local_name");
                entity.Property(e => e.UserGroupForeignName).HasColumnName("user_group_foreign_name");
                entity.Property(e => e.AllowAddUser).HasColumnName("allow_add_user");
                entity.Property(e => e.AllowAddUserGroup).HasColumnName("allow_add_user_group");
                entity.Property(e => e.AllowAddCustomer).HasColumnName("allow_add_customer");
                entity.Property(e => e.AllowAddProducts).HasColumnName("allow_add_products");
                entity.Property(e => e.AllowAddOrder).HasColumnName("allow_add_order");
                entity.Property(e => e.InactiveFlag).HasColumnName("inactive_flag");
                entity.Property(e => e.IsAdmin).HasColumnName("is_admin");
            });
        }

    }
}
