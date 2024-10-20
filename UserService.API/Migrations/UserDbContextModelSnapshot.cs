﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using UserService.API.Infrastructure.DBContext;

#nullable disable

namespace UserService.API.Migrations
{
    [DbContext(typeof(UserDbContext))]
    partial class UserDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("UserService.API.Infrastructure.Entities.User", b =>
                {
                    b.Property<int>("UserNo")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("user_no");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("UserNo"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("address");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("email");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("password");

                    b.Property<string>("UserForeignName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("user_foreign_name");

                    b.Property<int>("UserGroupNo")
                        .HasColumnType("integer")
                        .HasColumnName("user_group_no");

                    b.Property<string>("UserLocalName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("user_local_name");

                    b.HasKey("UserNo");

                    b.HasIndex("UserGroupNo");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("UserService.API.Infrastructure.Entities.UserGroup", b =>
                {
                    b.Property<int>("UserGroupNo")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("user_group_no");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("UserGroupNo"));

                    b.Property<short>("AllowAddCustomer")
                        .HasColumnType("int2")
                        .HasColumnName("allow_add_customer");

                    b.Property<short>("AllowAddOrder")
                        .HasColumnType("int2")
                        .HasColumnName("allow_add_order");

                    b.Property<short>("AllowAddProducts")
                        .HasColumnType("int2")
                        .HasColumnName("allow_add_products");

                    b.Property<short>("AllowAddUser")
                        .HasColumnType("int2")
                        .HasColumnName("allow_add_user");

                    b.Property<short>("AllowAddUserGroup")
                        .HasColumnType("int2")
                        .HasColumnName("allow_add_user_group");

                    b.Property<short>("InactiveFlag")
                        .HasColumnType("int2")
                        .HasColumnName("inactive_flag");

                    b.Property<short>("IsAdmin")
                        .HasColumnType("int2")
                        .HasColumnName("is_admin");

                    b.Property<string>("UserGroupForeignName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("user_group_foreign_name");

                    b.Property<string>("UserGroupLocalName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("user_group_local_name");

                    b.HasKey("UserGroupNo");

                    b.ToTable("user_groups", (string)null);
                });

            modelBuilder.Entity("UserService.API.Infrastructure.Entities.User", b =>
                {
                    b.HasOne("UserService.API.Infrastructure.Entities.UserGroup", "UserGroup")
                        .WithMany()
                        .HasForeignKey("UserGroupNo")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserGroup");
                });
#pragma warning restore 612, 618
        }
    }
}
