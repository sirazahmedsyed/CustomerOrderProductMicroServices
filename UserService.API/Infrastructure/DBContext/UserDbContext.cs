using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using UserService.API.Infrastructure.Entities;

namespace UserService.API.Infrastructure.DBContext
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.Property(e => e.UserNo).HasColumnName("user_no");
                entity.Property(e => e.UserLocalName).HasColumnName("user_local_name");
                entity.Property(e => e.UserForeignName).HasColumnName("user_foreign_name");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.Address).HasColumnName("address");
                entity.Property(e => e.Password).HasColumnName("password");
                entity.Property(e => e.UserGroupNo).HasColumnName("user_group_no");
            });

            //modelBuilder.Entity<UserGroup>(entity =>
            //{
            //    entity.ToTable("user_groups");

            //    entity.Property(e => e.UserGroupNo).HasColumnName("user_group_no");
            //    entity.Property(e => e.UserGroupLocalName).HasColumnName("user_group_local_name");
            //    entity.Property(e => e.UserGroupForeignName).HasColumnName("user_group_foreign_name");
            //    entity.Property(e => e.AllowAddUser).HasColumnName("allow_add_user");
            //    entity.Property(e => e.AllowAddUserGroup).HasColumnName("allow_add_user_group");
            //    entity.Property(e => e.AllowAddCustomer).HasColumnName("allow_add_customer");
            //    entity.Property(e => e.AllowAddProducts).HasColumnName("allow_add_products");
            //    entity.Property(e => e.AllowAddOrder).HasColumnName("allow_add_order");
            //    entity.Property(e => e.InactiveFlag).HasColumnName("inactive_flag");
            //    entity.Property(e => e.IsAdmin).HasColumnName("is_admin");
            //});
        }
    }
}
