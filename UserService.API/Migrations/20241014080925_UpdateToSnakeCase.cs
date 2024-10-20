using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserGroups_UserGroupNo",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserGroups",
                table: "UserGroups");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "UserGroups",
                newName: "user_groups");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "users",
                newName: "password");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "users",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "users",
                newName: "address");

            migrationBuilder.RenameColumn(
                name: "UserLocalName",
                table: "users",
                newName: "user_local_name");

            migrationBuilder.RenameColumn(
                name: "UserGroupNo",
                table: "users",
                newName: "user_group_no");

            migrationBuilder.RenameColumn(
                name: "UserForeignName",
                table: "users",
                newName: "user_foreign_name");

            migrationBuilder.RenameColumn(
                name: "UserNo",
                table: "users",
                newName: "user_no");

            migrationBuilder.RenameIndex(
                name: "IX_Users_UserGroupNo",
                table: "users",
                newName: "IX_users_user_group_no");

            migrationBuilder.RenameColumn(
                name: "UserGroupLocalName",
                table: "user_groups",
                newName: "user_group_local_name");

            migrationBuilder.RenameColumn(
                name: "UserGroupForeignName",
                table: "user_groups",
                newName: "user_group_foreign_name");

            migrationBuilder.RenameColumn(
                name: "IsAdmin",
                table: "user_groups",
                newName: "is_admin");

            migrationBuilder.RenameColumn(
                name: "InactiveFlag",
                table: "user_groups",
                newName: "inactive_flag");

            migrationBuilder.RenameColumn(
                name: "AllowAddUserGroup",
                table: "user_groups",
                newName: "allow_add_user_group");

            migrationBuilder.RenameColumn(
                name: "AllowAddUser",
                table: "user_groups",
                newName: "allow_add_user");

            migrationBuilder.RenameColumn(
                name: "AllowAddProducts",
                table: "user_groups",
                newName: "allow_add_products");

            migrationBuilder.RenameColumn(
                name: "AllowAddOrder",
                table: "user_groups",
                newName: "allow_add_order");

            migrationBuilder.RenameColumn(
                name: "AllowAddCustomer",
                table: "user_groups",
                newName: "allow_add_customer");

            migrationBuilder.RenameColumn(
                name: "UserGroupNo",
                table: "user_groups",
                newName: "user_group_no");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "user_no");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_groups",
                table: "user_groups",
                column: "user_group_no");

            migrationBuilder.AddForeignKey(
                name: "FK_users_user_groups_user_group_no",
                table: "users",
                column: "user_group_no",
                principalTable: "user_groups",
                principalColumn: "user_group_no",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_user_groups_user_group_no",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_groups",
                table: "user_groups");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "user_groups",
                newName: "UserGroups");

            migrationBuilder.RenameColumn(
                name: "password",
                table: "Users",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Users",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "address",
                table: "Users",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "user_local_name",
                table: "Users",
                newName: "UserLocalName");

            migrationBuilder.RenameColumn(
                name: "user_group_no",
                table: "Users",
                newName: "UserGroupNo");

            migrationBuilder.RenameColumn(
                name: "user_foreign_name",
                table: "Users",
                newName: "UserForeignName");

            migrationBuilder.RenameColumn(
                name: "user_no",
                table: "Users",
                newName: "UserNo");

            migrationBuilder.RenameIndex(
                name: "IX_users_user_group_no",
                table: "Users",
                newName: "IX_Users_UserGroupNo");

            migrationBuilder.RenameColumn(
                name: "user_group_local_name",
                table: "UserGroups",
                newName: "UserGroupLocalName");

            migrationBuilder.RenameColumn(
                name: "user_group_foreign_name",
                table: "UserGroups",
                newName: "UserGroupForeignName");

            migrationBuilder.RenameColumn(
                name: "is_admin",
                table: "UserGroups",
                newName: "IsAdmin");

            migrationBuilder.RenameColumn(
                name: "inactive_flag",
                table: "UserGroups",
                newName: "InactiveFlag");

            migrationBuilder.RenameColumn(
                name: "allow_add_user_group",
                table: "UserGroups",
                newName: "AllowAddUserGroup");

            migrationBuilder.RenameColumn(
                name: "allow_add_user",
                table: "UserGroups",
                newName: "AllowAddUser");

            migrationBuilder.RenameColumn(
                name: "allow_add_products",
                table: "UserGroups",
                newName: "AllowAddProducts");

            migrationBuilder.RenameColumn(
                name: "allow_add_order",
                table: "UserGroups",
                newName: "AllowAddOrder");

            migrationBuilder.RenameColumn(
                name: "allow_add_customer",
                table: "UserGroups",
                newName: "AllowAddCustomer");

            migrationBuilder.RenameColumn(
                name: "user_group_no",
                table: "UserGroups",
                newName: "UserGroupNo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "UserNo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserGroups",
                table: "UserGroups",
                column: "UserGroupNo");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserGroups_UserGroupNo",
                table: "Users",
                column: "UserGroupNo",
                principalTable: "UserGroups",
                principalColumn: "UserGroupNo",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
